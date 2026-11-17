namespace MacacosMeMordam.Gameplay
{
    /// <summary>Supported player interface languages.</summary>
    public enum PlayerLocale
    {
        /// <summary>Invalid or uninitialized value.</summary>
        Unspecified = 0,

        /// <summary>United States English.</summary>
        EnUs = 1,

        /// <summary>Brazilian Portuguese.</summary>
        PtBr = 2
    }

    /// <summary>Player-selected settings persisted across application launches.</summary>
    /// <remarks>Source: design/ux/campaign-flow.md.</remarks>
    public readonly struct PlayerPreferences
    {
        /// <summary>Creates a preference snapshot.</summary>
        public PlayerPreferences(
            PlayerLocale locale,
            float musicVolume,
            float effectsVolume)
        {
            Locale = locale;
            MusicVolume = musicVolume;
            EffectsVolume = effectsVolume;
        }

        /// <summary>Gets the selected interface language.</summary>
        public PlayerLocale Locale { get; }

        /// <summary>Gets the normalized music volume value in the inclusive range from 0 to 1.</summary>
        public float MusicVolume { get; }

        /// <summary>Gets the normalized sound-effects volume value in the inclusive range from 0 to 1.</summary>
        public float EffectsVolume { get; }

        /// <summary>Gets whether this snapshot contains supported, finite values.</summary>
        public bool IsValid =>
            (Locale == PlayerLocale.EnUs || Locale == PlayerLocale.PtBr)
            && IsNormalizedVolume(MusicVolume)
            && IsNormalizedVolume(EffectsVolume);

        /// <summary>Gets the defaults for a new installation.</summary>
        public static PlayerPreferences Default =>
            new PlayerPreferences(PlayerLocale.EnUs, 1f, 1f);

        private static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);

        private static bool IsNormalizedVolume(float value) =>
            IsFinite(value) && value >= 0f && value <= 1f;
    }

    /// <summary>Loads and saves player preferences without owning a global instance.</summary>
    public interface IPlayerPreferencesStore
    {
        /// <summary>Loads saved values, or defaults when no values have been saved yet.</summary>
        PlayerPreferences Load();

        /// <summary>Saves a validated preference snapshot.</summary>
        void Save(PlayerPreferences preferences);
    }
}
