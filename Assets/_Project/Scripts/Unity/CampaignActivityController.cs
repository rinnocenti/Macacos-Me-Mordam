using System;
using Immersive.Framework.ActivityRestart;
using Immersive.Framework.Pause;
using Immersive.Framework.Reset;
using Immersive.Framework.Reset.Unity;
using Immersive.Logging.Loggers;
using Immersive.Logging.Records;
using Immersive.Logging.Unity;
using MacacosMeMordam.Gameplay;
using UnityEngine;

namespace MacacosMeMordam.Unity
{
    /// <summary>
    /// Scene-local adapter that advances campaign rules and forwards touch commands.
    /// Source: design/gdd/voyage-system.md.
    /// </summary>
    [RequireComponent(typeof(PauseRequestTrigger), typeof(ActivityRestartTrigger), typeof(Resettable))]
    public sealed class CampaignActivityController :
        MonoBehaviour,
        IUnityResettable,
        IUnityResettableMetadata
    {
        [Tooltip("Opcional. Sem configuração, o Immersive Logging registra eventos Info e superiores.")]
        [SerializeField] private LoggingConfigAsset loggingConfig;
        [SerializeField] private CampaignDefinitionAsset campaignDefinition;
        [SerializeField] private CampaignTouchBoard touchBoard;

        private CampaignSession _session;
        private CampaignDiagnostics _diagnostics;
        private ScopedLogger _logger;
        private string _sessionId;
        private PauseRequestTrigger _pauseRequestTrigger;
        private ActivityRestartTrigger _activityRestartTrigger;
        private Resettable _resettable;
        private bool _retryRestartBlocked;
        private bool _retryResetApplied;
        private bool _pauseSnapshotUnavailableReported;

        /// <summary>Raised after an active frame or accepted campaign command updates the projection.</summary>
        public event Action<CampaignSnapshot> SnapshotChanged;

        /// <summary>Raised when a Framework Activity Restart starts or finishes successfully.</summary>
        public event Action<bool> RetryRestartBlockedChanged;

        /// <summary>Gets whether a validated campaign session is available.</summary>
        public bool IsReady => _session != null;

        /// <summary>Gets whether retry is pending or blocked by a failed Framework restart.</summary>
        public bool IsRetryRestartBlocked => _retryRestartBlocked;

        internal string SessionId => _sessionId ?? string.Empty;

        /// <summary>Gets the board rectangle shared with touch and monkey presentation.</summary>
        public RectTransform BoardRect =>
            touchBoard != null
                ? touchBoard.BoardRect
                : throw new InvalidOperationException("Campaign Activity Controller has no Campaign Touch Board.");

        /// <summary>Gets the current campaign projection or fails when initialization was blocked.</summary>
        public CampaignSnapshot Snapshot =>
            _session != null
                ? _session.Snapshot
                : throw new InvalidOperationException("Campaign Activity Controller has no valid campaign session.");

        private void Awake()
        {
            _logger = UnityLoggingFactory
                .CreateLogger(loggingConfig)
                .For<CampaignActivityController>("MacacosMeMordam.Campaign", gameObject.name);
            _sessionId = Guid.NewGuid().ToString("N");
            _diagnostics = new CampaignDiagnostics(_logger, _sessionId);

            _pauseRequestTrigger = GetComponent<PauseRequestTrigger>();
            _activityRestartTrigger = GetComponent<ActivityRestartTrigger>();
            _resettable = GetComponent<Resettable>();

            if (_pauseRequestTrigger == null ||
                _activityRestartTrigger == null ||
                _resettable == null)
            {
                Block("Campaign Activity Controller requires Pause Request, Activity Restart and Resettable components on the same GameObject.");
                return;
            }

            if (_resettable.Membership != ResetMembership.Activity)
            {
                Block("Campaign Resettable membership must be Activity so the Framework can preserve campaign progress while retrying.");
                return;
            }

            if (_activityRestartTrigger.ResetTarget.Kind != ResetTargetKind.CurrentActivity)
            {
                Block("Campaign Activity Restart Trigger must target Current Activity.");
                return;
            }

            if (touchBoard == null)
            {
                Block("Campaign Activity Controller requires an explicit Campaign Touch Board.");
                return;
            }

            if (!touchBoard.TryValidate(out string touchBoardIssue))
            {
                Block($"Campaign Touch Board is invalid. {touchBoardIssue}");
                return;
            }

            if (campaignDefinition == null)
            {
                Block("Campaign Activity Controller requires an explicit Campaign Definition asset.");
                return;
            }

            if (!campaignDefinition.TryBuildDefinition(
                    out CampaignDefinition definition,
                    out int resolvedSeed,
                    out string definitionIssue))
            {
                Block($"Campaign Definition is invalid. {definitionIssue}");
                return;
            }

            if (!CampaignSession.TryCreate(definition, out _session, out string sessionIssue))
            {
                Block($"Campaign session could not be created. {sessionIssue}");
                return;
            }

            if (!_session.TryStart(out string startIssue))
            {
                _session = null;
                Block($"Campaign session could not start. {startIssue}");
                return;
            }

            _diagnostics.Start(_session.Snapshot, resolvedSeed, definition.Voyages.Count);
        }

        private void OnEnable()
        {
            if (_session != null && touchBoard != null)
            {
                touchBoard.PointClicked += OnPointClicked;
            }
        }

        private void OnDisable()
        {
            if (touchBoard != null)
            {
                touchBoard.PointClicked -= OnPointClicked;
            }
        }

        private void OnDestroy()
        {
            OnDisable();
        }

        private void Update()
        {
            if (_session == null ||
                _retryRestartBlocked ||
                _session.Snapshot.State != CampaignState.Sailing ||
                !IsFrameworkGameplayRunning())
            {
                return;
            }

            Publish(_session.Advance(Time.deltaTime));
        }

        private void OnApplicationPause(bool paused)
        {
            _diagnostics?.RecordApplicationPause(paused);
            if (paused)
            {
                RequestPause();
            }
        }

        /// <summary>Requests logical Pause from the Framework for UI or mobile lifecycle integration.</summary>
        public void RequestPause()
        {
            if (_pauseRequestTrigger == null)
            {
                Block("Campaign Pause Request Trigger is unavailable.");
                return;
            }

            _pauseRequestTrigger.RequestPause();
            if (_pauseRequestTrigger.LastRequestFailed)
            {
                _logger?.Error(
                    "Campaign Pause request failed.",
                    LogFields.Field("sessionId", SessionId),
                    LogFields.Field("reason", _pauseRequestTrigger.LastMessage));
            }
        }

        /// <summary>Requests logical Resume from the Framework for the pause surface.</summary>
        public void RequestResume()
        {
            if (_pauseRequestTrigger == null)
            {
                Block("Campaign Pause Request Trigger is unavailable.");
                return;
            }

            _pauseRequestTrigger.RequestResume();
            if (_pauseRequestTrigger.LastRequestFailed)
            {
                _logger?.Error(
                    "Campaign Resume request failed.",
                    LogFields.Field("sessionId", SessionId),
                    LogFields.Field("reason", _pauseRequestTrigger.LastMessage));
            }
        }

        /// <summary>Requests the configured Activity Restart flow to retry the current voyage.</summary>
        public async void RequestRetry()
        {
            if (!CanAcceptCampaignCommand(out string rejectionReason) ||
                _session.Snapshot.State != CampaignState.ReadyForRetry)
            {
                _logger?.Debug(
                    "Retry command rejected.",
                    LogFields.Field("sessionId", SessionId),
                    LogFields.Field("reason", string.IsNullOrEmpty(rejectionReason)
                        ? "CampaignNotReadyForRetry"
                        : rejectionReason));
                return;
            }

            SetRetryRestartBlocked(true);
            _retryResetApplied = false;

            ActivityRestartResult result;
            try
            {
                result = await _activityRestartTrigger.RequestActivityRestartAsync();
            }
            catch (Exception exception)
            {
                LogRetryRestartFailure(null, exception.Message);
                return;
            }

            if (result == null ||
                (!result.Succeeded && !result.CompletedWithWarnings) ||
                !_retryResetApplied)
            {
                string reason = result == null
                    ? "Activity Restart returned no result."
                    : result.Failed
                        ? result.Message
                        : "Activity Restart completed without applying the required Campaign Reset participant.";
                LogRetryRestartFailure(result, reason);
                return;
            }

            _retryResetApplied = false;
            SetRetryRestartBlocked(false);
            Publish(_session.Snapshot);
        }

        /// <summary>Requests the next scheduled voyage through the campaign rules.</summary>
        public bool TryContinue()
        {
            if (!CanAcceptCampaignCommand(out string rejectionReason))
            {
                _logger?.Debug(
                    "Continue command rejected.",
                    LogFields.Field("sessionId", SessionId),
                    LogFields.Field("reason", rejectionReason));
                return false;
            }

            if (!_session.TryContinue(out string issue))
            {
                _logger?.Debug(
                    "Continue command rejected.",
                    LogFields.Field("sessionId", SessionId),
                    LogFields.Field("reason", issue));
                return false;
            }

            Publish(_session.Snapshot);
            return true;
        }

        ResetParticipantResult IUnityResettable.Reset(ResetContext context)
        {
            if (!context.IsValid)
            {
                throw new ArgumentException("Campaign Reset requires a valid Framework Reset context.", nameof(context));
            }

            if (!_retryRestartBlocked)
            {
                return ResetParticipantResult.CreateFailed(
                    context.Participant,
                    1,
                    nameof(CampaignActivityController),
                    context.Reason,
                    "Campaign Reset was requested without an active retry operation.");
            }

            if (_session == null)
            {
                return ResetParticipantResult.CreateFailed(
                    context.Participant,
                    1,
                    nameof(CampaignActivityController),
                    context.Reason,
                    "Campaign session is unavailable during Activity Restart.");
            }

            if (!_session.TryRetry(out string issue))
            {
                return ResetParticipantResult.CreateFailed(
                    context.Participant,
                    1,
                    nameof(CampaignActivityController),
                    context.Reason,
                    issue);
            }

            _retryResetApplied = true;
            return ResetParticipantResult.CreateSucceeded(
                context.Participant,
                nameof(CampaignActivityController),
                context.Reason,
                "The failed voyage was reset while campaign relics and schedule index were preserved.");
        }

        ResetParticipantRequiredness IUnityResettableMetadata.ResetRequiredness =>
            ResetParticipantRequiredness.Required;

        int IUnityResettableMetadata.ResetOrder => 0;

        string IUnityResettableMetadata.ResetDisplayName => nameof(CampaignActivityController);

        private void OnPointClicked(Point2 point)
        {
            if (_session == null ||
                _retryRestartBlocked ||
                _session.Snapshot.State != CampaignState.Sailing ||
                !IsFrameworkGameplayRunning())
            {
                return;
            }

            bool hit = _session.TryHitAt(
                point,
                out long spawnOrder,
                out int remainingHits,
                out bool defeated);
            _diagnostics.RecordTouch(hit, spawnOrder, remainingHits, defeated);
            if (hit)
            {
                Publish(_session.Snapshot);
            }
        }

        private bool CanAcceptCampaignCommand(out string rejectionReason)
        {
            if (_session == null)
            {
                rejectionReason = "SessionUnavailable";
                return false;
            }

            if (_retryRestartBlocked)
            {
                rejectionReason = "ActivityRestartPendingOrBlocked";
                return false;
            }

            if (!TryGetFrameworkPauseSnapshot(out PauseSnapshot snapshot))
            {
                rejectionReason = "FrameworkPauseSnapshotUnavailable";
                return false;
            }

            if (!snapshot.IsRunning)
            {
                rejectionReason = "FrameworkPaused";
                return false;
            }

            rejectionReason = string.Empty;
            return true;
        }

        private bool IsFrameworkGameplayRunning()
        {
            return TryGetFrameworkPauseSnapshot(out PauseSnapshot snapshot) && snapshot.IsRunning;
        }

        private bool TryGetFrameworkPauseSnapshot(out PauseSnapshot snapshot)
        {
            if (_pauseRequestTrigger != null &&
                _pauseRequestTrigger.TryGetPauseSnapshot(out snapshot) &&
                snapshot.IsValid)
            {
                _pauseSnapshotUnavailableReported = false;
                return true;
            }

            snapshot = default;
            if (!_pauseSnapshotUnavailableReported)
            {
                _logger?.Warning(
                    "Campaign gameplay is waiting for a valid Framework Pause snapshot during Activity entry.",
                    LogFields.Field("sessionId", SessionId),
                    LogFields.Field("bindingStatus", _pauseRequestTrigger != null
                        ? _pauseRequestTrigger.ProductRequestBindingStatus
                        : "Missing"));
                _pauseSnapshotUnavailableReported = true;
            }

            return false;
        }

        private void SetRetryRestartBlocked(bool blocked)
        {
            if (_retryRestartBlocked == blocked)
            {
                return;
            }

            _retryRestartBlocked = blocked;
            RetryRestartBlockedChanged?.Invoke(blocked);
        }

        private void LogRetryRestartFailure(ActivityRestartResult result, string reason)
        {
            _logger?.Error(
                "Campaign retry remains blocked after Framework Activity Restart failure.",
                LogFields.Field("sessionId", SessionId),
                LogFields.Field("retryResetApplied", _retryResetApplied),
                LogFields.Field("restartStatus", result != null ? result.Status.ToString() : "Unavailable"),
                LogFields.Field("resetStatus", result != null ? result.ResetStatus : "None"),
                LogFields.Field("clearStatus", result != null ? result.ClearStatus : "None"),
                LogFields.Field("reenterStatus", result != null ? result.ReenterStatus : "None"),
                LogFields.Field("reason", reason));
        }

        private void Publish(CampaignSnapshot snapshot)
        {
            _diagnostics?.Observe(snapshot);
            SnapshotChanged?.Invoke(snapshot);
        }

        private void Block(string issue)
        {
            _logger?.Error(
                "Campaign session blocked during initialization.",
                LogFields.Field("sessionId", SessionId),
                LogFields.Field("reason", issue));
            enabled = false;
        }
    }
}
