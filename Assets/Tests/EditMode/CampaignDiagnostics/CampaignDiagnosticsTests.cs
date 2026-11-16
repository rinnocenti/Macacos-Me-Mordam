using System;
using System.Collections.Generic;
using Immersive.Logging.Loggers;
using Immersive.Logging.Policies;
using Immersive.Logging.Records;
using Immersive.Logging.Sinks;
using MacacosMeMordam.Gameplay;
using MacacosMeMordam.Unity;
using NUnit.Framework;

namespace MacacosMeMordam.Unity.Tests
{
    public sealed class CampaignDiagnosticsTests
    {
        [Test]
        public void Start_ObservedAgain_LogsOneInitialAttempt()
        {
            CampaignSession session = DiagnosticsFixtures.Session(DiagnosticsFixtures.Campaign());
            Assert.IsTrue(session.TryStart(out string issue), issue);
            var sink = new RecordingLogSink();
            var diagnostics = new CampaignDiagnostics(CreateLogger(sink), "session-test");

            diagnostics.Start(session.Snapshot, 904, 3);
            diagnostics.Observe(session.Snapshot);

            LogRecord[] starts = sink.Find("Voyage attempt started.");
            Assert.AreEqual(1, sink.Find("Campaign session started.").Length);
            Assert.AreEqual(1, starts.Length);
            Assert.AreEqual(0, Field(starts[0], "voyageIndex"));
            Assert.AreEqual(1, Field(starts[0], "attempt"));
            Assert.AreEqual(DiagnosticsFixtures.DurationSeconds, Field(starts[0], "durationSeconds"));
        }

        [TestCase(59f, false)]
        [TestCase(60f, true)]
        [TestCase(300f, true)]
        [TestCase(301f, false)]
        public void CampaignDurationValidation_AcceptsOnlyOneToFiveMinutes(float seconds, bool expected)
        {
            Assert.AreEqual(expected, CampaignDefinitionAsset.IsVoyageDurationValid(seconds));
        }

        [TestCase(3d, 60d, 3d, 1.5d)]
        [TestCase(15d, 300d, 3d, 3d)]
        [TestCase(57d, 60d, 3d, 1.5d)]
        public void SpawnTimingRandomness_StaysInsideDepartureAndArrival(
            double nominalTime,
            double duration,
            double configuredMaximum,
            double expectedMaximum)
        {
            Assert.AreEqual(
                expectedMaximum,
                CampaignDefinitionAsset.ResolveTimingRandomness(nominalTime, duration, configuredMaximum),
                0.000001d);
        }

        [Test]
        public void SpawnHitAndArrival_LogsCountsAndTerminalSummary()
        {
            VoyageDefinition voyage = DiagnosticsFixtures.Voyage(
                DiagnosticsFixtures.Wave(
                    0.25d,
                    DiagnosticsFixtures.Spawn(DiagnosticsFixtures.Origin, DiagnosticsFixtures.LightClass)));
            CampaignDefinition campaign = DiagnosticsFixtures.Campaign(
                voyage,
                DiagnosticsFixtures.Voyage(),
                DiagnosticsFixtures.Voyage());
            CampaignSession session = DiagnosticsFixtures.Session(campaign);
            Assert.IsTrue(session.TryStart(out string issue), issue);
            var sink = new RecordingLogSink();
            var diagnostics = new CampaignDiagnostics(CreateLogger(sink), "session-test");
            diagnostics.Start(session.Snapshot, 7, 3);

            session.Advance(2.5d);
            diagnostics.Observe(session.Snapshot);
            diagnostics.RecordTouch(false, -1L, 0, false);
            bool hit = session.TryHitAt(
                DiagnosticsFixtures.Origin,
                out long spawnOrder,
                out int remainingHits,
                out bool defeated);
            diagnostics.RecordTouch(hit, spawnOrder, remainingHits, defeated);
            diagnostics.Observe(session.Snapshot);
            session.Advance(7.5d);
            diagnostics.Observe(session.Snapshot);

            LogRecord[] spawns = sink.Find("Monkeys spawned.");
            Assert.AreEqual(1, spawns.Length);
            Assert.AreEqual(1, Field(spawns[0], "spawnedThisEvent"));
            Assert.AreEqual(1, Field(spawns[0], "activeMonkeyCount"));
            LogRecord[] hits = sink.Find("Monkey hit by touch.");
            Assert.AreEqual(1, hits.Length);
            Assert.AreEqual(true, Field(hits[0], "hit"));
            Assert.AreEqual(true, Field(hits[0], "defeated"));
            Assert.AreEqual(0, Field(hits[0], "remainingHits"));
            LogRecord[] endings = sink.Find("Voyage ended.");
            Assert.AreEqual(1, endings.Length);
            Assert.AreEqual("Arrived", Field(endings[0], "outcome"));
            Assert.AreEqual(1, Field(endings[0], "spawnedCount"));
            Assert.AreEqual(1, Field(endings[0], "defeatedCount"));
            Assert.AreEqual(1, Field(endings[0], "hitCount"));
            Assert.AreEqual(1, Field(endings[0], "missCount"));
            Assert.AreEqual(2, Field(endings[0], "touchAttempts"));
        }

