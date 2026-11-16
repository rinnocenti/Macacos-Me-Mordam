namespace MacacosMeMordam.Gameplay
{
    /// <summary>Terminal and active states for one voyage attempt.</summary>
    public enum VoyageState
    {
        /// <summary>The attempt is active.</summary>
        Sailing = 0,
        /// <summary>The attempt reached the island.</summary>
        Arrived = 1,
        /// <summary>The accumulated sinking reached its limit first.</summary>
        Sunk = 2
    }
}
