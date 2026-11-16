using System;
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
    public sealed class CampaignActivityController : MonoBehaviour
    {
        [Tooltip("Opcional. Sem configuração, o Immersive Logging registra eventos Info e superiores.")]
        [SerializeField] private LoggingConfigAsset loggingConfig;
        [SerializeField] private CampaignDefinitionAsset campaignDefinition;
        [SerializeField] private CampaignTouchBoard touchBoard;

        private CampaignSession _session;
        private CampaignDiagnostics _diagnostics;
        private ScopedLogger _logger;
        private string _sessionId;
        private bool _applicationPaused;

        /// <summary>Raised after an active frame or accepted campaign command updates the projection.</summary>
        public event Action<CampaignSnapshot> SnapshotChanged;

        /// <summary>Gets whether a validated campaign session is available.</summary>
        public bool IsReady => _session != null;

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
                _applicationPaused ||
                _session.Snapshot.State != CampaignState.Sailing)
            {
                return;
            }

            Publish(_session.Advance(Time.deltaTime));
        }

        private void OnApplicationPause(bool paused)
        {
            _applicationPaused = paused;
            _diagnostics?.RecordApplicationPause(paused);
        }

        /// <summary>Requests a retry through the campaign rules.</summary>
        public bool TryRetry()
        {
            if (_session == null)
            {
                _logger?.Debug(
                    "Retry command rejected.",
                    LogFields.Field("sessionId", SessionId),
                    LogFields.Field("reason", "SessionUnavailable"));
                return false;
            }

            if (!_session.TryRetry(out string issue))
            {
                _logger?.Debug(
                    "Retry command rejected.",
                    LogFields.Field("sessionId", SessionId),
                    LogFields.Field("reason", issue));
                return false;
            }

            Publish(_session.Snapshot);
            return true;
        }

        /// <summary>Requests the next scheduled voyage through the campaign rules.</summary>
        public bool TryContinue()
        {
            if (_session == null)
            {
                _logger?.Debug(
                    "Continue command rejected.",
                    LogFields.Field("sessionId", SessionId),
                    LogFields.Field("reason", "SessionUnavailable"));
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

        private void OnPointClicked(Point2 point)
        {
            if (_session == null || _session.Snapshot.State != CampaignState.Sailing)
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
