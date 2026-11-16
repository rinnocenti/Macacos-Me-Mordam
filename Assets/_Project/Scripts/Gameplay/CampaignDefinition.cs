using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MacacosMeMordam.Gameplay
{
    /// <summary>
    /// Validated campaign tuning and ordered voyage schedules.
    /// Source: design/gdd/voyage-system.md.
    /// </summary>
    public sealed class CampaignDefinition
    {
        private CampaignDefinition(
            double sinkingRate,
            int requiredRelics,
            ReadOnlyCollection<VoyageDefinition> voyages)
        {
            SinkingRate = sinkingRate;
            RequiredRelics = requiredRelics;
            Voyages = voyages;
        }

        /// <summary>Gets sinking progress per active weight and second.</summary>
        public double SinkingRate { get; }

        /// <summary>Gets the relic count required to break the curse.</summary>
        public int RequiredRelics { get; }

        /// <summary>Gets the ordered schedules used by campaign successes and retries.</summary>
        public IReadOnlyList<VoyageDefinition> Voyages { get; }

        /// <summary>Creates campaign data and requires one voyage schedule per possible relic.</summary>
        public static bool TryCreate(
            IReadOnlyList<VoyageDefinition> voyages,
            double sinkingRate,
            int requiredRelics,
            out CampaignDefinition campaign,
            out string issue)
        {
            if (!Point2.IsFiniteValue(sinkingRate) || sinkingRate <= 0d)
            {
                campaign = null;
                issue = "Sinking rate must be finite and positive.";
                return false;
            }

            if (requiredRelics <= 0)
            {
                campaign = null;
                issue = "Required relic count must be positive.";
                return false;
            }

            if (voyages == null || voyages.Count < requiredRelics)
            {
                campaign = null;
                issue = "Campaign requires at least one voyage schedule for each relic needed to break the curse.";
                return false;
            }

            var copiedVoyages = new VoyageDefinition[voyages.Count];
            for (int index = 0; index < voyages.Count; index++)
            {
                if (voyages[index] == null)
                {
                    campaign = null;
                    issue = $"Campaign voyage schedule '{index}' is missing.";
                    return false;
                }

                copiedVoyages[index] = voyages[index];
            }

            campaign = new CampaignDefinition(
                sinkingRate,
                requiredRelics,
                Array.AsReadOnly(copiedVoyages));
            issue = string.Empty;
            return true;
        }
    }
}
