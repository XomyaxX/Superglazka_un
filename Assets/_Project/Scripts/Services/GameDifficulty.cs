using System.Collections.Generic;
using UnityEngine;

namespace Superglazka.Services
{
    public class GameDifficulty : MonoBehaviour
    {
        public static GameDifficulty Instance { get; private set; }

        private Dictionary<string, int> _levels = new();
        private const string SAVE_KEY = "superglazka_difficulty";

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

        public int GetLevel(string gameId)
        {
            return _levels.TryGetValue(gameId, out var level) ? level : 0;
        }

        public void IncreaseLevel(string gameId)
        {
            if (!_levels.ContainsKey(gameId)) _levels[gameId] = 0;
            _levels[gameId]++;
            Save();
        }

        public DifficultyConfig GetConfig(string gameId)
        {
            int level = GetLevel(gameId);
            return gameId switch
            {
                "blink" => new DifficultyConfig { rounds = 3, targetClicks = 8 + level * 2, holdTime = 3f - level * 0.1f, speed = 1f + level * 0.1f },
                "gym" => new DifficultyConfig { rounds = 3, targetHits = new[] { 8, 5, 4 }, hp = 100 + level * 10 },
                "peripheral" => new DifficultyConfig { duration = 60f, lives = 3, spawnRate = Mathf.Max(0.5f, 1.5f - level * 0.1f), maxTargets = 3 + level },
                "runner" => new DifficultyConfig { targetDist = 2000 + level * 200, speed = 5f + level * 0.5f, speedCap = 12f + level },
                "scrollshooter" => new DifficultyConfig { wave = 1 + level, hp = 100 + level * 20 },
                "tracker" => new DifficultyConfig { rounds = 4 + level, duration = 8f, radius = 0.3f + level * 0.02f },
                _ => new DifficultyConfig(),
            };
        }

        private void Save()
        {
            var json = JsonUtility.ToJson(new SaveData { entries = new List<Entry>() });
            // Simplified: use PlayerPrefs string list
            var sb = new System.Text.StringBuilder();
            foreach (var kv in _levels)
                sb.Append($"{kv.Key}={kv.Value};");
            PlayerPrefs.SetString(SAVE_KEY, sb.ToString());
            PlayerPrefs.Save();
        }

        private void Load()
        {
            var str = PlayerPrefs.GetString(SAVE_KEY, "");
            if (string.IsNullOrEmpty(str)) return;
            var parts = str.Split(';');
            foreach (var part in parts)
            {
                var kv = part.Split('=');
                if (kv.Length == 2 && int.TryParse(kv[1], out var lvl))
                    _levels[kv[0]] = lvl;
            }
        }

        [System.Serializable]
        public class DifficultyConfig
        {
            public int rounds;
            public int targetClicks;
            public float holdTime;
            public float speed;
            public int[] targetHits;
            public int hp;
            public float duration;
            public int lives;
            public float spawnRate;
            public int maxTargets;
            public float targetDist;
            public float speedCap;
            public int wave;
            public float radius;
        }

        [System.Serializable]
        private class SaveData
        {
            public List<Entry> entries;
        }

        [System.Serializable]
        private class Entry
        {
            public string game;
            public int level;
        }
    }
}
