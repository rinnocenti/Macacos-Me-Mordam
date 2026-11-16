using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MacacosMeMordam.Gameplay
{
    /// <summary>
    /// Ordered group of monkey occurrences triggered by route progress.
    /// Source: design/gdd/voyage-system.md.
    /// </summary>
    public sealed class SpawnWaveDefinition
    {
        private SpawnWaveDefinition(
            int order,
            double progress,
            ReadOnlyCollection<MonkeySpawnDefinition> monkeys)
        {
            Order = order;
            Progress = progress;
            Monkeys = monkeys;
        }

        /// <summary>Gets the stable order among waves triggered at equal progress.</summary>
        public int Order { get; }

        /// <summary>Gets the trigger position from zero to one along the route.</summary>
        public double Progress { get; }

        /// <summary>Gets the ordered monkey occurrences spawned together.</summary>
        public IReadOnlyList<MonkeySpawnDefinition> Monkeys { get; }

        /// <summary>Creates a non-empty wave whose trigger is in the inclusive route-progress range.</summary>
        public static bool TryCreate(
            int order,
            double progress,
            IReadOnlyList<MonkeySpawnDefinition> monkeys,
            out SpawnWaveDefinition wave,
            out string issue)
        {
            if (order < 0)
            {
                wave = null;
                issue = "Spawn wave order cannot be negative.";
                return false;
            }

            if (!Point2.IsFiniteValue(progress) || progress < 0d || progress > 1d)
            {
                wave = null;
                issue = "Spawn wave progress must be finite and in the range 0..1.";
                return false;
            }

            if (monkeys == null || monkeys.Count == 0)
            {
                wave = null;
                issue = "Spawn wave must contain at least one monkey.";
                return false;
            }

            var copiedMonkeys = new MonkeySpawnDefinition[monkeys.Count];
            for (int index = 0; index < monkeys.Count; index++)
            {
                if (!monkeys[index].IsValid)
                {
                    wave = null;
                    issue = $"Spawn wave entry '{index}' is invalid.";
                    return false;
                }

                copiedMonkeys[index] = monkeys[index];
            }

            wave = new SpawnWaveDefinition(
                order,
                progress,
                Array.AsReadOnly(copiedMonkeys));
            issue = string.Empty;
            return true;
        }
    }
}
