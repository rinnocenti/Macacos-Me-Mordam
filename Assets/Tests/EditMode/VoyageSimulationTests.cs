using MacacosMeMordam.Gameplay;
using NUnit.Framework;

namespace MacacosMeMordam.Gameplay.Tests
{
    public sealed class VoyageSimulationTests
    {
        [Test]
        public void Advance_EmptyVoyage_ReachesIslandWithoutSinking()
        {
            VoyageSimulation simulation = VoyageTestFixtures.Simulation(VoyageTestFixtures.Voyage());

            VoyageSnapshot result = simulation.Advance(VoyageTestFixtures.DurationSeconds);

            Assert.AreEqual(VoyageState.Arrived, result.State);
            Assert.AreEqual(1d, result.TravelProgress, 0.000001d);
            Assert.AreEqual(0d, result.SinkingProgress, 0.000001d);
            Assert.AreEqual(0d, result.ActiveWeight, 0.000001d);
        }

        [Test]
        public void Advance_PersistentHeavyMonkey_SinksBeforeArrival()
        {
            SpawnWaveDefinition wave = VoyageTestFixtures.Wave(
                0d,
                VoyageTestFixtures.Spawn(VoyageTestFixtures.Origin, VoyageTestFixtures.HeavyClass));
            VoyageSimulation simulation = VoyageTestFixtures.Simulation(VoyageTestFixtures.Voyage(wave));

            VoyageSnapshot result = simulation.Advance(VoyageTestFixtures.DurationSeconds);

            Assert.AreEqual(VoyageState.Sunk, result.State);
            Assert.AreEqual(VoyageTestFixtures.HeavyWeight, result.ActiveWeight, 0.000001d);
            Assert.AreEqual(1d, result.SinkingProgress, 0.000001d);
        }

        [Test]
        public void HeavyMonkey_FirstClick_LeavesMonkeyAndWeightActive()
        {
            SpawnWaveDefinition wave = VoyageTestFixtures.Wave(
                0d,
                VoyageTestFixtures.Spawn(VoyageTestFixtures.Origin, VoyageTestFixtures.HeavyClass));
            VoyageSimulation simulation = VoyageTestFixtures.Simulation(VoyageTestFixtures.Voyage(wave));
            simulation.Advance(0d);

            bool hit = simulation.TryHitAt(
                VoyageTestFixtures.Origin,
                out _,
                out int remainingHits,
                out bool defeated);
            VoyageSnapshot afterFirstClick = simulation.Snapshot;

            Assert.IsTrue(hit);
            Assert.IsFalse(defeated);
            Assert.AreEqual(1, remainingHits);
            Assert.AreEqual(1, afterFirstClick.ActiveMonkeys.Count);
            Assert.AreEqual(VoyageTestFixtures.HeavyWeight, afterFirstClick.ActiveWeight, 0.000001d);
            Assert.AreEqual(1, afterFirstClick.ActiveMonkeys[0].RemainingHits);
        }

        [Test]
        public void TryHitAt_ThirdClassMonkey_RequiresThreeTouches()
        {
            SpawnWaveDefinition wave = VoyageTestFixtures.Wave(
                0d,
                VoyageTestFixtures.Spawn(VoyageTestFixtures.Origin, VoyageTestFixtures.ThirdClass));
            VoyageSimulation simulation = VoyageTestFixtures.Simulation(VoyageTestFixtures.Voyage(wave));
            simulation.Advance(0d);

            for (int touch = 1; touch <= 3; touch++)
            {
                bool hit = simulation.TryHitAt(
                    VoyageTestFixtures.Origin,
                    out _,
                    out int remainingHits,
                    out bool defeated);

                Assert.IsTrue(hit);
                Assert.AreEqual(3 - touch, remainingHits);
                Assert.AreEqual(touch == 3, defeated);
                Assert.AreEqual(touch == 3 ? 0d : VoyageTestFixtures.ThirdClassWeight,
                    simulation.Snapshot.ActiveWeight,
                    0.000001d);
            }
        }

        [Test]
        public void Advance_LightMonkeyDefeatedAfterFiveSeconds_PreservesAccumulatedSinking()
        {
            SpawnWaveDefinition wave = VoyageTestFixtures.Wave(
                0d,
                VoyageTestFixtures.Spawn(VoyageTestFixtures.Origin, VoyageTestFixtures.LightClass));
            VoyageSimulation simulation = VoyageTestFixtures.Simulation(VoyageTestFixtures.Voyage(wave));

            simulation.Advance(VoyageTestFixtures.FiveSeconds);
            Assert.IsTrue(simulation.TryHitAt(VoyageTestFixtures.Origin, out _, out _, out bool defeated));
            Assert.IsTrue(defeated);
            VoyageSnapshot result = simulation.Advance(VoyageTestFixtures.FiveSeconds);

            Assert.AreEqual(VoyageState.Arrived, result.State);
            Assert.AreEqual(VoyageTestFixtures.HalfSinking, result.SinkingProgress, 0.000001d);
            Assert.AreEqual(0d, result.ActiveWeight, 0.000001d);
        }

