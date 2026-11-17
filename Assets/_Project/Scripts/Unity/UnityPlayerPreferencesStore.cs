using System;
using MacacosMeMordam.Gameplay;
using UnityEngine;

namespace MacacosMeMordam.Unity
{
    /// <summary>
    /// Unity PlayerPrefs adapter for player settings.
    /// This consumer-local backend is interim until Framework Preferences is available.
    /// </summary>
    public sealed class UnityPlayerPreferencesStore : IPlayerPreferencesStore
    {
        private const string LocaleKey = "MacacosMeMordam.PlayerPreferences.Locale";
        private const string MusicVolumeKey = "MacacosMeMordam.PlayerPreferences.MusicVolume";
        private const string EffectsVolumeKey = "MacacosMeMordam.PlayerPreferences.EffectsVolume";

        /// <summary>Loads the saved settings, using approved defaults when keys are absent.</summary>
        public PlayerPreferences Load()
        {
            PlayerPreferences defaults = PlayerPreferences.Default;
            int localeValue = PlayerPrefs.GetInt(LocaleKey, (int)defaults.Locale);
            PlayerLocale locale = (PlayerLocale)localeValue;
            var preferences = new PlayerPreferences(
                locale,
                PlayerPrefs.GetFloat(MusicVolumeKey, defaults.MusicVolume),
                PlayerPrefs.GetFloat(EffectsVolumeKey, defaults.EffectsVolume));

            if (!preferences.IsValid)
            {
                throw new InvalidOperationException(
                    "Stored player preferences contain an unsupported locale or non-finite volume value.");
            }

            return preferences;
        }

        /// <summary>Saves all player settings and flushes them to local storage.</summary>
        public void Save(PlayerPreferences preferences)
        {
            if (!preferences.IsValid)
            {
                throw new ArgumentException(
                    "Player preferences must contain a supported locale and finite volume values.",
                    nameof(preferences));
            }

            PlayerPrefs.SetInt(LocaleKey, (int)preferences.Locale);
            PlayerPrefs.SetFloat(MusicVolumeKey, preferences.MusicVolume);
            PlayerPrefs.SetFloat(EffectsVolumeKey, preferences.EffectsVolume);
            PlayerPrefs.Save();
        }
    }
}
