using MacacosMeMordam.Gameplay;
using NUnit.Framework;

namespace MacacosMeMordam.Gameplay.Tests
{
    public sealed class CampaignSessionTests
    {
        [Test]
        public void Create_CampaignMissingOneSchedulePerRelic_IsRejected()
        {
            VoyageDefinition[] incompleteSchedules =
            {
                VoyageTestFixtures.Voyage(),
                VoyageTestFixtures.Voyage()
            };

            bool created = CampaignDefinition.TryCreate(
                incompleteSchedules,
                VoyageTestFixtures.SinkingRate,
                VoyageTestFixtures.RequiredRelics,
                out _,
                out string issue);

            Assert.IsFalse(created);
            Assert.IsNotEmpty(issue);
        }

        [Test]
        public void Start_ArrivalBeforeRelicThreshold_AwardsOnceAndWaitsForContinue()
        {
            CampaignSession session = VoyageTestFixtures.Session(VoyageTestFixtures.Campaign());
            Assert.IsTrue(session.TryStart(out string startIssue), startIssue);

            CampaignSnapshot result = session.Advance(VoyageTestFixtures.DurationSeconds);

            Assert.AreEqual(CampaignState.ReadyForContinue, result.State);
            Assert.AreEqual(1, result.RelicCount);
            Assert.AreEqual(1, result.VoyageIndex);
            Assert.AreEqual(VoyageState.Arrived, result.Voyage.Value.State);
        }

        [Test]
        public void Continue_AfterArrival_StartsNextScheduleWithoutAnotherReward()
        {
            CampaignSession session = VoyageTestFixtures.Session(VoyageTestFixtures.Campaign());
            session.TryStart(out _);
            session.Advance(VoyageTestFixtures.DurationSeconds);

            bool continued = session.TryContinue(out string continueIssue);
            CampaignSnapshot result = session.Snapshot;

            Assert.IsTrue(continued, continueIssue);
            Assert.AreEqual(CampaignState.Sailing, result.State);
            Assert.AreEqual(1, result.RelicCount);
            Assert.AreEqual(1, result.VoyageIndex);
            Assert.AreEqual(0d, result.Voyage.Value.ElapsedSeconds, 0.000001d);
            Assert.AreEqual(0, result.Voyage.Value.ActiveMonkeys.Count);
        }

        [Test]
        public void Retry_AfterSinking_RepeatsCurrentScheduleAndPreservesRelics()
        {
            VoyageDefinition heavyVoyage = VoyageTestFixtures.Voyage(
                VoyageTestFixtures.Wave(
                    0d,
                    VoyageTestFixtures.Spawn(VoyageTestFixtures.Origin, VoyageTestFixtures.HeavyClass)));
            CampaignDefinition campaign = VoyageTestFixtures.Campaign(
                VoyageTestFixtures.Voyage(),
                heavyVoyage,
                VoyageTestFixtures.Voyage());
            CampaignSession session = VoyageTestFixtures.Session(campaign);
            session.TryStart(out _);
            session.Advance(VoyageTestFixtures.DurationSeconds);
            session.TryContinue(out _);
            session.Advance(0d);
            session.Advance(VoyageTestFixtures.DurationSeconds);

            CampaignSnapshot failed = session.Snapshot;
            bool retried = session.TryRetry(out string retryIssue);
            CampaignSnapshot retry = session.Snapshot;

            Assert.AreEqual(CampaignState.ReadyForRetry, failed.State);
            Assert.AreEqual(1, failed.RelicCount);
            Assert.AreEqual(1, failed.VoyageIndex);
            Assert.IsTrue(retried, retryIssue);
            Assert.AreEqual(CampaignState.Sailing, retry.State);
            Assert.AreEqual(1, retry.RelicCount);
            Assert.AreEqual(1, retry.VoyageIndex);
            Assert.AreEqual(0d, retry.Voyage.Value.ElapsedSeconds, 0.000001d);
            Assert.AreEqual(0, retry.Voyage.Value.ActiveMonkeys.Count);

            VoyageSnapshot restartedSchedule = session.Advance(0d).Voyage.Value;
            Assert.AreEqual(VoyageTestFixtures.HeavyWeight, restartedSchedule.ActiveWeight, 0.000001d);
        }

        [Test]
        public void SunkAttempt_DoesNotAwardRelicAndKeepsCurrentVoyageIndex()
        {
            VoyageDefinition heavyVoyage = VoyageTestFixtures.Voyage(
                VoyageTestFixtures.Wave(
                    0d,
                    VoyageTestFixtures.Spawn(VoyageTestFixtures.Origin, VoyageTestFixtures.HeavyClass)));
            CampaignSession session = VoyageTestFixtures.Session(
                VoyageTestFixtures.Campaign(heavyVoyage, VoyageTestFixtures.Voyage(), VoyageTestFixtures.Voyage()));
            session.TryStart(out _);
            session.Advance(0d);

            CampaignSnapshot result = session.Advance(VoyageTestFixtures.DurationSeconds);

            Assert.AreEqual(CampaignState.ReadyForRetry, result.State);
            Assert.AreEqual(0, result.RelicCount);
            Assert.AreEqual(0, result.VoyageIndex);
            Assert.AreEqual(VoyageState.Sunk, result.Voyage.Value.State);
        }

        [Test]
        public void ThirdArrival_BreaksCurseWithoutStartingAnotherVoyage()
        {
            CampaignSession session = VoyageTestFixtures.Session(VoyageTestFixtures.Campaign());
            session.TryStart(out _);

            for (int voyageNumber = 0; voyageNumber < VoyageTestFixtures.RequiredRelics; voyageNumber++)
            {
                session.Advance(VoyageTestFixtures.DurationSeconds);
                if (voyageNumber < VoyageTestFixtures.RequiredRelics - 1)
                {
                    Assert.IsTrue(session.TryContinue(out string continueIssue), continueIssue);
                }
            }

            CampaignSnapshot result = session.Snapshot;

            Assert.AreEqual(CampaignState.CurseBroken, result.State);
            Assert.AreEqual(VoyageTestFixtures.RequiredRelics, result.RelicCount);
            Assert.AreEqual(VoyageTestFixtures.RequiredRelics - 1, result.VoyageIndex);
            Assert.AreEqual(VoyageState.Arrived, result.Voyage.Value.State);
            Assert.IsFalse(session.TryContinue(out _));
            CampaignSnapshot afterTerminalAdvance = session.Advance(VoyageTestFixtures.DurationSeconds);
            Assert.AreEqual(VoyageTestFixtures.RequiredRelics, afterTerminalAdvance.RelicCount);
            Assert.AreEqual(CampaignState.CurseBroken, afterTerminalAdvance.State);
        }
    }
}
