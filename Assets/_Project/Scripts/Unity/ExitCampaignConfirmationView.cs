using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MacacosMeMordam.Unity
{
    /// <summary>Closes the exit confirmation without changing Framework pause state.</summary>
    public sealed class ExitCampaignConfirmationView : MonoBehaviour, ICancelHandler
    {
        [SerializeField] private Button stayButton;

        private void OnEnable()
        {
            if (stayButton == null)
            {
                throw new MissingReferenceException(
                    "Exit Campaign Confirmation View requires its Stay button.");
            }

            if (EventSystem.current == null)
            {
                throw new MissingReferenceException(
                    "Exit Campaign Confirmation View requires an active EventSystem.");
            }

            EventSystem.current.SetSelectedGameObject(stayButton.gameObject);
        }

        /// <inheritdoc />
        public void OnCancel(BaseEventData eventData)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            gameObject.SetActive(false);
            eventData.Use();
        }
    }
}
