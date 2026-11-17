using System;
using System.Globalization;
using Immersive.Logging.Loggers;
using Immersive.Logging.Records;
using Immersive.Logging.Unity;
using MacacosMeMordam.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace MacacosMeMordam.Unity
{
    /// <summary>
    /// Presents campaign progress and sends Retry or Continue commands to the controller.
    /// Source: design/gdd/voyage-system.md.
    /// </summary>
    public sealed class CampaignHudView : MonoBehaviour
    {
        [Tooltip("Opcional. Use uma Logging Config para habilitar mensagens Debug desta HUD.")]
        [SerializeField] private LoggingConfigAsset loggingConfig;
        [SerializeField] private CampaignActivityController controller;
        [SerializeField] private CampaignLocalizationAsset localization;
        [SerializeField] private Text statusText;
        [SerializeField] private Text relicProgressText;
        [SerializeField] private Text retryButtonText;
        [SerializeField] private Text continueButtonText;
        [SerializeField] private Image travelProgress;
        [SerializeField] private Image sinkingProgress;
        [Tooltip("Contêiner com Horizontal Layout Group para organizar os marcadores.")]
        [SerializeField] private RectTransform relicMarkersContainer;
        [Tooltip("Prefab visual usado para criar um marcador por relíquia exigida.")]
        [SerializeField] private GameObject relicMarkerPrefab;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject arrivedPanel;
        [SerializeField] private GameObject sunkPanel;
        [SerializeField] private GameObject curseBrokenPanel;

        private bool _initialized;
        private bool _subscribed;
        private string _relicProgressFormat;
        private GameObject[] _relicMarkers;
        private ScopedLogger _logger;
        private CampaignState? _lastLoggedState;

        private void Start()
        {
            _logger = UnityLoggingFactory
                .CreateLogger(loggingConfig)
                .For<CampaignHudView>("MacacosMeMordam.Campaign.Hud", gameObject.name);

            if (controller == null || !controller.IsReady)
            {
                Block("Campaign HUD View requires a ready Campaign Activity Controller.");
                return;
            }

            if (travelProgress == null ||
                sinkingProgress == null ||
                localization == null ||
                statusText == null ||
                relicProgressText == null ||
                retryButtonText == null ||
                continueButtonText == null ||
                travelProgress.type != Image.Type.Filled ||
                sinkingProgress.type != Image.Type.Filled ||
                relicMarkersContainer == null ||
                relicMarkerPrefab == null ||
                relicMarkersContainer.GetComponent<HorizontalLayoutGroup>() == null ||
                relicMarkerPrefab.GetComponent<RectTransform>() == null ||
                relicMarkerPrefab.GetComponent<Image>() == null ||
                retryButton == null ||
                continueButton == null ||
                arrivedPanel == null ||
                sunkPanel == null ||
                curseBrokenPanel == null)
            {
                Block("Campaign HUD View requires filled progress Images, a relic marker container with a Horizontal Layout Group, a relic marker prefab with RectTransform and Image, result panels and action Buttons.");
                return;
            }

            if (!TryGetText(CampaignTextKey.RelicProgress, out _relicProgressFormat) ||
                !TryGetText(CampaignTextKey.Retry, out string retryText) ||
                !TryGetText(CampaignTextKey.Continue, out string continueText) ||
                !TryGetText(CampaignTextKey.NotStarted, out _) ||
                !TryGetText(CampaignTextKey.Sailing, out _) ||
                !TryGetText(CampaignTextKey.Arrived, out _) ||
                !TryGetText(CampaignTextKey.Sunk, out _) ||
                !TryGetText(CampaignTextKey.CurseBroken, out _))
            {
                return;
            }

            retryButtonText.text = retryText;
            continueButtonText.text = continueText;

            if (!TryCreateRelicMarkers(controller.Snapshot.RequiredRelics))
            {
                return;
            }

            _initialized = true;
            Subscribe();
        }

        private bool TryCreateRelicMarkers(int requiredRelics)
        {
            if (requiredRelics < 1)
            {
                Block("Campaign HUD cannot create relic markers for an invalid relic requirement.");
                return false;
            }

            _relicMarkers = new GameObject[requiredRelics];
            for (int index = 0; index < requiredRelics; index++)
            {
                GameObject marker = Instantiate(relicMarkerPrefab, relicMarkersContainer, false);
                marker.name = $"Relic Marker {index + 1}";
                marker.SetActive(false);
                _relicMarkers[index] = marker;
            }

            return true;
        }

        private void OnEnable()
        {
            if (_initialized)
            {
                Subscribe();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_subscribed || controller == null)
            {
                return;
            }

            controller.SnapshotChanged += Render;
            controller.RetryRestartBlockedChanged += OnRetryRestartBlockedChanged;
            retryButton.onClick.AddListener(RequestRetry);
            continueButton.onClick.AddListener(RequestContinue);
            _subscribed = true;
            Render(controller.Snapshot);
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
            {
                return;
            }

            controller.SnapshotChanged -= Render;
            controller.RetryRestartBlockedChanged -= OnRetryRestartBlockedChanged;
            retryButton.onClick.RemoveListener(RequestRetry);
            continueButton.onClick.RemoveListener(RequestContinue);
            _subscribed = false;
        }

        private void Render(CampaignSnapshot campaign)
        {
            if (!campaign.Voyage.HasValue)
            {
                return;
            }

            VoyageSnapshot voyage = campaign.Voyage.Value;
            travelProgress.fillAmount = (float)voyage.TravelProgress;
            sinkingProgress.fillAmount = (float)voyage.SinkingProgress;
            if (!TryGetText(GetTextKey(campaign.State), out string status))
            {
                return;
            }

            statusText.text = status;
            try
            {
                relicProgressText.text = string.Format(
                    CultureInfo.InvariantCulture,
                    _relicProgressFormat,
                    campaign.RelicCount,
                    campaign.RequiredRelics);
            }
            catch (FormatException exception)
            {
                Block(
                    "Campaign Localization relic progress format is invalid.",
                    LogFields.Field("localeCode", localization.LocaleCode),
                    LogFields.Field("textKey", CampaignTextKey.RelicProgress.ToString()),
                    LogFields.Field("details", exception.Message));
                return;
            }

            if (_relicMarkers == null ||
                campaign.RequiredRelics != _relicMarkers.Length ||
                campaign.RelicCount > _relicMarkers.Length)
            {
                Block("Campaign HUD generated marker count does not match the campaign snapshot.");
                return;
            }

            for (int index = 0; index < _relicMarkers.Length; index++)
            {
                if (_relicMarkers[index] == null)
                {
                    Block($"Campaign HUD relic marker '{index}' is missing.");
                    return;
                }

                _relicMarkers[index].SetActive(index < campaign.RelicCount);
            }

            bool arrived = campaign.State == CampaignState.ReadyForContinue;
            bool sunk = campaign.State == CampaignState.ReadyForRetry;
            bool curseBroken = campaign.State == CampaignState.CurseBroken;
            arrivedPanel.SetActive(arrived);
            sunkPanel.SetActive(sunk);
            curseBrokenPanel.SetActive(curseBroken);
            retryButton.gameObject.SetActive(sunk);
            retryButton.interactable = sunk && !controller.IsRetryRestartBlocked;
            continueButton.gameObject.SetActive(arrived);

            if (!_lastLoggedState.HasValue || _lastLoggedState.Value != campaign.State)
            {
                _logger.Info(
                    "Campaign HUD state displayed.",
                    LogFields.Field("sessionId", controller.SessionId),
                    LogFields.Field("localeCode", localization.LocaleCode),
                    LogFields.Field("state", campaign.State.ToString()),
                    LogFields.Field("statusTextKey", GetTextKey(campaign.State).ToString()),
                    LogFields.Field("retryVisible", sunk),
                    LogFields.Field("continueVisible", arrived),
                    LogFields.Field("curseBrokenVisible", curseBroken));
                _lastLoggedState = campaign.State;
            }
        }

        private void RequestRetry() => controller.RequestRetry();

        private void RequestContinue() => controller.TryContinue();

        private void OnRetryRestartBlockedChanged(bool blocked)
        {
            if (retryButton == null || controller == null || !controller.IsReady)
            {
                return;
            }

            retryButton.interactable =
                !blocked &&
                controller.Snapshot.State == CampaignState.ReadyForRetry;
        }

        private bool TryGetText(CampaignTextKey key, out string text)
        {
            if (localization.TryGetText(key, out text, out string issue))
            {
                return true;
            }

            Block(
                "Campaign Localization entry is missing or invalid.",
                LogFields.Field("localeCode", localization.LocaleCode),
                LogFields.Field("textKey", key.ToString()),
                LogFields.Field("details", issue));
            return false;
        }

        private static CampaignTextKey GetTextKey(CampaignState state)
        {
            switch (state)
            {
                case CampaignState.NotStarted:
                    return CampaignTextKey.NotStarted;
                case CampaignState.Sailing:
                    return CampaignTextKey.Sailing;
                case CampaignState.ReadyForRetry:
                    return CampaignTextKey.Sunk;
                case CampaignState.ReadyForContinue:
                    return CampaignTextKey.Arrived;
                case CampaignState.CurseBroken:
                    return CampaignTextKey.CurseBroken;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(state), state, "Unknown campaign state.");
            }
        }

        private void Block(string issue, params LogField[] additionalFields)
        {
            if (_logger != null)
            {
                var fields = new LogField[additionalFields.Length + 1];
                fields[0] = LogFields.Field("reason", issue);
                Array.Copy(additionalFields, 0, fields, 1, additionalFields.Length);
                var contextualFields = new LogField[fields.Length + 1];
                contextualFields[0] = LogFields.Field(
                    "sessionId",
                    controller != null ? controller.SessionId : string.Empty);
                Array.Copy(fields, 0, contextualFields, 1, fields.Length);
                _logger.Error("Campaign HUD blocked during initialization or rendering.", contextualFields);
            }

            enabled = false;
        }
    }
}
