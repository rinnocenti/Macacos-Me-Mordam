using System;
using System.Collections.Generic;
using Random = System.Random;
using MacacosMeMordam.Gameplay;
using UnityEngine;

namespace MacacosMeMordam.Unity
{
    /// <summary>
    /// Unity authoring adapter for the data-driven campaign definition.
    /// Source: design/gdd/voyage-system.md.
    /// </summary>
    [CreateAssetMenu(
        fileName = "Campaign Definition",
        menuName = "Macacos Me Mordam/Campaign Definition")]
    public sealed class CampaignDefinitionAsset : ScriptableObject
    {
        private const float MinimumVoyageDurationSeconds = 60f;
        private const float MaximumVoyageDurationSeconds = 300f;

        [Tooltip("Afundamento por unidade de peso e segundo ativo.")]
        [SerializeField, Min(0.0001f)] private float sinkingRate = 0.0015f;
        [SerializeField, Min(1)] private int requiredRelics = 3;
        [Tooltip("-1 sorteia um padrão novo por sessão. Um valor não negativo repete o padrão para testes.")]
        [SerializeField] private int randomSeed = -1;
        [SerializeField] private MonkeyClassData[] monkeyClasses = Array.Empty<MonkeyClassData>();
        [SerializeField] private VoyageData[] voyages = Array.Empty<VoyageData>();

        /// <summary>Builds validated campaign data and returns its random seed when creation succeeds.</summary>
        public bool TryBuildDefinition(
            out CampaignDefinition definition,
            out int resolvedSeed,
            out string issue)
        {
            definition = null;
            resolvedSeed = 0;
            if (randomSeed < -1)
            {
                issue = "Campaign random seed must be -1 or non-negative.";
                return false;
            }

            if (monkeyClasses == null)
            {
                issue = "Campaign Monkey Classes data is missing.";
                return false;
            }

            var resolvedClasses = new MonkeyClassDefinition[monkeyClasses.Length];
            for (int classIndex = 0; classIndex < monkeyClasses.Length; classIndex++)
            {
                MonkeyClassData classData = monkeyClasses[classIndex];
                if (classData == null)
                {
                    issue = $"Campaign Monkey Class '{classIndex}' is missing.";
                    return false;
                }

                if (!classData.TryBuild(out MonkeyClassDefinition monkeyClass, out string classIssue))
                {
                    issue = $"Campaign Monkey Class '{classIndex}' is invalid. {classIssue}";
                    return false;
                }

                if (classIndex > 0 &&
                    (monkeyClass.Weight <= resolvedClasses[classIndex - 1].Weight ||
                     monkeyClass.RequiredHitsToDefeat <= resolvedClasses[classIndex - 1].RequiredHitsToDefeat))
                {
                    issue = "Campaign Monkey Classes must increase in both weight and touches required.";
                    return false;
                }

                resolvedClasses[classIndex] = monkeyClass;
            }

            if (voyages == null)
            {
                issue = "Campaign voyage schedules are missing.";
                return false;
            }

            int sessionSeed = randomSeed < 0 ? Guid.NewGuid().GetHashCode() : randomSeed;
            var random = new Random(sessionSeed);
            var resolvedVoyages = new List<VoyageDefinition>(voyages.Length);
            for (int voyageIndex = 0; voyageIndex < voyages.Length; voyageIndex++)
            {
                VoyageData voyageData = voyages[voyageIndex];
                if (voyageData == null)
                {
                    issue = $"Campaign voyage schedule '{voyageIndex}' is missing.";
                    return false;
                }

                if (!voyageData.TryBuild(resolvedClasses, random, out VoyageDefinition voyage, out string voyageIssue))
                {
                    issue = $"Campaign voyage schedule '{voyageIndex}' is invalid. {voyageIssue}";
                    return false;
                }

                resolvedVoyages.Add(voyage);
            }

            bool created = CampaignDefinition.TryCreate(
                resolvedVoyages,
                sinkingRate,
                requiredRelics,
                out definition,
                out issue);
            if (created)
            {
                resolvedSeed = sessionSeed;
            }

            return created;
        }

