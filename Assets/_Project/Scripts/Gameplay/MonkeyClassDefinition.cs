namespace MacacosMeMordam.Gameplay
{
    /// <summary>
    /// Immutable gameplay traits for one monkey class.
    /// Source: design/gdd/voyage-system.md.
    /// </summary>
    public readonly struct MonkeyClassDefinition
    {
        private MonkeyClassDefinition(double weight, double hitRadius, int requiredHitsToDefeat)
        {
            Weight = weight;
            HitRadius = hitRadius;
            RequiredHitsToDefeat = requiredHitsToDefeat;
            IsValid = true;
        }

        /// <summary>Gets the weight added to the active ship load.</summary>
        public double Weight { get; }

        /// <summary>Gets the inclusive tap radius in board-local units.</summary>
        public double HitRadius { get; }

        /// <summary>Gets the number of valid touches required to defeat this class.</summary>
        public int RequiredHitsToDefeat { get; }

        /// <summary>Gets whether the value was created through validation.</summary>
        public bool IsValid { get; }

        /// <summary>Creates validated class data without supplying implicit gameplay defaults.</summary>
        public static bool TryCreate(
            double weight,
            double hitRadius,
            int requiredHitsToDefeat,
            out MonkeyClassDefinition definition,
            out string issue)
        {
            if (!Point2.IsFiniteValue(weight) || weight <= 0d)
            {
                definition = default;
                issue = "Monkey weight must be a finite positive value.";
                return false;
            }

            if (!Point2.IsFiniteValue(hitRadius) || hitRadius <= 0d)
            {
                definition = default;
                issue = "Monkey hit radius must be a finite positive value.";
                return false;
            }

            if (requiredHitsToDefeat < 1)
            {
                definition = default;
                issue = "Monkey class must require at least one valid touch.";
                return false;
            }

            definition = new MonkeyClassDefinition(weight, hitRadius, requiredHitsToDefeat);
            issue = string.Empty;
            return true;
        }
    }
}
