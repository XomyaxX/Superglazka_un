using System;
using System.Collections.Generic;
using Superglazka.Data;
using UnityEngine;

namespace Superglazka.Services
{
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        [SerializeField] private List<AchievementData> _achievements;
        public event Action<AchievementData> OnAchievementUnlocked;

        private HashSet<string> _unlocked = new();
        private const string SAVE_KEY = "superglazka_achievements";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public void Check(AchievementType type, string value = "", int count = 1)
        {
            foreach (var ach in _achievements)
            {
                if (_unlocked.Contains(ach.key)) continue;
                if (ach.type != type) continue;

                bool shouldUnlock = false;
                switch (type)
                {
                    case AchievementType.Frame:
                    case AchievementType.Game:
                    case AchievementType.Episode:
                        shouldUnlock = string.IsNullOrEmpty(ach.titleKey) || value == ach.titleKey;
                        break;
                    case AchievementType.GameCount:
                    case AchievementType.Coins:
                        shouldUnlock = count >= ach.targetValue;
                        break;
                    case AchievementType.EpisodeAll:
                        shouldUnlock = count >= ach.targetValue;
                        break;
                }

                if (shouldUnlock)
                    Unlock(ach);
            }
        }

        public void Unlock(AchievementData achievement)
        {
            if (_unlocked.Contains(achievement.key)) return;
            _unlocked.Add(achievement.key);
            PlayerProfile.Instance?.AddCoins(achievement.rewardCoins);
            OnAchievementUnlocked?.Invoke(achievement);
            Save();
        }

        public bool IsUnlocked(string key) => _unlocked.Contains(key);
        public IReadOnlyList<AchievementData> AllAchievements => _achievements;
        public HashSet<string> UnlockedKeys => new(_unlocked);

        private void Save()
        {
            var json = JsonUtility.ToJson(new SaveData { unlocked = new List<string>(_unlocked) });
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            var json = PlayerPrefs.GetString(SAVE_KEY, "");
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data.unlocked != null)
                    _unlocked = new HashSet<string>(data.unlocked);
            }
            catch { }
        }

        [Serializable]
        private class SaveData
        {
            public List<string> unlocked;
        }
    }
}