        [Serializable]
        private sealed class MonkeyClassData
        {
            [Tooltip("O peso desta classe. Ordene as classes do menor para o maior peso.")]
            [SerializeField, Min(0.001f)] private float weight;
            [Tooltip("Toques necessários para derrotar esta classe. Aumente junto com o peso; valores iniciais: classe 1 = 1, classe 2 = 2.")]
            [SerializeField, Min(1)] private int clicksToDefeat = 1;
            [Tooltip("Inclusive touch radius in Campaign Board local units.")]
            [SerializeField, Min(0.001f)] private float hitRadius;

            public bool TryBuild(out MonkeyClassDefinition definition, out string issue) =>
                MonkeyClassDefinition.TryCreate(
                    weight,
                    hitRadius,
                    clicksToDefeat,
                    out definition,
                    out issue);
        }

        [Serializable]
        private sealed class VoyageData
        {
            [Tooltip("Duração configurável entre 60 segundos (1 minuto) e 300 segundos (5 minutos).")]
            [SerializeField, Range(
                CampaignDefinitionAsset.MinimumVoyageDurationSeconds,
                CampaignDefinitionAsset.MaximumVoyageDurationSeconds)] private float durationSeconds = 300f;
            [SerializeField] private SpawnWaveData[] spawnWaves = Array.Empty<SpawnWaveData>();

            public bool TryBuild(
                IReadOnlyList<MonkeyClassDefinition> classes,
                Random random,
                out VoyageDefinition definition,
                out string issue)
            {
                definition = null;
                if (!CampaignDefinitionAsset.IsVoyageDurationValid(durationSeconds))
                {
                    issue = "Voyage duration must be configured between 60 and 300 seconds.";
                    return false;
                }

                if (spawnWaves == null)
                {
                    issue = "Spawn wave collection is missing.";
                    return false;
                }

                var resolvedWaves = new List<SpawnWaveDefinition>(spawnWaves.Length);
                double previousMaximumSpawnTime = double.NegativeInfinity;
                for (int waveIndex = 0; waveIndex < spawnWaves.Length; waveIndex++)
                {
                    SpawnWaveData waveData = spawnWaves[waveIndex];
                    if (waveData == null)
                    {
                        issue = $"Spawn wave '{waveIndex}' is missing.";
                        return false;
                    }

                    if (!waveData.TryBuild(
                            classes,
                            random,
                            durationSeconds,
                            previousMaximumSpawnTime,
                            out SpawnWaveDefinition wave,
                            out double maximumSpawnTime,
                            out string waveIssue))
                    {
                        issue = $"Spawn wave '{waveIndex}' is invalid. {waveIssue}";
                        return false;
                    }

                    resolvedWaves.Add(wave);
                    previousMaximumSpawnTime = maximumSpawnTime;
                }

                return VoyageDefinition.TryCreate(durationSeconds, resolvedWaves, out definition, out issue);
            }
        }

        internal static bool IsVoyageDurationValid(float seconds) =>
            !float.IsNaN(seconds) &&
            !float.IsInfinity(seconds) &&
            seconds >= MinimumVoyageDurationSeconds &&
            seconds <= MaximumVoyageDurationSeconds;

        internal static double ResolveTimingRandomness(
            double nominalSpawnTime,
            double voyageDurationSeconds,
            double configuredMaximumSeconds) =>
            Math.Min(
                configuredMaximumSeconds,
                Math.Min(nominalSpawnTime * 0.5d, (voyageDurationSeconds - nominalSpawnTime) * 0.5d));

        [Serializable]
        private sealed class SpawnWaveData
        {
            [SerializeField] private int order;
            [SerializeField, Range(0f, 1f)] private float progress;
            [Tooltip("Variação aleatória máxima do horário desta onda, em segundos.")]
            [SerializeField, Min(0f)] private float timingRandomnessSeconds = 3f;
            [SerializeField] private MonkeySpawnData[] monkeys = Array.Empty<MonkeySpawnData>();

