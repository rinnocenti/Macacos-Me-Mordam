using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MacacosMeMordam.Gameplay
{
    /// <summary>
    /// Validated duration and ordered spawn schedule for one voyage.
    /// Source: design/gdd/voyage-system.md.
    /// </summary>
    public sealed class VoyageDefinition
    {
        private VoyageDefinition(
            double durationSeconds,
            ReadOnlyCollection<SpawnWaveDefinition> spawnWaves)
        {
            DurationSeconds = durationSeconds;
            SpawnWaves = spawnWaves;
        }

        /// <summary>Gets the active time required to reach the island.</summary>
        public double DurationSeconds { get; }

        /// <summary>Gets the validated, chronological spawn schedule.</summary>
        public IReadOnlyList<SpawnWaveDefinition> SpawnWaves { get; }

        /// <summary>Creates a voyage and rejects unordered or invalid schedule data.</summary>
        public static bool TryCreate(
            double durationSeconds,
            IReadOnlyList<SpawnWaveDefinition> spawnWaves,
            out VoyageDefinition voyage,
            out string issue)
        {
            if (!Point2.IsFiniteValue(durationSeconds) || durationSeconds <= 0d)
            {
                voyage = null;
                issue = "Voyage duration must be finite and positive.";
                return false;
            }

            if (spawnWaves == null)
            {
                voyage = null;
                issue = "Voyage spawn schedule is required; use an empty collection for a voyage without waves.";
                return false;
            }

            var copiedWaves = new SpawnWaveDefinition[spawnWaves.Count];
            for (int index = 0; index < spawnWaves.Count; index++)
            {
                SpawnWaveDefinition current = spawnWaves[index];
                if (current == null)
                {
                    voyage = null;
                    issue = $"Voyage spawn wave '{index}' is missing.";
                    return false;
                }

                if (index > 0)
                {
                    SpawnWaveDefinition previous = spawnWaves[index - 1];
                    if (current.Progress < previous.Progress ||
                        (current.Progress == previous.Progress && current.Order <= previous.Order))
                    {
                        voyage = null;
                        issue = "Voyage waves must be ordered by progress, then by increasing order at equal progress.";
                        return false;
                    }
                }

                copiedWaves[index] = current;
            }

            voyage = new VoyageDefinition(durationSeconds, Array.AsReadOnly(copiedWaves));
            issue = string.Empty;
            return true;
        }
    }
}