        [Test]
        public void Advance_HeavyMonkeyDefeatedAfterTwoSeconds_ReachesIslandBelowSinkingLimit()
        {
            SpawnWaveDefinition wave = VoyageTestFixtures.Wave(
                0d,
                VoyageTestFixtures.Spawn(VoyageTestFixtures.Origin, VoyageTestFixtures.HeavyClass));
            VoyageSimulation simulation = VoyageTestFixtures.Simulation(VoyageTestFixtures.Voyage(wave));

            simulation.Advance(VoyageTestFixtures.TwoSeconds);
            Assert.IsTrue(simulation.TryHitAt(VoyageTestFixtures.Origin, out _, out int remainingHits, out bool defeated));
            Assert.IsFalse(defeated);
            Assert.AreEqual(1, remainingHits);
            Assert.AreEqual(VoyageTestFixtures.HeavyWeight, simulation.Snapshot.ActiveWeight, 0.000001d);
            Assert.IsTrue(simulation.TryHitAt(VoyageTestFixtures.Origin, out _, out remainingHits, out defeated));
            Assert.IsTrue(defeated);
            Assert.AreEqual(0, remainingHits);
            VoyageSnapshot result = simulation.Advance(VoyageTestFixtures.DurationSeconds);

            Assert.AreEqual(VoyageState.Arrived, result.State);
            Assert.AreEqual(VoyageTestFixtures.HeavySinkingBeforeRemoval, result.SinkingProgress, 0.000001d);
        }

        [Test]
        public void Advance_ExactArrivalAndSinkingTie_ArrivalWins()
        {
            SpawnWaveDefinition wave = VoyageTestFixtures.Wave(
                0d,
                VoyageTestFixtures.Spawn(VoyageTestFixtures.Origin, VoyageTestFixtures.LightClass));
            VoyageSimulation simulation = VoyageTestFixtures.Simulation(VoyageTestFixtures.Voyage(wave));

            VoyageSnapshot result = simulation.Advance(VoyageTestFixtures.DurationSeconds);

            Assert.AreEqual(VoyageState.Arrived, result.State);
            Assert.AreEqual(1d, result.SinkingProgress, 0.000001d);
        }

        [Test]
        public void Advance_SpawnAndSinkingTie_SpawnIsProcessedBeforeSunkTerminal()
        {
            SpawnWaveDefinition firstWave = VoyageTestFixtures.Wave(
                0,
                0d,
                VoyageTestFixtures.Spawn(VoyageTestFixtures.Origin, VoyageTestFixtures.HeavyClass));
            SpawnWaveDefinition tiedWave = VoyageTestFixtures.Wave(
                1,
                0.5d,
                VoyageTestFixtures.Spawn(VoyageTestFixtures.Origin, VoyageTestFixtures.LightClass));
            VoyageSimulation simulation = VoyageTestFixtures.Simulation(
                VoyageTestFixtures.Voyage(firstWave, tiedWave));

            VoyageSnapshot result = simulation.Advance(VoyageTestFixtures.DurationSeconds);

            Assert.AreEqual(VoyageState.Sunk, result.State);
            Assert.AreEqual(1d, result.SinkingProgress, 0.000001d);
            Assert.AreEqual(2, result.ActiveMonkeys.Count);
        }

        [Test]
        public void Advance_CombinedLightAndHeavyLoad_UsesTotalWeight()
        {
            SpawnWaveDefinition wave = VoyageTestFixtures.Wave(
                0d,
                VoyageTestFixtures.Spawn(VoyageTestFixtures.Origin, VoyageTestFixtures.LightClass),
                VoyageTestFixtures.Spawn(VoyageTestFixtures.Origin, VoyageTestFixtures.HeavyClass));
            VoyageSimulation simulation = VoyageTestFixtures.Simulation(VoyageTestFixtures.Voyage(wave));

            VoyageSnapshot result = simulation.Advance(VoyageTestFixtures.TwoSeconds);

            Assert.AreEqual(VoyageTestFixtures.LightWeight + VoyageTestFixtures.HeavyWeight, result.ActiveWeight, 0.000001d);
            Assert.AreEqual(VoyageTestFixtures.CombinedSinkingAfterTwoSeconds, result.SinkingProgress, 0.000001d);
        }

