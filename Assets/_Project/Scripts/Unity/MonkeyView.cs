using System;
using System.Globalization;
using MacacosMeMordam.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace MacacosMeMordam.Unity
{
    /// <summary>
    /// Positions one authored monkey visual from a gameplay snapshot.
    /// Source: design/gdd/voyage-system.md.
    /// </summary>
    public sealed class MonkeyView : MonoBehaviour
    {
        [SerializeField] private RectTransform visualRect;
        [SerializeField] private Text weightText;

        /// <summary>Gets whether the visual has its required shape and weight label.</summary>
        public bool IsReady => visualRect != null && weightText != null;

        /// <summary>Applies the board-local position from the supplied snapshot.</summary>
        public void Render(MonkeySnapshot snapshot, Vector2 boardLocalCenter)
        {
            if (visualRect == null || weightText == null)
            {
                throw new InvalidOperationException("Monkey View requires an explicit Visual Rect and Weight Text.");
            }

            float diameter = (float)(snapshot.HitRadius * 2d);
            visualRect.sizeDelta = new Vector2(diameter, diameter);
            visualRect.anchoredPosition = new Vector2(
                boardLocalCenter.x + (float)snapshot.Position.X,
                boardLocalCenter.y + (float)snapshot.Position.Y);
            weightText.text = string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.##} ({1}/{2})",
                snapshot.Weight,
                snapshot.RemainingHits,
                snapshot.RequiredHitsToDefeat);
        }
    }
}
