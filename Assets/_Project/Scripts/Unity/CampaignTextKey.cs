namespace MacacosMeMordam.Unity
{
    /// <summary>Localized copy keys consumed by the campaign HUD.</summary>
    public enum CampaignTextKey
    {
        /// <summary>Campaign session has not started.</summary>
        NotStarted = 0,
        /// <summary>The ship is sailing toward the island.</summary>
        Sailing = 1,
        /// <summary>The ship reached the island.</summary>
        Arrived = 2,
        /// <summary>The ship sank before reaching the island.</summary>
        Sunk = 3,
        /// <summary>The required relic count broke the curse.</summary>
        CurseBroken = 4,
        /// <summary>Retry action label.</summary>
        Retry = 5,
        /// <summary>Continue action label.</summary>
        Continue = 6,
        /// <summary>Relic count format with current and required counts as arguments.</summary>
        RelicProgress = 7
    }
}