        [Test]
        public void HeavyMonkey_FirstTouch_LogsRemainingHitsWithoutDefeat()
        {
            VoyageDefinition voyage = DiagnosticsFixtures.Voyage(
                DiagnosticsFixtures.Wave(
                    0d,
                    DiagnosticsFixtures.Spawn(DiagnosticsFixtures.Origin, DiagnosticsFixtures.HeavyClass)));
            CampaignSession session = DiagnosticsFixtures.Session(DiagnosticsFixtures.Campaign(
                voyage,
                DiagnosticsFixtures.Voyage(),
                DiagnosticsFixtures.Voyage()));
            Assert.IsTrue(session.TryStart(out string issue), issue);
            session.Advance(0d);
            var sink = new RecordingLogSink();
            var diagnostics = new CampaignDiagnostics(CreateLogger(sink), "session-test");
            diagnostics.Start(session.Snapshot, 12, 3);

            bool hit = session.TryHitAt(
                DiagnosticsFixtures.Origin,
                out long spawnOrder,
                out int remainingHits,
                out bool defeated);
            diagnostics.RecordTouch(hit, spawnOrder, remainingHits, defeated);

            LogRecord[] hits = sink.Find("Monkey hit by touch.");
            Assert.AreEqual(1, hits.Length);
            Assert.AreEqual(true, Field(hits[0], "hit"));
            Assert.AreEqual(false, Field(hits[0], "defeated"));
            Assert.AreEqual(1, Field(hits[0], "remainingHits"));
            Assert.AreEqual(DiagnosticsFixtures.HeavyWeight, session.Snapshot.Voyage.Value.ActiveWeight);
        }

        [Test]
        public void Retry_AfterSinking_LogsSameVoyageWithIncrementedAttempt()
        {
            VoyageDefinition heavyVoyage = DiagnosticsFixtures.Voyage(
                DiagnosticsFixtures.Wave(
                    0d,
                    DiagnosticsFixtures.Spawn(DiagnosticsFixtures.Origin, DiagnosticsFixtures.HeavyClass)));
            CampaignDefinition campaign = DiagnosticsFixtures.Campaign(
                heavyVoyage,
                DiagnosticsFixtures.Voyage(),
                DiagnosticsFixtures.Voyage());
            CampaignSession session = DiagnosticsFixtures.Session(campaign);
            Assert.IsTrue(session.TryStart(out string issue), issue);
            var sink = new RecordingLogSink();
            var diagnostics = new CampaignDiagnostics(CreateLogger(sink), "session-test");
            diagnostics.Start(session.Snapshot, 14, 3);

            session.Advance(0d);
            diagnostics.Observe(session.Snapshot);
            session.Advance(5d);
            diagnostics.Observe(session.Snapshot);
            Assert.IsTrue(session.TryRetry(out issue), issue);
            diagnostics.Observe(session.Snapshot);

            LogRecord[] starts = sink.Find("Voyage attempt started.");
            Assert.AreEqual(2, starts.Length);
            Assert.AreEqual(0, Field(starts[0], "voyageIndex"));
            Assert.AreEqual(1, Field(starts[0], "attempt"));
            Assert.AreEqual(0, Field(starts[1], "voyageIndex"));
            Assert.AreEqual(2, Field(starts[1], "attempt"));
            LogRecord[] endings = sink.Find("Voyage ended.");
            Assert.AreEqual(1, endings.Length);
            Assert.AreEqual("Sunk", Field(endings[0], "outcome"));
        }

