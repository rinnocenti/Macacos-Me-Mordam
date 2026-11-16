using System;
using Immersive.Logging.Loggers;
using Immersive.Logging.Records;
using Immersive.Logging.Unity;
using MacacosMeMordam.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MacacosMeMordam.Unity
{
    /// <summary>
    /// Converts an overlay Canvas click into a board-local gameplay point.
    /// Source: design/gdd/voyage-system.md.
    /// </summary>
    public sealed class CampaignTouchBoard : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("Opcional. Reutilize a Logging Config da campanha para habilitar diagnósticos Debug.")]
        [SerializeField] private LoggingConfigAsset loggingConfig;
        [SerializeField] private RectTransform boardRect;
        private ScopedLogger _logger;

        /// <summary>Raised with a board-local point after a successful overlay click conversion.</summary>
        public event Action<Point2> PointClicked;

        /// <summary>Gets the RectTransform used to convert and present board-local points.</summary>
        public RectTransform BoardRect => boardRect;

        private void Awake()
        {
            _logger = UnityLoggingFactory
                .CreateLogger(loggingConfig)
                .For<CampaignTouchBoard>("MacacosMeMordam.Campaign.Input", gameObject.name);

            if (!TryValidate(out string issue))
            {
                _logger.Error(
                    "Campaign touch board is not configured.",
                    LogFields.Field("reason", issue));
                enabled = false;
            }
        }

        /// <summary>Validates the explicit board rectangle and its Screen Space Overlay Canvas.</summary>
        public bool TryValidate(out string issue)
        {
            if (boardRect == null)
            {
                issue = "Campaign Touch Board requires an explicit Board Rect.";
                return false;
            }

            Canvas canvas = boardRect.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                issue = "Campaign Touch Board requires a Screen Space Overlay Canvas.";
                return false;
            }

            issue = string.Empty;
            return true;
        }

        /// <summary>Forwards one screen click as a local point; gameplay chooses the target.</summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData == null || boardRect == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    boardRect,
                    eventData.position,
                    null,
                    out Vector2 localPoint))
            {
                _logger?.Debug(
                    "Touch coordinate conversion failed.",
                    LogFields.Field("screenX", eventData.position.x),
                    LogFields.Field("screenY", eventData.position.y));
                return;
            }

            Vector2 centeredPoint = localPoint - boardRect.rect.center;
            PointClicked?.Invoke(new Point2(centeredPoint.x, centeredPoint.y));
        }
    }
}
