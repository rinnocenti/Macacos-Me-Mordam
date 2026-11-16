using System.Collections.Generic;
using Immersive.Logging.Loggers;
using Immersive.Logging.Records;
using MacacosMeMordam.Gameplay;

namespace MacacosMeMordam.Unity
{
    internal sealed class CampaignDiagnostics
    {
        private readonly ScopedLogger _logger;
        private readonly string _sessionId;
        private readonly HashSet<long> _previousSpawnOrders = new HashSet<long>();
        private readonly HashSet<long> _currentSpawnOrders = new HashSet<long>();
        private int _randomSeed;
        private int _voyageCount;
        private int _activeVoyageIndex = -1;
        private int _attempt;
        private int _spawnedCount;
        private int _defeatedCount;
        private int _touchAttempts;
        private int _hitCount;
        private int _missCount;
        private double _lastElapsedSeconds;
        private CampaignState? _lastState;
        private bool? _lastApplicationPaused;
        private bool _started;

        public CampaignDiagnostics(ScopedLogger logger, string sessionId)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _sessionId = string.IsNullOrWhiteSpace(sessionId)
                ? throw new System.ArgumentException("Session id is required.", nameof(sessionId))
                : sessionId;
        }

        public void Start(CampaignSnapshot snapshot, int randomSeed, int voyageCount)
        {
            if (_started)
            {
                return;
            }

            _started = true;
            _randomSeed = randomSeed;
            _voyageCount = voyageCount;
            _logger.Info(
                "Campaign session started.",
                LogFields.Field("sessionId", _sessionId),
                LogFields.Field("randomSeed", _randomSeed),
                LogFields.Field("voyageCount", _voyageCount),
                LogFields.Field("requiredRelics", snapshot.RequiredRelics));
            Observe(snapshot);
        }

        public void Observe(CampaignSnapshot snapshot)
        {
            if (!_started || !snapshot.Voyage.HasValue)
            {
                return;
            }

            if (snapshot.State == CampaignState.Sailing &&
                (_activeVoyageIndex != snapshot.VoyageIndex ||
                 !_lastState.HasValue ||
                 _lastState.Value != CampaignState.Sailing))
            {
                StartAttempt(snapshot);
            }

            VoyageSnapshot voyage = snapshot.Voyage.Value;
            _currentSpawnOrders.Clear();
            int spawnedThisEvent = 0;
            double spawnedWeight = 0d;
            for (int index = 0; index < voyage.ActiveMonkeys.Count; index++)
            {
                MonkeySnapshot monkey = voyage.ActiveMonkeys[index];
                _currentSpawnOrders.Add(monkey.SpawnOrder);
                if (_previousSpawnOrders.Contains(monkey.SpawnOrder))
                {
                    continue;
                }

                spawnedThisEvent++;
                spawnedWeight += monkey.Weight;
                _logger.Debug(
                    "Monkey spawned.",
                    LogFields.Field("sessionId", _sessionId),
                    LogFields.Field("voyageIndex", _activeVoyageIndex),
                    LogFields.Field("attempt", _attempt),
                    LogFields.Field("spawnOrder", monkey.SpawnOrder),
                    LogFields.Field("weight", monkey.Weight),
                    LogFields.Field("hitRadius", monkey.HitRadius),
                    LogFields.Field("requiredHitsToDefeat", monkey.RequiredHitsToDefeat),
                    LogFields.Field("x", monkey.Position.X),
                    LogFields.Field("y", monkey.Position.Y));
            }

            foreach (long previousSpawnOrder in _previousSpawnOrders)
            {
                if (!_currentSpawnOrders.Contains(previousSpawnOrder))
                {
                    _defeatedCount++;
                }
            }

            _previousSpawnOrders.Clear();
            foreach (long currentSpawnOrder in _currentSpawnOrders)
            {
                _previousSpawnOrders.Add(currentSpawnOrder);
            }

            if (spawnedThisEvent > 0)
            {
                _spawnedCount += spawnedThisEvent;
                _logger.Info(
                    "Monkeys spawned.",
                    LogFields.Field("sessionId", _sessionId),
                    LogFields.Field("voyageIndex", _activeVoyageIndex),
                    LogFields.Field("attempt", _attempt),
                    LogFields.Field("spawnedThisEvent", spawnedThisEvent),
                    LogFields.Field("spawnedWeight", spawnedWeight),
                    LogFields.Field("spawnedCount", _spawnedCount),
                    LogFields.Field("activeMonkeyCount", voyage.ActiveMonkeys.Count),
                    LogFields.Field("activeWeight", voyage.ActiveWeight),
                    LogFields.Field("travelProgress", voyage.TravelProgress));
            }

            _lastElapsedSeconds = voyage.ElapsedSeconds;
            if (_lastState == CampaignState.Sailing && snapshot.State != CampaignState.Sailing)
            {
                LogVoyageEnd(snapshot, voyage);
            }

            _lastState = snapshot.State;
        }

