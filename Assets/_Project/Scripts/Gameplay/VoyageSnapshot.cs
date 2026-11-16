using System.Collections.ObjectModel;

namespace MacacosMeMordam.Gameplay
{
    /// <summary>Immutable progress and active-load projection for a voyage attempt.</summary>
    public readonly struct VoyageSnapshot
    {
        internal VoyageSnapshot(
            VoyageState state,
            double elapsedSeconds,
            double durationSeconds,
            double sinkingProgress,
            double activeWeight,
            ReadOnlyCollection<MonkeySnapshot> activeMonkeys)
        {
            State = state;
            ElapsedSeconds = elapsedSeconds;
            DurationSeconds = durationSeconds;
            SinkingProgress = sinkingProgress;
            ActiveWeight = activeWeight;
            ActiveMonkeys = activeMonkeys;
        }

        /// <summary>Gets the current attempt state.</summary>
        public VoyageState State { get; }

        /// <summary>Gets active route time already elapsed.</summary>
        public double ElapsedSeconds { get; }

        /// <summary>Gets the configured voyage duration.</summary>
        public double DurationSeconds { get; }

        /// <summary>Gets route progress in the range zero to one.</summary>
        public double TravelProgress => ElapsedSeconds / DurationSeconds;

        /// <summary>Gets accumulated sinking progress in the range zero to one.</summary>
        public double SinkingProgress { get; }

        /// <summary>Gets the total weight of active monkeys.</summary>
        public double ActiveWeight { get; }

        /// <summary>Gets an immutable projection of active monkey occurrences.</summary>
        public ReadOnlyCollection<MonkeySnapshot> ActiveMonkeys { get; }
    }
}
