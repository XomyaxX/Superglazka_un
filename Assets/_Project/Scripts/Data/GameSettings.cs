using UnityEngine;

namespace Superglazka.Data
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Superglazka/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Audio")]
        [Range(0f, 1f)] public float musicVolume = 0.7f;
        [Range(0f, 1f)] public float sfxVolume = 1f;
        [Range(0f, 1f)] public float narrationVolume = 1f;
        public bool musicEnabled = true;
        public bool sfxEnabled = true;

        [Header("Visual")]
        public bool subtitlesEnabled = true;
        public int fontSizeLevel = 1; // 0=small, 1=normal, 2=large
        public bool highContrast;
        public bool reduceMotion;
        public bool darkTheme;

        [Header("Gameplay")]
        public string language = "ru";
        public bool tutorialEnabled = true;
        public bool hapticEnabled = true;

        public void Save()
        {
            var json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString("superglazka_settings", json);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            var json = PlayerPrefs.GetString("superglazka_settings", "");
            if (!string.IsNullOrEmpty(json))
                JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}
