namespace MacacosMeMordam.Gameplay
{
    /// <summary>
    /// Immutable presentation data for one active monkey occurrence.
    /// Source: design/gdd/voyage-system.md.
    /// </summary>
    public readonly struct MonkeySnapshot
    {
        internal MonkeySnapshot(
            long spawnOrder,
            Point2 position,
            double weight,
            double hitRadius,
            int requiredHitsToDefeat,
            int remainingHits)
        {
            SpawnOrder = spawnOrder;
            Position = position;
            Weight = weight;
            HitRadius = hitRadius;
            RequiredHitsToDefeat = requiredHitsToDefeat;
            RemainingHits = remainingHits;
        }

        /// <summary>Gets the occurrence order used to resolve equal-distance taps.</summary>
        public long SpawnOrder { get; }

        /// <summary>Gets the board-local center.</summary>
        public Point2 Position { get; }

        /// <summary>Gets the contribution to active ship load.</summary>
        public double Weight { get; }

        /// <summary>Gets the inclusive board-local tap radius.</summary>
        public double HitRadius { get; }

        /// <summary>Gets the number of touches required to defeat this class.</summary>
        public int RequiredHitsToDefeat { get; }

        /// <summary>Gets the number of touches still needed to defeat this occurrence.</summary>
        public int RemainingHits { get; }
    }
}
