namespace MacacosMeMordam.Gameplay
{
    /// <summary>
    /// Position and traits for one monkey occurrence in a spawn wave.
    /// Source: design/gdd/voyage-system.md.
    /// </summary>
    public readonly struct MonkeySpawnDefinition
    {
        private MonkeySpawnDefinition(Point2 position, MonkeyClassDefinition monkeyClass)
        {
            Position = position;
            MonkeyClass = monkeyClass;
            IsValid = true;
        }

        /// <summary>Gets the authored board-local position.</summary>
        public Point2 Position { get; }

        /// <summary>Gets the weight and hit radius used by this occurrence.</summary>
        public MonkeyClassDefinition MonkeyClass { get; }

        /// <summary>Gets whether the value was created through validation.</summary>
        public bool IsValid { get; }

        /// <summary>Creates one spawn entry from explicit, validated data.</summary>
        public static bool TryCreate(
            Point2 position,
            MonkeyClassDefinition monkeyClass,
            out MonkeySpawnDefinition spawn,
            out string issue)
        {
            if (!monkeyClass.IsValid)
            {
                spawn = default;
                issue = "Monkey spawn requires a valid class definition.";
                return false;
            }

            spawn = new MonkeySpawnDefinition(position, monkeyClass);
            issue = string.Empty;
            return true;
        }
    }
}
