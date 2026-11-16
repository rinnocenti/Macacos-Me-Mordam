using System;
using UnityEngine;

namespace MacacosMeMordam.Unity
{
    /// <summary>
    /// Explicit localized copy table for one campaign HUD locale.
    /// Source: docs/superpowers/plans/2026-11-15-campaign-implementation-plan.md.
    /// </summary>
    [CreateAssetMenu(
        fileName = "Campaign Localization",
        menuName = "Macacos Me Mordam/Campaign Localization")]
    public sealed class CampaignLocalizationAsset : ScriptableObject
    {
        [SerializeField] private string localeCode;
        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        /// <summary>Gets the locale tag associated with this authored copy table.</summary>
        public string LocaleCode => localeCode ?? string.Empty;

        /// <summary>Resolves one non-empty, unique text entry without a fallback value.</summary>
        public bool TryGetText(CampaignTextKey key, out string value, out string issue)
        {
            value = string.Empty;
            if (string.IsNullOrWhiteSpace(localeCode))
            {
                issue = "Campaign Localization requires an explicit locale code.";
                return false;
            }

            if (entries == null)
            {
                issue = "Campaign Localization entries are missing.";
                return false;
            }

            bool found = false;
            for (int index = 0; index < entries.Length; index++)
            {
                Entry entry = entries[index];
                if (entry == null || entry.Key != key)
                {
                    continue;
                }

                if (found)
                {
                    issue = $"Campaign Localization contains duplicate entries for '{key}'.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(entry.Value))
                {
                    issue = $"Campaign Localization entry '{key}' is empty.";
                    return false;
                }

                value = entry.Value;
                found = true;
            }

            if (!found)
            {
                issue = $"Campaign Localization is missing entry '{key}'.";
                return false;
            }

            issue = string.Empty;
            return true;
        }

        [Serializable]
        private sealed class Entry
        {
            [SerializeField] private CampaignTextKey key;
            [SerializeField, TextArea(1, 3)] private string value;

            public CampaignTextKey Key => key;

            public string Value => value ?? string.Empty;
        }
    }
}
