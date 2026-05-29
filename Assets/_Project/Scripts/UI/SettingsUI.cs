using Superglazka.Data;
using Superglazka.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Superglazka.UI
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Slider _narrationSlider;
        [SerializeField] private Toggle _musicToggle;
        [SerializeField] private Toggle _sfxToggle;

        [Header("Visual")]
        [SerializeField] private Toggle _subtitlesToggle;
        [SerializeField] private Toggle _highContrastToggle;
        [SerializeField] private Toggle _reduceMotionToggle;
        [SerializeField] private Toggle _darkThemeToggle;
        [SerializeField] private Button _fontSmall;
        [SerializeField] private Button _fontNormal;
        [SerializeField] private Button _fontLarge;

        [Header("Other")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _resetButton;

        [SerializeField] private GameSettings _settings;

        private void Start()
        {
            if (_settings != null) _settings.Load();
            BindUI();
            ApplySettings();
        }

        private void BindUI()
        {
            _musicSlider?.onValueChanged.AddListener(OnMusicVolume);
            _sfxSlider?.onValueChanged.AddListener(OnSfxVolume);
            _narrationSlider?.onValueChanged.AddListener(OnNarrationVolume);
            _musicToggle?.onValueChanged.AddListener(OnMusicToggle);
            _sfxToggle?.onValueChanged.AddListener(OnSfxToggle);
            _subtitlesToggle?.onValueChanged.AddListener(OnSubtitles);
            _highContrastToggle?.onValueChanged.AddListener(OnHighContrast);
            _reduceMotionToggle?.onValueChanged.AddListener(OnReduceMotion);
            _darkThemeToggle?.onValueChanged.AddListener(OnDarkTheme);
            _fontSmall?.onClick.AddListener(() => SetFont(0));
            _fontNormal?.onClick.AddListener(() => SetFont(1));
            _fontLarge?.onClick.AddListener(() => SetFont(2));
            _closeButton?.onClick.AddListener(() => gameObject.SetActive(false));
            _resetButton?.onClick.AddListener(ResetSettings);
        }

        private void ApplySettings()
        {
            if (_settings == null) return;
            if (_musicSlider != null) _musicSlider.value = _settings.musicVolume;
            if (_sfxSlider != null) _sfxSlider.value = _settings.sfxVolume;
            if (_narrationSlider != null) _narrationSlider.value = _settings.narrationVolume;
            if (_musicToggle != null) _musicToggle.isOn = _settings.musicEnabled;
            if (_sfxToggle != null) _sfxToggle.isOn = _settings.sfxEnabled;
            if (_subtitlesToggle != null) _subtitlesToggle.isOn = _settings.subtitlesEnabled;
            if (_highContrastToggle != null) _highContrastToggle.isOn = _settings.highContrast;
            if (_reduceMotionToggle != null) _reduceMotionToggle.isOn = _settings.reduceMotion;
            if (_darkThemeToggle != null) _darkThemeToggle.isOn = _settings.darkTheme;

            AudioManager.Instance?.SetMusicVolume(_settings.musicVolume);
            AudioManager.Instance?.SetSfxVolume(_settings.sfxVolume);
            AudioManager.Instance?.SetNarrationVolume(_settings.narrationVolume);
            HapticService.Instance?.SetReduceMotion(_settings.reduceMotion);
        }

        private void OnMusicVolume(float v) { if (_settings != null) { _settings.musicVolume = v; _settings.Save(); } AudioManager.Instance?.SetMusicVolume(v); }
        private void OnSfxVolume(float v) { if (_settings != null) { _settings.sfxVolume = v; _settings.Save(); } AudioManager.Instance?.SetSfxVolume(v); }
        private void OnNarrationVolume(float v) { if (_settings != null) { _settings.narrationVolume = v; _settings.Save(); } AudioManager.Instance?.SetNarrationVolume(v); }
        private void OnMusicToggle(bool b) { if (_settings != null) { _settings.musicEnabled = b; _settings.Save(); } if (!b) AudioManager.Instance?.StopMusic(fast: true); }
        private void OnSfxToggle(bool b) { if (_settings != null) { _settings.sfxEnabled = b; _settings.Save(); } }
        private void OnSubtitles(bool b) { if (_settings != null) { _settings.subtitlesEnabled = b; _settings.Save(); } }
        private void OnHighContrast(bool b) { if (_settings != null) { _settings.highContrast = b; _settings.Save(); } }
        private void OnReduceMotion(bool b) { if (_settings != null) { _settings.reduceMotion = b; _settings.Save(); } HapticService.Instance?.SetReduceMotion(b); }
        private void OnDarkTheme(bool b) { if (_settings != null) { _settings.darkTheme = b; _settings.Save(); } }
        private void SetFont(int level) { if (_settings != null) { _settings.fontSizeLevel = level; _settings.Save(); } }

        private void ResetSettings()
        {
            if (_settings != null)
            {
                _settings.musicVolume = 0.7f;
                _settings.sfxVolume = 1f;
                _settings.narrationVolume = 1f;
                _settings.musicEnabled = true;
                _settings.sfxEnabled = true;
                _settings.subtitlesEnabled = true;
                _settings.fontSizeLevel = 1;
                _settings.highContrast = false;
                _settings.reduceMotion = false;
                _settings.darkTheme = false;
                _settings.Save();
            }
            ApplySettings();
        }
    }
}
