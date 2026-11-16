using System;
using System.Collections.Generic;

namespace MacacosMeMordam.Gameplay
{
    /// <summary>
    /// Deterministic time, spawn, load, sinking and arrival rules for one attempt.
    /// Source: design/gdd/voyage-system.md.
    /// </summary>
    public sealed class VoyageSimulation : IVoyageSimulation
    {
        private readonly VoyageDefinition _definition;
        private readonly double _sinkingRate;
        private readonly List<ActiveMonkey> _activeMonkeys = new List<ActiveMonkey>();
        private double _elapsedSeconds;
        private double _sinkingProgress;
        private int _nextWaveIndex;
        private long _nextSpawnOrder;
        private VoyageState _state = VoyageState.Sailing;

        /// <summary>Creates a simulation from validated voyage and sinking data.</summary>
        public VoyageSimulation(VoyageDefinition definition, double sinkingRate)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            if (!Point2.IsFiniteValue(sinkingRate) || sinkingRate <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(sinkingRate), "Sinking rate must be finite and positive.");
            }

            _sinkingRate = sinkingRate;
        }

        /// <summary>Gets an immutable copy of the current attempt.</summary>
        public VoyageSnapshot Snapshot => CreateSnapshot();

        /// <summary>Advances active time through every earlier spawn, sinking or arrival event.</summary>
        public VoyageSnapshot Advance(double activeDeltaSeconds)
        {
            if (!Point2.IsFiniteValue(activeDeltaSeconds) || activeDeltaSeconds < 0d)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(activeDeltaSeconds),
                    "Active delta time must be finite and non-negative.");
            }

            if (_state != VoyageState.Sailing)
            {
                return CreateSnapshot();
            }

            double remainingVoyageTime = _definition.DurationSeconds - _elapsedSeconds;
            double targetTime = activeDeltaSeconds >= remainingVoyageTime
                ? _definition.DurationSeconds
                : _elapsedSeconds + activeDeltaSeconds;

            while (_state == VoyageState.Sailing)
            {
                double arrivalTime = _definition.DurationSeconds;
                double sinkTime = CalculateSinkTime();
                double nextWaveTime = CalculateNextWaveTime();

                if (nextWaveTime <= targetTime &&
                    nextWaveTime < arrivalTime &&
                    nextWaveTime <= sinkTime)
                {
                    AdvanceLoadTo(nextWaveTime);
                    SpawnNextWave();
                    if (_sinkingProgress >= 1d)
                    {
                        _state = VoyageState.Sunk;
                        break;
                    }

                    continue;
                }

                if (arrivalTime <= targetTime && arrivalTime <= sinkTime)
                {
                    AdvanceLoadTo(arrivalTime);
                    _state = VoyageState.Arrived;
                    break;
                }

                if (sinkTime <= targetTime && sinkTime < arrivalTime)
                {
                    AdvanceLoadTo(sinkTime);
                    _sinkingProgress = 1d;
                    _state = VoyageState.Sunk;
                    break;
                }

                AdvanceLoadTo(targetTime);
                break;
            }

            return CreateSnapshot();
        }

        /// <summary>Applies one touch to the nearest active monkey, preferring distance then spawn order.</summary>
        public bool TryHitAt(Point2 point, out long hitSpawnOrder, out int remainingHits, out bool defeated)
        {
            hitSpawnOrder = -1L;
            remainingHits = 0;
            defeated = false;
            if (_state != VoyageState.Sailing || _activeMonkeys.Count == 0)
            {
                return false;
            }

            int nearestIndex = -1;
            double nearestDistanceSquared = double.PositiveInfinity;
            for (int index = 0; index < _activeMonkeys.Count; index++)
            {
                ActiveMonkey candidate = _activeMonkeys[index];
                double distanceSquared = point.DistanceSquaredTo(candidate.Definition.Position);
                double radiusSquared = candidate.Definition.MonkeyClass.HitRadius *
                                       candidate.Definition.MonkeyClass.HitRadius;
                if (distanceSquared <= radiusSquared && distanceSquared < nearestDistanceSquared)
                {
                    nearestIndex = index;
                    nearestDistanceSquared = distanceSquared;
                }
            }

            if (nearestIndex < 0)
            {
                return false;
            }

            ActiveMonkey target = _activeMonkeys[nearestIndex];
            hitSpawnOrder = target.SpawnOrder;
            remainingHits = target.ApplyHit();
            defeated = remainingHits == 0;
            if (defeated)
            {
                _activeMonkeys.RemoveAt(nearestIndex);
            }

            return true;
        }

        private double CalculateSinkTime()
        {
            double activeWeight = CalculateActiveWeight();
            if (activeWeight <= 0d || _sinkingProgress >= 1d)
            {
                return double.PositiveInfinity;
            }

            double sinkingRatePerSecond = activeWeight * _sinkingRate;
            return _elapsedSeconds + ((1d - _sinkingProgress) / sinkingRatePerSecond);
        }

        private double CalculateNextWaveTime()
        {
            if (_nextWaveIndex >= _definition.SpawnWaves.Count)
            {
                return double.PositiveInfinity;
            }

            return _definition.SpawnWaves[_nextWaveIndex].Progress * _definition.DurationSeconds;
        }

        private void AdvanceLoadTo(double targetTime)
        {
            double deltaSeconds = targetTime - _elapsedSeconds;
            if (deltaSeconds > 0d)
            {
                double activeWeight = CalculateActiveWeight();
                _sinkingProgress = Math.Min(
                    1d,
                    _sinkingProgress + (activeWeight * _sinkingRate * deltaSeconds));
            }

            _elapsedSeconds = targetTime;
        }

        private void SpawnNextWave()
        {
            SpawnWaveDefinition wave = _definition.SpawnWaves[_nextWaveIndex];
            _nextWaveIndex++;

            for (int index = 0; index < wave.Monkeys.Count; index++)
            {
                _activeMonkeys.Add(new ActiveMonkey(_nextSpawnOrder, wave.Monkeys[index]));
                _nextSpawnOrder = checked(_nextSpawnOrder + 1L);
            }
        }

        private double CalculateActiveWeight()
        {
            double totalWeight = 0d;
            for (int index = 0; index < _activeMonkeys.Count; index++)
            {
                totalWeight += _activeMonkeys[index].Definition.MonkeyClass.Weight;
            }

            return totalWeight;
        }

        private VoyageSnapshot CreateSnapshot()
        {
            var monkeys = new MonkeySnapshot[_activeMonkeys.Count];
            for (int index = 0; index < _activeMonkeys.Count; index++)
            {
                ActiveMonkey activeMonkey = _activeMonkeys[index];
                MonkeyClassDefinition monkeyClass = activeMonkey.Definition.MonkeyClass;
                monkeys[index] = new MonkeySnapshot(
                    activeMonkey.SpawnOrder,
                    activeMonkey.Definition.Position,
                    monkeyClass.Weight,
                    monkeyClass.HitRadius,
                    monkeyClass.RequiredHitsToDefeat,
                    activeMonkey.RemainingHits);
            }

            return new VoyageSnapshot(
                _state,
                _elapsedSeconds,
                _definition.DurationSeconds,
                _sinkingProgress,
                CalculateActiveWeight(),
                Array.AsReadOnly(monkeys));
        }

        private sealed class ActiveMonkey
        {
            public ActiveMonkey(long spawnOrder, MonkeySpawnDefinition definition)
            {
                SpawnOrder = spawnOrder;
                Definition = definition;
                RemainingHits = definition.MonkeyClass.RequiredHitsToDefeat;
            }

            public long SpawnOrder { get; }

            public MonkeySpawnDefinition Definition { get; }

            public int RemainingHits { get; private set; }

            public int ApplyHit()
            {
                if (RemainingHits <= 0)
                {
                    throw new InvalidOperationException("A defeated monkey cannot receive another touch.");
                }

                RemainingHits--;
                return RemainingHits;
            }
        }
    }
}
