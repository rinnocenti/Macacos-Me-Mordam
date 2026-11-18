using System;
using MacacosMeMordam.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace MacacosMeMordam.Unity
{
    /// <summary>Displays the main menu labels in the player's saved interface language.</summary>
    public sealed class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Text playButtonText;
        [SerializeField] private Text settingsButtonText;

        private readonly IPlayerPreferencesStore _store = new UnityPlayerPreferencesStore();

        private void Awake()
        {
            if (playButtonText == null || settingsButtonText == null)
            {
                throw new InvalidOperationException(
                    "Main Menu View requires the Play and Settings labels.");
            }

            ApplyLocale(_store.Load().Locale);
        }

        /// <summary>Applies the supplied supported language to the main menu actions.</summary>
        public void ApplyLocale(PlayerLocale locale)
        {
            switch (locale)
            {
                case PlayerLocale.EnUs:
                    playButtonText.text = "PLAY";
                    settingsButtonText.text = "SETTINGS";
                    return;
                case PlayerLocale.PtBr:
                    playButtonText.text = "JOGAR";
                    settingsButtonText.text = "CONFIGURAÇÕES";
                    return;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(locale),
                        locale,
                        "Main Menu View only supports the authored locales.");
            }
        }
    }
}
