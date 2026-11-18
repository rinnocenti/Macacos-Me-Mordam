using Immersive.Logging.Loggers;
using Immersive.Logging.Records;
using Immersive.Logging.Unity;
using MacacosMeMordam.Gameplay;
using UnityEngine;

namespace MacacosMeMordam.Unity
{
    /// <summary>Presents accumulated voyage sinking by lowering the ship around the fixed water layer.</summary>
    public sealed class ShipSinkingView : MonoBehaviour
    {
        [Tooltip("Opcional. Use a Logging Config da campanha para manter os eventos no mesmo fluxo.")]
        [SerializeField] private LoggingConfigAsset loggingConfig;
        [SerializeField] private CampaignActivityController controller;
        [SerializeField] private GameObject shipRearLayer;
        [SerializeField] private GameObject waterFrontLayer;
        [SerializeField] private GameObject shipFrontLayer;
        [SerializeField, Min(0f)] private float maximumSinkDistance = 480f;
        [SerializeField] private AnimationCurve sinkingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private ScopedLogger _logger;
        private Vector3 _shipRearRestPosition;
        private Vector3 _shipFrontRestPosition;
        private bool _initialized;
        private bool _subscribed;

        private void Awake()
        {
            _logger = UnityLoggingFactory
                .CreateLogger(loggingConfig)
                .For<ShipSinkingView>("MacacosMeMordam.Campaign.Presentation", gameObject.name);

            if (!TryValidate())
            {
                enabled = false;
                return;
            }

            _shipRearRestPosition = shipRearLayer.transform.localPosition;
            _shipFrontRestPosition = shipFrontLayer.transform.localPosition;
            _initialized = true;
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

        private void Start()
        {
            if (_initialized && controller.IsReady)
            {
                Render(controller.Snapshot);
            }
        }

        private bool TryValidate()
        {
            if (controller == null ||
                shipRearLayer == null ||
                waterFrontLayer == null ||
                shipFrontLayer == null ||
                maximumSinkDistance <= 0f ||
                sinkingCurve == null ||
                sinkingCurve.length < 2)
            {
                Block("Ship Sinking View requires a controller, three ordered visual layers, a positive sink distance and a valid curve.");
                return false;
            }

            Transform rear = shipRearLayer.transform;
            Transform water = waterFrontLayer.transform;
            Transform front = shipFrontLayer.transform;
            if (rear.parent == null ||
                rear.parent != water.parent ||
                water.parent != front.parent ||
                rear.GetSiblingIndex() >= water.GetSiblingIndex() ||
                water.GetSiblingIndex() >= front.GetSiblingIndex())
            {
                Block("Ship Sinking View layers must be siblings ordered behind, at, and in front of the fixed water layer.");
                return false;
            }

            return true;
        }

        private void Subscribe()
        {
            if (_subscribed || controller == null)
            {
                return;
            }

            controller.SnapshotChanged += Render;
            _subscribed = true;
            if (controller.IsReady)
            {
                Render(controller.Snapshot);
            }
        }

        private void Unsubscribe()
        {
            if (!_subscribed || controller == null)
            {
                return;
            }

            controller.SnapshotChanged -= Render;
            _subscribed = false;
        }

        private void Render(CampaignSnapshot campaign)
        {
            if (!campaign.Voyage.HasValue)
            {
                return;
            }

            float progress = Mathf.Clamp01((float)campaign.Voyage.Value.SinkingProgress);
            float curveProgress = Mathf.Clamp01(sinkingCurve.Evaluate(progress));
            float verticalOffset = maximumSinkDistance * curveProgress;
            shipRearLayer.transform.localPosition = _shipRearRestPosition + Vector3.down * verticalOffset;
            shipFrontLayer.transform.localPosition = _shipFrontRestPosition + Vector3.down * verticalOffset;
        }

        private void Block(string reason)
        {
            _logger?.Error(
                "Ship sinking presentation is blocked by invalid scene configuration.",
                LogFields.Field("sessionId", controller != null ? controller.SessionId : string.Empty),
                LogFields.Field("reason", reason));
        }
    }
}
