namespace MacacosMeMordam.Gameplay
{
    /// <summary>Campaign progression states after start, arrival, sinking or completion.</summary>
    public enum CampaignState
    {
        /// <summary>No voyage has started.</summary>
        NotStarted = 0,
        /// <summary>A voyage attempt is active.</summary>
        Sailing = 1,
        /// <summary>A sunk attempt can be retried.</summary>
        ReadyForRetry = 2,
        /// <summary>An arrived attempt is waiting for explicit continuation.</summary>
        ReadyForContinue = 3,
        /// <summary>The required relic count has been reached.</summary>
        CurseBroken = 4
    }
}
