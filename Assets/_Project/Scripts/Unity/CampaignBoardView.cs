using System.Collections.Generic;
using Immersive.Logging.Loggers;
using Immersive.Logging.Records;
using Immersive.Logging.Unity;
using MacacosMeMordam.Gameplay;
using UnityEngine;

namespace MacacosMeMordam.Unity
{
    /// <summary>
    /// Presents active monkey snapshots without owning spawn, weight or defeat rules.
    /// Source: design/gdd/voyage-system.md.
    /// </summary>
    public sealed class CampaignBoardView : MonoBehaviour
    {
        [Tooltip("Opcional. Reutilize a Logging Config da campanha para habilitar diagnósticos Debug.")]
        [SerializeField] private LoggingConfigAsset loggingConfig;
        [SerializeField] private CampaignActivityController controller;
        [SerializeField] private MonkeyView monkeyPrefab;

        private readonly Dictionary<long, MonkeyView> _viewsBySpawnOrder =
            new Dictionary<long, MonkeyView>();
        private readonly HashSet<long> _visibleSpawnOrders = new HashSet<long>();
        private readonly List<long> _staleSpawnOrders = new List<long>();
        private ScopedLogger _logger;
        private bool _initialized;

        private void Start()
        {
            _logger = UnityLoggingFactory
                .CreateLogger(loggingConfig)
                .For<CampaignBoardView>("MacacosMeMordam.Campaign.Presentation", gameObject.name);

            if (controller == null || !controller.IsReady)
            {
                Block("Campaign Board View requires a ready Campaign Activity Controller.");
                return;
            }

            if (monkeyPrefab == null || !monkeyPrefab.IsReady)
            {
                Block("Campaign Board View requires a configured Monkey View prefab.");
                return;
            }

            _initialized = true;
            Subscribe();
        }

        private void OnEnable()
        {
            if (_initialized)
            {
                Subscribe();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (controller == null)
            {
                return;
            }

            controller.SnapshotChanged -= Render;
            controller.SnapshotChanged += Render;
            Render(controller.Snapshot);
        }

        private void Unsubscribe()
        {
            if (controller != null)
            {
                controller.SnapshotChanged -= Render;
            }
        }

        private void Render(CampaignSnapshot campaign)
        {
            if (!campaign.Voyage.HasValue)
            {
                ClearViews();
                return;
            }

            IReadOnlyList<MonkeySnapshot> monkeys = campaign.Voyage.Value.ActiveMonkeys;
            _visibleSpawnOrders.Clear();
            for (int index = 0; index < monkeys.Count; index++)
            {
                MonkeySnapshot monkey = monkeys[index];
                _visibleSpawnOrders.Add(monkey.SpawnOrder);
                if (!_viewsBySpawnOrder.TryGetValue(monkey.SpawnOrder, out MonkeyView view))
                {
                    view = Instantiate(monkeyPrefab, controller.BoardRect, false);
                    _viewsBySpawnOrder.Add(monkey.SpawnOrder, view);
                }

                view.Render(monkey, controller.BoardRect.rect.center);
            }

            _staleSpawnOrders.Clear();
            foreach (KeyValuePair<long, MonkeyView> entry in _viewsBySpawnOrder)
            {
                if (!_visibleSpawnOrders.Contains(entry.Key))
                {
                    if (entry.Value != null)
                    {
                        Destroy(entry.Value.gameObject);
                    }

                    _staleSpawnOrders.Add(entry.Key);
                }
            }

            for (int index = 0; index < _staleSpawnOrders.Count; index++)
            {
                _viewsBySpawnOrder.Remove(_staleSpawnOrders[index]);
            }
        }

        private void ClearViews()
        {
            foreach (KeyValuePair<long, MonkeyView> entry in _viewsBySpawnOrder)
            {
                if (entry.Value != null)
                {
                    Destroy(entry.Value.gameObject);
                }
            }

            _viewsBySpawnOrder.Clear();
            _visibleSpawnOrders.Clear();
            _staleSpawnOrders.Clear();
        }

        private void Block(string issue)
        {
            _logger?.Error(
                "Campaign board presentation is not configured.",
                LogFields.Field("reason", issue));
            enabled = false;
        }
    }
}