        public void RecordTouch(bool hit, long spawnOrder, int remainingHits, bool defeated)
        {
            if (!_started || _lastState != CampaignState.Sailing)
            {
                return;
            }

            _touchAttempts++;
            if (hit)
            {
                _hitCount++;
                _logger.Info(
                    "Monkey hit by touch.",
                    LogFields.Field("sessionId", _sessionId),
                    LogFields.Field("voyageIndex", _activeVoyageIndex),
                    LogFields.Field("attempt", _attempt),
                    LogFields.Field("hit", true),
                    LogFields.Field("defeated", defeated),
                    LogFields.Field("remainingHits", remainingHits),
                    LogFields.Field("spawnOrder", spawnOrder),
                    LogFields.Field("touchAttempts", _touchAttempts),
                    LogFields.Field("hitCount", _hitCount),
                    LogFields.Field("missCount", _missCount));
                return;
            }

            _missCount++;
            _logger.Debug(
                "Touch did not hit an active monkey.",
                LogFields.Field("sessionId", _sessionId),
                LogFields.Field("voyageIndex", _activeVoyageIndex),
                LogFields.Field("attempt", _attempt),
                LogFields.Field("touchAttempts", _touchAttempts),
                LogFields.Field("hitCount", _hitCount),
                LogFields.Field("missCount", _missCount));
        }

        public void RecordApplicationPause(bool paused)
        {
            if (!_started || _lastApplicationPaused == paused)
            {
                return;
            }

            bool? previous = _lastApplicationPaused;
            _lastApplicationPaused = paused;
            if (!previous.HasValue && !paused)
            {
                return;
            }

            _logger.Info(
                paused ? "Application paused." : "Application resumed.",
                LogFields.Field("sessionId", _sessionId),
                LogFields.Field("voyageIndex", _activeVoyageIndex),
                LogFields.Field("attempt", _attempt),
                LogFields.Field("elapsedSeconds", CurrentElapsedSeconds()));
        }

        private void StartAttempt(CampaignSnapshot snapshot)
        {
            int nextAttempt = _activeVoyageIndex == snapshot.VoyageIndex ? _attempt + 1 : 1;
            _activeVoyageIndex = snapshot.VoyageIndex;
            _attempt = nextAttempt;
            _previousSpawnOrders.Clear();
            _currentSpawnOrders.Clear();
            _spawnedCount = 0;
            _defeatedCount = 0;
            _touchAttempts = 0;
            _hitCount = 0;
            _missCount = 0;

            VoyageSnapshot voyage = snapshot.Voyage.Value;
            _logger.Info(
                "Voyage attempt started.",
                LogFields.Field("sessionId", _sessionId),
                LogFields.Field("randomSeed", _randomSeed),
                LogFields.Field("voyageIndex", _activeVoyageIndex),
                LogFields.Field("voyageNumber", _activeVoyageIndex + 1),
                LogFields.Field("attempt", _attempt),
                LogFields.Field("voyageCount", _voyageCount),
                LogFields.Field("durationSeconds", voyage.DurationSeconds),
                LogFields.Field("activeMonkeyCount", voyage.ActiveMonkeys.Count));
        }

        private void LogVoyageEnd(CampaignSnapshot snapshot, VoyageSnapshot voyage)
        {
            _logger.Info(
                "Voyage ended.",
                LogFields.Field("sessionId", _sessionId),
                LogFields.Field("outcome", GetOutcome(snapshot.State)),
                LogFields.Field("voyageIndex", _activeVoyageIndex),
                LogFields.Field("voyageNumber", _activeVoyageIndex + 1),
                LogFields.Field("attempt", _attempt),
                LogFields.Field("elapsedSeconds", voyage.ElapsedSeconds),
                LogFields.Field("durationSeconds", voyage.DurationSeconds),
                LogFields.Field("travelProgress", voyage.TravelProgress),
                LogFields.Field("sinkingProgress", voyage.SinkingProgress),
                LogFields.Field("activeMonkeyCount", voyage.ActiveMonkeys.Count),
                LogFields.Field("activeWeight", voyage.ActiveWeight),
                LogFields.Field("spawnedCount", _spawnedCount),
                LogFields.Field("defeatedCount", _defeatedCount),
                LogFields.Field("touchAttempts", _touchAttempts),
                LogFields.Field("hitCount", _hitCount),
                LogFields.Field("missCount", _missCount),
                LogFields.Field("relicCount", snapshot.RelicCount),
                LogFields.Field("requiredRelics", snapshot.RequiredRelics));
        }

        private double CurrentElapsedSeconds() =>
            _lastState.HasValue ? _lastElapsedSeconds : 0d;

        private static string GetOutcome(CampaignState state)
        {
            switch (state)
            {
                case CampaignState.ReadyForRetry:
                    return "Sunk";
                case CampaignState.ReadyForContinue:
                    return "Arrived";
                case CampaignState.CurseBroken:
                    return "CurseBroken";
                default:
                    return state.ToString();
            }
        }
    }
}
