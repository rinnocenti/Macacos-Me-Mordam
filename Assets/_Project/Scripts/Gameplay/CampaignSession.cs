using System;

namespace MacacosMeMordam.Gameplay
{
    /// <summary>
    /// Owns relic rewards and retry/continue progression for one application session.
    /// Source: design/gdd/voyage-system.md.
    /// </summary>
    public sealed class CampaignSession : ICampaignSession
    {
        private readonly CampaignDefinition _definition;
        private IVoyageSimulation _voyage;
        private CampaignState _state = CampaignState.NotStarted;
        private int _relicCount;
        private int _voyageIndex;

        private CampaignSession(CampaignDefinition definition)
        {
            _definition = definition;
        }

        /// <summary>Gets an immutable projection of campaign progression and the current voyage.</summary>
        public CampaignSnapshot Snapshot => CreateSnapshot();

        /// <summary>Creates a campaign session only when its definition is valid.</summary>
        public static bool TryCreate(
            CampaignDefinition definition,
            out CampaignSession session,
            out string issue)
        {
            if (definition == null)
            {
                session = null;
                issue = "Campaign definition is required.";
                return false;
            }

            session = new CampaignSession(definition);
            issue = string.Empty;
            return true;
        }

        /// <summary>Starts the first scheduled voyage exactly once.</summary>
        public bool TryStart(out string issue)
        {
            if (_state != CampaignState.NotStarted)
            {
                issue = "Campaign can only be started once from NotStarted.";
                return false;
            }

            StartVoyage(_voyageIndex);
            _state = CampaignState.Sailing;
            issue = string.Empty;
            return true;
        }

        /// <summary>Advances the active voyage and grants arrival rewards once.</summary>
        public CampaignSnapshot Advance(double activeDeltaSeconds)
        {
            if (_state != CampaignState.Sailing)
            {
                return CreateSnapshot();
            }

            VoyageSnapshot result = _voyage.Advance(activeDeltaSeconds);
            if (result.State == VoyageState.Arrived)
            {
                _relicCount++;
                if (_relicCount >= _definition.RequiredRelics)
                {
                    _state = CampaignState.CurseBroken;
                }
                else
                {
                    _voyageIndex++;
                    _state = CampaignState.ReadyForContinue;
                }
            }
            else if (result.State == VoyageState.Sunk)
            {
                _state = CampaignState.ReadyForRetry;
            }

            return CreateSnapshot();
        }

        /// <summary>Forwards a touch point only while a voyage is sailing.</summary>
        public bool TryHitAt(Point2 point, out long hitSpawnOrder, out int remainingHits, out bool defeated)
        {
            hitSpawnOrder = -1L;
            remainingHits = 0;
            defeated = false;
            return _state == CampaignState.Sailing &&
                   _voyage.TryHitAt(point, out hitSpawnOrder, out remainingHits, out defeated);
        }

        /// <summary>Restarts the failed schedule while preserving already earned relics.</summary>
        public bool TryRetry(out string issue)
        {
            if (_state != CampaignState.ReadyForRetry)
            {
                issue = "Retry is only available after a sunk voyage.";
                return false;
            }

            StartVoyage(_voyageIndex);
            _state = CampaignState.Sailing;
            issue = string.Empty;
            return true;
        }

        /// <summary>Starts the next scheduled voyage without granting another relic.</summary>
        public bool TryContinue(out string issue)
        {
            if (_state != CampaignState.ReadyForContinue)
            {
                issue = "Continue is only available after an arrival below the relic threshold.";
                return false;
            }

            StartVoyage(_voyageIndex);
            _state = CampaignState.Sailing;
            issue = string.Empty;
            return true;
        }

        private void StartVoyage(int voyageIndex)
        {
            VoyageDefinition voyageDefinition = _definition.Voyages[voyageIndex];
            _voyage = new VoyageSimulation(voyageDefinition, _definition.SinkingRate);
        }

        private CampaignSnapshot CreateSnapshot() =>
            new CampaignSnapshot(
                _state,
                _relicCount,
                _definition.RequiredRelics,
                _voyageIndex,
                _voyage == null ? (VoyageSnapshot?)null : _voyage.Snapshot);
    }
}
