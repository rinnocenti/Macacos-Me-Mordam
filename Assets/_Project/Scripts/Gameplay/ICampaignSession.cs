namespace MacacosMeMordam.Gameplay
{
    /// <summary>Explicit commands and read model for one in-memory campaign session.</summary>
    public interface ICampaignSession
    {
        /// <summary>Gets the immutable campaign projection.</summary>
        CampaignSnapshot Snapshot { get; }

        /// <summary>Starts the first voyage once.</summary>
        bool TryStart(out string issue);

        /// <summary>Advances the current attempt and processes its terminal result.</summary>
        CampaignSnapshot Advance(double activeDeltaSeconds);

        /// <summary>Forwards one touch to the nearest active monkey while sailing.</summary>
        bool TryHitAt(Point2 point, out long hitSpawnOrder, out int remainingHits, out bool defeated);

        /// <summary>Retries the current schedule after sinking.</summary>
        bool TryRetry(out string issue);

        /// <summary>Starts the next schedule after an arrival below the threshold.</summary>
        bool TryContinue(out string issue);
    }
}
