using System;
using System.Globalization;
using MacacosMeMordam.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MacacosMeMordam.Unity
{
    /// <summary>Displays and persists the player's language and audio level preferences.</summary>
    public sealed class SettingsPanelView : MonoBehaviour, ICancelHandler
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text languageHeadingText;
        [SerializeField] private Text audioHeadingText;
        [SerializeField] private Text musicLabelText;
        [SerializeField] private Text effectsLabelText;
        [SerializeField] private Text musicValueText;
        [SerializeField] private Text effectsValueText;
        [SerializeField] private Text englishOptionText;
        [SerializeField] private Text portugueseOptionText;
        [SerializeField] private Text backButtonText;
        [SerializeField] private MainMenuView mainMenuView;
        [SerializeField] private Button englishOptionButton;
        [SerializeField] private Button portugueseOptionButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider effectsSlider;

        private readonly IPlayerPreferencesStore _store = new UnityPlayerPreferencesStore();
        private PlayerPreferences _preferences;
        private bool _ready;

        private void Awake()
        {
            if (titleText == null ||
                languageHeadingText == null ||
                audioHeadingText == null ||
                musicLabelText == null ||
                effectsLabelText == null ||
                musicValueText == null ||
                effectsValueText == null ||
                englishOptionText == null ||
                portugueseOptionText == null ||
                backButtonText == null ||
                englishOptionButton == null ||
                portugueseOptionButton == null ||
                backButton == null ||
                musicSlider == null ||
                effectsSlider == null)
            {
                throw new InvalidOperationException(
                    "Settings Panel View requires every authored label, button and volume slider.");
            }

            englishOptionButton.onClick.AddListener(SelectEnglish);
            portugueseOptionButton.onClick.AddListener(SelectPortuguese);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            effectsSlider.onValueChanged.AddListener(SetEffectsVolume);
            englishOptionButton.interactable = true;
            portugueseOptionButton.interactable = true;
            englishOptionButton.image.raycastTarget = true;
            portugueseOptionButton.image.raycastTarget = true;
            ConfigureSlider(musicSlider);
            ConfigureSlider(effectsSlider);
            _ready = true;
            LoadAndRender();
        }

        private void OnDestroy()
        {
            if (!_ready)
            {
                return;
            }

            englishOptionButton.onClick.RemoveListener(SelectEnglish);
            portugueseOptionButton.onClick.RemoveListener(SelectPortuguese);
            musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
            effectsSlider.onValueChanged.RemoveListener(SetEffectsVolume);
        }

        /// <inheritdoc />
        public void OnCancel(BaseEventData eventData)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            gameObject.SetActive(false);
            eventData.Use();
        }

        private void OnEnable()
        {
            if (_ready)
            {
                if (EventSystem.current == null)
                {
                    throw new MissingReferenceException(
                        "Settings Panel View requires an active EventSystem.");
                }

                LoadAndRender();
                EventSystem.current.SetSelectedGameObject(backButton.gameObject);
            }
        }

        private static void ConfigureSlider(Slider slider)
        {
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.wholeNumbers = true;
            slider.interactable = true;
            if (slider.targetGraphic != null)
            {
                slider.targetGraphic.raycastTarget = true;
            }
        }

        private void LoadAndRender()
        {
            _preferences = _store.Load();
            Render();
        }

        private void SelectEnglish() => Save(new PlayerPreferences(
            PlayerLocale.EnUs,
            _preferences.MusicVolume,
            _preferences.EffectsVolume));

        private void SelectPortuguese() => Save(new PlayerPreferences(
            PlayerLocale.PtBr,
            _preferences.MusicVolume,
            _preferences.EffectsVolume));

        private void SetMusicVolume(float value) => Save(new PlayerPreferences(
            _preferences.Locale,
            Mathf.Clamp(Mathf.Round(value), 0f, 100f) / 100f,
            _preferences.EffectsVolume));

        private void SetEffectsVolume(float value) => Save(new PlayerPreferences(
            _preferences.Locale,
            _preferences.MusicVolume,
            Mathf.Clamp(Mathf.Round(value), 0f, 100f) / 100f));

        private void Save(PlayerPreferences preferences)
        {
            _store.Save(preferences);
            _preferences = preferences;
            Render();
        }

        private void Render()
        {
            bool english = _preferences.Locale == PlayerLocale.EnUs;
            titleText.text = english ? "SETTINGS" : "CONFIGURAÇÕES";
            languageHeadingText.text = english ? "LANGUAGE" : "IDIOMA";
            audioHeadingText.text = english ? "AUDIO" : "ÁUDIO";
            musicLabelText.text = english ? "MUSIC" : "MÚSICA";
            effectsLabelText.text = english ? "EFFECTS" : "EFEITOS";
            englishOptionText.text = english ? "ENGLISH (US) — SELECTED" : "INGLÊS (EUA)";
            portugueseOptionText.text = english ? "PORTUGUESE (BRAZIL)" : "PORTUGUÊS (BRASIL) — SELECIONADO";
            backButtonText.text = english ? "BACK" : "VOLTAR";
            if (mainMenuView != null)
            {
                mainMenuView.ApplyLocale(_preferences.Locale);
            }

            englishOptionButton.image.color = english
                ? new Color32(242, 184, 75, 255)
                : new Color32(255, 241, 207, 255);
            portugueseOptionButton.image.color = english
                ? new Color32(255, 241, 207, 255)
                : new Color32(242, 184, 75, 255);

            float musicPercent = _preferences.MusicVolume * 100f;
            float effectsPercent = _preferences.EffectsVolume * 100f;
            musicSlider.SetValueWithoutNotify(musicPercent);
            effectsSlider.SetValueWithoutNotify(effectsPercent);
            musicValueText.text = musicPercent.ToString("0", CultureInfo.InvariantCulture) + "%";
            effectsValueText.text = effectsPercent.ToString("0", CultureInfo.InvariantCulture) + "%";
        }
    }
}