        [Test]
        public void ThirdArrival_LogsCurseBrokenForFinalVoyage()
        {
            CampaignSession session = DiagnosticsFixtures.Session(DiagnosticsFixtures.Campaign());
            Assert.IsTrue(session.TryStart(out string issue), issue);
            var sink = new RecordingLogSink();
            var diagnostics = new CampaignDiagnostics(CreateLogger(sink), "session-test");
            diagnostics.Start(session.Snapshot, 22, 3);

            for (int voyageIndex = 0; voyageIndex < 3; voyageIndex++)
            {
                session.Advance(DiagnosticsFixtures.DurationSeconds);
                diagnostics.Observe(session.Snapshot);
                if (voyageIndex < 2)
                {
                    Assert.IsTrue(session.TryContinue(out issue), issue);
                    diagnostics.Observe(session.Snapshot);
                }
            }

            LogRecord[] endings = sink.Find("Voyage ended.");
            Assert.AreEqual(3, endings.Length);
            Assert.AreEqual("CurseBroken", Field(endings[2], "outcome"));
            Assert.AreEqual(2, Field(endings[2], "voyageIndex"));
            Assert.AreEqual(3, Field(endings[2], "relicCount"));
        }

        private static ScopedLogger CreateLogger(ILogSink sink) =>
            new Logger(sink, new AllowAllLogPolicy()).For<CampaignDiagnosticsTests>("MacacosMeMordam.Campaign");

        private static class DiagnosticsFixtures
        {
            public const double DurationSeconds = 10d;
            private const double SinkingRate = 0.1d;
            private const double HitRadius = 0.25d;
            private const int RequiredRelics = 3;
            public const double HeavyWeight = 2d;

            public static Point2 Origin => new Point2(0d, 0d);

            public static MonkeyClassDefinition LightClass => CreateMonkeyClass(1d, 1);

            public static MonkeyClassDefinition HeavyClass => CreateMonkeyClass(HeavyWeight, 2);

            public static SpawnWaveDefinition Wave(double progress, params MonkeySpawnDefinition[] monkeys) =>
                Require(
                    SpawnWaveDefinition.TryCreate(0, progress, monkeys, out SpawnWaveDefinition wave, out string issue),
                    issue,
                    wave);

            public static MonkeySpawnDefinition Spawn(Point2 position, MonkeyClassDefinition monkeyClass) =>
                Require(
                    MonkeySpawnDefinition.TryCreate(position, monkeyClass, out MonkeySpawnDefinition spawn, out string issue),
                    issue,
                    spawn);

            public static VoyageDefinition Voyage(params SpawnWaveDefinition[] waves) =>
                Require(
                    VoyageDefinition.TryCreate(
                        DurationSeconds,
                        waves ?? Array.Empty<SpawnWaveDefinition>(),
                        out VoyageDefinition voyage,
                        out string issue),
                    issue,
                    voyage);

            public static CampaignDefinition Campaign(params VoyageDefinition[] voyages)
            {
                VoyageDefinition[] schedules = voyages == null || voyages.Length == 0
                    ? new[] { Voyage(), Voyage(), Voyage() }
                    : voyages;
                return Require(
                    CampaignDefinition.TryCreate(
                        schedules,
                        SinkingRate,
                        RequiredRelics,
                        out CampaignDefinition campaign,
                        out string issue),
                    issue,
                    campaign);
            }

            public static CampaignSession Session(CampaignDefinition campaign) =>
                Require(
                    CampaignSession.TryCreate(campaign, out CampaignSession session, out string issue),
                    issue,
                    session);

            private static MonkeyClassDefinition CreateMonkeyClass(double weight, int requiredHitsToDefeat) =>
                Require(
                    MonkeyClassDefinition.TryCreate(
                        weight,
                        HitRadius,
                        requiredHitsToDefeat,
                        out MonkeyClassDefinition definition,
                        out string issue),
                    issue,
                    definition);

            private static T Require<T>(bool succeeded, string issue, T value)
            {
                if (!succeeded)
                {
                    throw new InvalidOperationException(issue);
                }

                return value;
            }
        }

        private static object Field(LogRecord record, string key)
        {
            for (int index = 0; index < record.Fields.Count; index++)
            {
                if (record.Fields[index].Key == key)
                {
                    return record.Fields[index].Value;
                }
            }

            Assert.Fail($"Log record did not contain field '{key}'.");
            return null;
        }

        private sealed class RecordingLogSink : ILogSink
        {
            private readonly List<LogRecord> _records = new List<LogRecord>();

            public void Write(LogRecord record) => _records.Add(record);

            public LogRecord[] Find(string message)
            {
                var matches = new List<LogRecord>();
                for (int index = 0; index < _records.Count; index++)
                {
                    if (_records[index].Message == message)
                    {
                        matches.Add(_records[index]);
                    }
                }

                return matches.ToArray();
            }
        }
    }
}
