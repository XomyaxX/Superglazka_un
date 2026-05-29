using System.Collections.Generic;
using Superglazka.Data;
using UnityEngine;

namespace Superglazka.Services
{
    public class LocalizationService : MonoBehaviour
    {
        public static LocalizationService Instance { get; private set; }

        [SerializeField] private LocaleDatabase _localeDatabase;
        [SerializeField] private SystemLanguage _defaultLanguage = SystemLanguage.Russian;

        private Dictionary<string, LocaleEntry> _dict;
        private SystemLanguage _currentLang;

        public SystemLanguage CurrentLanguage => _currentLang;
        public event System.Action OnLanguageChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildDictionary();
            LoadLanguage();
        }

        private void BuildDictionary()
        {
            _dict = new Dictionary<string, LocaleEntry>();
            if (_localeDatabase == null || _localeDatabase.Entries == null) return;
            foreach (var entry in _localeDatabase.Entries)
            {
                if (!string.IsNullOrEmpty(entry.key))
                    _dict[entry.key] = new LocaleEntry
                    {
                        key = entry.key,
                        ru = entry.ru,
                        en = entry.en,
                        kz = entry.kz,
                        zh = entry.zh
                    };
            }
        }

        private void LoadLanguage()
        {
            var saved = PlayerPrefs.GetString("superglazka_lang", "");
            if (!string.IsNullOrEmpty(saved) && System.Enum.TryParse<SystemLanguage>(saved, out var lang))
            {
                _currentLang = lang;
            }
            else
            {
                _currentLang = Application.systemLanguage == SystemLanguage.Russian
                    || Application.systemLanguage == SystemLanguage.English
                    || Application.systemLanguage == SystemLanguage.Chinese
                    || Application.systemLanguage == SystemLanguage.Kazakh
                    ? Application.systemLanguage
                    : _defaultLanguage;
            }
        }

        public void SetLanguage(SystemLanguage lang)
        {
            if (_currentLang == lang) return;
            _currentLang = lang;
            PlayerPrefs.SetString("superglazka_lang", lang.ToString());
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke();
        }

        public string Translate(string key, Dictionary<string, string> vars = null)
        {
            if (!_dict.TryGetValue(key, out var entry))
                return key;

            string text = _currentLang switch
            {
                SystemLanguage.Russian => entry.ru,
                SystemLanguage.English => entry.en,
                SystemLanguage.Kazakh => entry.kz,
                SystemLanguage.Chinese => entry.zh,
                _ => entry.ru,
            };

            if (vars != null)
            {
                foreach (var kv in vars)
                    text = text.Replace($"{{{{{kv.Key}}}}}", kv.Value);
            }
            return string.IsNullOrEmpty(text) ? key : text;
        }

        public string T(string key) => Translate(key);

        private class LocaleEntry
        {
            public string key;
            public string ru;
            public string en;
            public string kz;
            public string zh;
        }
    }
}