            public bool TryBuild(
                IReadOnlyList<MonkeyClassDefinition> classes,
                Random random,
                double voyageDurationSeconds,
                double previousMaximumSpawnTime,
                out SpawnWaveDefinition definition,
                out double maximumSpawnTime,
                out string issue)
            {
                definition = null;
                maximumSpawnTime = double.NegativeInfinity;
                if (random == null)
                {
                    issue = "Spawn wave random source is required.";
                    return false;
                }

                if (float.IsNaN(progress) || float.IsInfinity(progress) ||
                    float.IsNaN(timingRandomnessSeconds) || float.IsInfinity(timingRandomnessSeconds) ||
                    timingRandomnessSeconds < 0f)
                {
                    issue = "Spawn wave progress and timing randomness must be finite; timing randomness cannot be negative.";
                    return false;
                }

                double nominalSpawnTime = progress * voyageDurationSeconds;
                double effectiveTimingRandomnessSeconds = CampaignDefinitionAsset.ResolveTimingRandomness(
                    nominalSpawnTime,
                    voyageDurationSeconds,
                    timingRandomnessSeconds);
                double minimumSpawnTime = nominalSpawnTime - effectiveTimingRandomnessSeconds;
                maximumSpawnTime = nominalSpawnTime + effectiveTimingRandomnessSeconds;
                if (minimumSpawnTime <= 0d || maximumSpawnTime >= voyageDurationSeconds)
                {
                    issue = "Spawn wave timing bounds must stay after departure and before arrival.";
                    return false;
                }

                if (minimumSpawnTime < previousMaximumSpawnTime)
                {
                    issue = "Spawn wave timing windows must not overlap or reorder.";
                    return false;
                }

                double sampledSpawnTime = minimumSpawnTime + random.NextDouble() * (maximumSpawnTime - minimumSpawnTime);
                double sampledProgress = sampledSpawnTime / voyageDurationSeconds;
                if (monkeys == null)
                {
                    issue = "Spawn wave monkey collection is missing.";
                    return false;
                }

                var resolvedMonkeys = new List<MonkeySpawnDefinition>(monkeys.Length);
                for (int monkeyIndex = 0; monkeyIndex < monkeys.Length; monkeyIndex++)
                {
                    MonkeySpawnData monkeyData = monkeys[monkeyIndex];
                    if (monkeyData == null)
                    {
                        issue = $"Spawn entry '{monkeyIndex}' is missing.";
                        return false;
                    }

                    if (!monkeyData.TryBuild(classes, random, out MonkeySpawnDefinition monkey, out string monkeyIssue))
                    {
                        issue = $"Spawn entry '{monkeyIndex}' is invalid. {monkeyIssue}";
                        return false;
                    }

                    resolvedMonkeys.Add(monkey);
                }

                return SpawnWaveDefinition.TryCreate(order, sampledProgress, resolvedMonkeys, out definition, out issue);
            }
        }

        [Serializable]
        private sealed class MonkeySpawnData
        {
            [SerializeField] private int monkeyClassIndex;
            [Tooltip("Centro da área de surgimento em unidades locais do tabuleiro, relativo ao centro visual.")]
            [SerializeField] private Vector2 position;
            [Tooltip("Metade da largura e da altura da área retangular usada para sortear a posição.")]
            [SerializeField] private Vector2 positionRandomness = new Vector2(40f, 24f);

            public bool TryBuild(
                IReadOnlyList<MonkeyClassDefinition> classes,
                Random random,
                out MonkeySpawnDefinition definition,
                out string issue)
            {
                definition = default;
                if (monkeyClassIndex < 0 || monkeyClassIndex >= classes.Count)
                {
                    issue = "Monkey class index does not refer to an authored class.";
                    return false;
                }

                if (random == null ||
                    float.IsNaN(positionRandomness.x) || float.IsInfinity(positionRandomness.x) ||
                    float.IsNaN(positionRandomness.y) || float.IsInfinity(positionRandomness.y) ||
                    positionRandomness.x < 0f || positionRandomness.y < 0f)
                {
                    issue = "Monkey spawn randomness must be finite and non-negative.";
                    return false;
                }

                try
                {
                    var point = new Point2(
                        position.x + (float)((random.NextDouble() * 2d - 1d) * positionRandomness.x),
                        position.y + (float)((random.NextDouble() * 2d - 1d) * positionRandomness.y));
                    return MonkeySpawnDefinition.TryCreate(
                        point,
                        classes[monkeyClassIndex],
                        out definition,
                        out issue);
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    issue = exception.Message;
                    return false;
                }
            }
        }
    }
}