        [Test]
        public void TryHitAt_OverlappingTargets_HitsTheNearestMonkeyOnly()
        {
            SpawnWaveDefinition wave = VoyageTestFixtures.Wave(
                0d,
                VoyageTestFixtures.Spawn(VoyageTestFixtures.Origin, VoyageTestFixtures.LightClass),
                VoyageTestFixtures.Spawn(
                    new Point2(VoyageTestFixtures.PointOffset, 0d),
                    VoyageTestFixtures.HeavyClass));
            VoyageSimulation simulation = VoyageTestFixtures.Simulation(VoyageTestFixtures.Voyage(wave));
            simulation.Advance(0d);

            bool hit = simulation.TryHitAt(
                new Point2(VoyageTestFixtures.PointOffset, 0d),
                out long hitSpawnOrder,
                out int remainingHits,
                out bool defeated);
            VoyageSnapshot result = simulation.Snapshot;

            Assert.IsTrue(hit);
            Assert.IsFalse(defeated);
            Assert.AreEqual(1, remainingHits);
            Assert.AreEqual(1L, hitSpawnOrder);
            Assert.AreEqual(2, result.ActiveMonkeys.Count);
            Assert.AreEqual(1, result.ActiveMonkeys[1].RemainingHits);
        }

        [Test]
        public void TryHitAt_BeforeWaveTrigger_CannotTargetFutureMonkeys()
        {
            SpawnWaveDefinition wave = VoyageTestFixtures.Wave(
                0d,
                VoyageTestFixtures.Spawn(VoyageTestFixtures.Origin, VoyageTestFixtures.LightClass));
            VoyageSimulation simulation = VoyageTestFixtures.Simulation(VoyageTestFixtures.Voyage(wave));

            bool hitBeforeSpawn = simulation.TryHitAt(VoyageTestFixtures.Origin, out _, out _, out _);
            VoyageSnapshot afterTrigger = simulation.Advance(0d);

            Assert.IsFalse(hitBeforeSpawn);
            Assert.AreEqual(1, afterTrigger.ActiveMonkeys.Count);
        }

        [Test]
        public void TryHitAt_EqualDistanceTargets_HitsTheOldestMonkey()
        {
            SpawnWaveDefinition wave = VoyageTestFixtures.Wave(
                0d,
                VoyageTestFixtures.Spawn(
                    new Point2(-VoyageTestFixtures.PointOffset, 0d),
                    VoyageTestFixtures.LightClass),
                VoyageTestFixtures.Spawn(
                    new Point2(VoyageTestFixtures.PointOffset, 0d),
                    VoyageTestFixtures.LightClass));
            VoyageSimulation simulation = VoyageTestFixtures.Simulation(VoyageTestFixtures.Voyage(wave));
            simulation.Advance(0d);

            bool hit = simulation.TryHitAt(
                VoyageTestFixtures.Origin,
                out long hitSpawnOrder,
                out int remainingHits,
                out bool defeated);

            Assert.IsTrue(hit);
            Assert.IsTrue(defeated);
            Assert.AreEqual(0, remainingHits);
            Assert.AreEqual(0L, hitSpawnOrder);
            Assert.AreEqual(1L, simulation.Snapshot.ActiveMonkeys[0].SpawnOrder);
        }

        [Test]
        public void Advance_WaveAtOneHundredPercent_IsSuppressedByArrival()
        {
            SpawnWaveDefinition finalWave = VoyageTestFixtures.Wave(
                VoyageTestFixtures.RequiredRelics,
                1d,
                VoyageTestFixtures.Spawn(VoyageTestFixtures.Origin, VoyageTestFixtures.HeavyClass));
            VoyageSimulation simulation = VoyageTestFixtures.Simulation(VoyageTestFixtures.Voyage(finalWave));

            VoyageSnapshot result = simulation.Advance(VoyageTestFixtures.DurationSeconds);

            Assert.AreEqual(VoyageState.Arrived, result.State);
            Assert.AreEqual(0, result.ActiveMonkeys.Count);
        }

        [Test]
        public void TerminalVoyage_AdvanceAndTouch_DoNotChangeTheSnapshot()
        {
            VoyageSimulation simulation = VoyageTestFixtures.Simulation(VoyageTestFixtures.Voyage());
            simulation.Advance(VoyageTestFixtures.DurationSeconds);
            VoyageSnapshot before = simulation.Snapshot;

            VoyageSnapshot after = simulation.Advance(VoyageTestFixtures.DurationSeconds);
            bool hit = simulation.TryHitAt(VoyageTestFixtures.Origin, out _, out _, out _);

            Assert.IsFalse(hit);
            Assert.AreEqual(before.State, after.State);
            Assert.AreEqual(before.ElapsedSeconds, after.ElapsedSeconds, 0.000001d);
            Assert.AreEqual(before.SinkingProgress, after.SinkingProgress, 0.000001d);
        }
    }
}
