namespace MacacosMeMordam.Gameplay
{
    /// <summary>Explicit command and read model for one voyage simulation.</summary>
    public interface IVoyageSimulation
    {
        /// <summary>Gets the current immutable voyage state.</summary>
        VoyageSnapshot Snapshot { get; }

        /// <summary>Advances active time to the next state or event.</summary>
        VoyageSnapshot Advance(double activeDeltaSeconds);

        /// <summary>Applies one touch to the nearest active monkey containing the point.</summary>
        bool TryHitAt(Point2 point, out long hitSpawnOrder, out int remainingHits, out bool defeated);
    }
}
