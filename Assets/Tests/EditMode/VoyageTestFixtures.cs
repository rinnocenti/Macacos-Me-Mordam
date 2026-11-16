using System;
using MacacosMeMordam.Gameplay;

namespace MacacosMeMordam.Gameplay.Tests
{
    internal static class VoyageTestFixtures
    {
        public const double DurationSeconds = 10d;
        public const double SinkingRate = 0.1d;
        public const double LightWeight = 1d;
        public const double HeavyWeight = 2d;
        public const double ThirdClassWeight = 3d;
        public const double HitRadius = 0.25d;
        public const double TwoSeconds = 2d;
        public const double FiveSeconds = 5d;
        public const int RequiredRelics = 3;
        public const double HalfSinking = 0.5d;
        public const double HeavySinkingBeforeRemoval = 0.4d;
        public const double CombinedSinkingAfterTwoSeconds = 0.6d;
        public const double PointOffset = 0.1d;

        public static Point2 Origin => new Point2(0d, 0d);

        public static MonkeyClassDefinition LightClass => CreateMonkeyClass(LightWeight, 1);

        public static MonkeyClassDefinition HeavyClass => CreateMonkeyClass(HeavyWeight, 2);

        public static MonkeyClassDefinition ThirdClass => CreateMonkeyClass(ThirdClassWeight, 3);

        public static MonkeyClassDefinition CreateMonkeyClass(double weight, int requiredHitsToDefeat) =>
            Require(
                MonkeyClassDefinition.TryCreate(
                    weight,
                    HitRadius,
                    requiredHitsToDefeat,
                    out MonkeyClassDefinition definition,
                    out string issue),
                issue,
                definition);

        public static MonkeySpawnDefinition Spawn(Point2 position, MonkeyClassDefinition monkeyClass) =>
            Require(
                MonkeySpawnDefinition.TryCreate(position, monkeyClass, out MonkeySpawnDefinition spawn, out string issue),
                issue,
                spawn);

        public static SpawnWaveDefinition Wave(double progress, params MonkeySpawnDefinition[] spawns) =>
            Wave(0, progress, spawns);

        public static SpawnWaveDefinition Wave(int order, double progress, params MonkeySpawnDefinition[] spawns) =>
            Require(
                SpawnWaveDefinition.TryCreate(order, progress, spawns, out SpawnWaveDefinition wave, out string issue),
                issue,
                wave);

        public static VoyageDefinition Voyage(params SpawnWaveDefinition[] waves) =>
            Require(
                VoyageDefinition.TryCreate(DurationSeconds, waves, out VoyageDefinition voyage, out string issue),
                issue,
                voyage);

        public static VoyageSimulation Simulation(VoyageDefinition voyage) =>
            new VoyageSimulation(voyage, SinkingRate);

        public static CampaignDefinition Campaign(params VoyageDefinition[] voyages)
        {
            if (voyages == null || voyages.Length == 0)
            {
                voyages = new[] { Voyage(), Voyage(), Voyage() };
            }

            return Require(
                CampaignDefinition.TryCreate(voyages, SinkingRate, RequiredRelics, out CampaignDefinition campaign, out string issue),
                issue,
                campaign);
        }

        public static CampaignSession Session(CampaignDefinition campaign)
        {
            return Require(
                CampaignSession.TryCreate(campaign, out CampaignSession session, out string issue),
                issue,
                session);
        }

        private static T Require<T>(bool succeeded, string issue, T value)
        {
            if (!succeeded)
            {
                throw new InvalidOperationException(issue);
            }

            return value;
        }
    }
}
