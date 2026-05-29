using System;
using System.Collections.Generic;
using System.Linq;
using Superglazka.Data;
using UnityEngine;

namespace Superglazka.Services
{
    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        private const string SAVE_KEY = "superglazka_leaderboard";
        private Dictionary<string, List<LeaderboardEntry>> _weekly = new();
        private Dictionary<string, List<LeaderboardEntry>> _allTime = new();

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

        public void RecordScore(string gameName, int score)
        {
            var name = PlayerProfile.Instance?.Nickname ?? "Player";
            var entry = new LeaderboardEntry
            {
                name = name,
                score = score,
                date = DateTime.Now.ToString("yyyy-MM-dd")
            };

            if (!_weekly.ContainsKey(gameName)) _weekly[gameName] = new List<LeaderboardEntry>();
            if (!_allTime.ContainsKey(gameName)) _allTime[gameName] = new List<LeaderboardEntry>();

            _weekly[gameName].Add(entry);
            _allTime[gameName].Add(entry);

            _weekly[gameName] = _weekly[gameName].OrderByDescending(e => e.score).Take(10).ToList();
            _allTime[gameName] = _allTime[gameName].OrderByDescending(e => e.score).Take(20).ToList();

            Save();
        }

        public IReadOnlyList<LeaderboardEntry> GetTop5(string gameName, bool weekly = false)
        {
            var dict = weekly ? _weekly : _allTime;
            if (!dict.TryGetValue(gameName, out var list)) return new List<LeaderboardEntry>();
            return list.Take(5).ToList();
        }

        private void Save()
        {
            var data = new SaveContainer
            {
                weekly = SerializeDict(_weekly),
                allTime = SerializeDict(_allTime)
            };
            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        private void Load()
        {
            var json = PlayerPrefs.GetString(SAVE_KEY, "");
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var data = JsonUtility.FromJson<SaveContainer>(json);
                _weekly = DeserializeDict(data.weekly);
                _allTime = DeserializeDict(data.allTime);
            }
            catch { }
        }

        private static List<SerialEntry> SerializeDict(Dictionary<string, List<LeaderboardEntry>> dict)
        {
            var result = new List<SerialEntry>();
            foreach (var kv in dict)
            {
                result.Add(new SerialEntry { game = kv.Key, entries = kv.Value });
            }
            return result;
        }

        private static Dictionary<string, List<LeaderboardEntry>> DeserializeDict(List<SerialEntry> list)
        {
            var dict = new Dictionary<string, List<LeaderboardEntry>>();
            if (list == null) return dict;
            foreach (var se in list)
            {
                if (se.entries != null)
                    dict[se.game] = se.entries;
            }
            return dict;
        }

        [Serializable]
        private class SaveContainer
        {
            public List<SerialEntry> weekly;
            public List<SerialEntry> allTime;
        }

        [Serializable]
        private class SerialEntry
        {
            public string game;
            public List<LeaderboardEntry> entries;
        }
    }
}
