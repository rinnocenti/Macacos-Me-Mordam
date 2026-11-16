namespace MacacosMeMordam.Gameplay
{
    /// <summary>Immutable campaign progression and current voyage projection.</summary>
    public readonly struct CampaignSnapshot
    {
        internal CampaignSnapshot(
            CampaignState state,
            int relicCount,
            int requiredRelics,
            int voyageIndex,
            VoyageSnapshot? voyage)
        {
            State = state;
            RelicCount = relicCount;
            RequiredRelics = requiredRelics;
            VoyageIndex = voyageIndex;
            Voyage = voyage;
        }

        /// <summary>Gets the current campaign state.</summary>
        public CampaignState State { get; }

        /// <summary>Gets relics collected during this application session.</summary>
        public int RelicCount { get; }

        /// <summary>Gets the configured relic count required to break the curse.</summary>
        public int RequiredRelics { get; }

        /// <summary>Gets the zero-based schedule index used by the current or most recent voyage.</summary>
        public int VoyageIndex { get; }

        /// <summary>Gets the current or most recent voyage when one has started.</summary>
        public VoyageSnapshot? Voyage { get; }
    }
}
