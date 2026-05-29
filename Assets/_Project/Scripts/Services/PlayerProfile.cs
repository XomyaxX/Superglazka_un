using System;
using System.Collections.Generic;
using System.Linq;
using Superglazka.Data;
using UnityEngine;

namespace Superglazka.Services
{
    public class PlayerProfile : MonoBehaviour
    {
        public static PlayerProfile Instance { get; private set; }

        public string Nickname { get; private set; }
        public int Coins { get; private set; }
        public event Action OnCoinsChanged;
        public event Action<string> OnEpisodeCompleted;
        public event Action<string, int> OnGameCompleted;

        private Dictionary<string, EpisodeProgress> _episodes = new();
        private Dictionary<string, GameStats> _games = new();

        private const string SAVE_KEY = "superglazka_profile";

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

        public void SetNickname(string name)
        {
            Nickname = name;
            Save();
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            Coins += amount;
            OnCoinsChanged?.Invoke();
            Save();
        }

        public bool SpendCoins(int amount)
        {
            if (amount <= 0) return true;
            if (Coins < amount) return false;
            Coins -= amount;
            OnCoinsChanged?.Invoke();
            Save();
            return true;
        }

        public void MarkFrameSeen(string episodeId, int frameIdx)
        {
            if (!_episodes.TryGetValue(episodeId, out var progress))
            {
                progress = new EpisodeProgress();
                _episodes[episodeId] = progress;
            }
            progress.framesSeen = Mathf.Max(progress.framesSeen, frameIdx + 1);
            progress.maxFrame = Mathf.Max(progress.maxFrame, frameIdx);
            Save();
        }

        public void CompleteEpisode(string episodeId)
        {
            if (!_episodes.TryGetValue(episodeId, out var progress))
            {
                progress = new EpisodeProgress();
                _episodes[episodeId] = progress;
            }
            progress.completed = true;
            OnEpisodeCompleted?.Invoke(episodeId);
            Save();
        }

        public EpisodeProgress GetProgress(string episodeId)
        {
            if (_episodes.TryGetValue(episodeId, out var p)) return p;
            return new EpisodeProgress();
        }

        public void CompleteGame(string gameName, int score)
        {
            if (!_games.TryGetValue(gameName, out var stats))
            {
                stats = new GameStats();
                _games[gameName] = stats;
            }
            stats.played++;
            if (score > stats.bestScore)
                stats.bestScore = score;
            OnGameCompleted?.Invoke(gameName, score);
            Save();
        }

        public GameStats GetGameStats(string gameName)
        {
            if (_games.TryGetValue(gameName, out var s)) return s;
            return new GameStats();
        }

        public PlayerProfileData GetProfileData()
        {
            return new PlayerProfileData
            {
                nickname = Nickname,
                coins = Coins,
                episodes = new Dictionary<string, EpisodeProgress>(_episodes),
                games = new Dictionary<string, GameStats>(_games)
            };
        }

        public (string episodeId, int frameIdx) GetLastPosition()
        {
            string lastEp = null;
            int lastFrame = -1;
            foreach (var kv in _episodes)
            {
                if (kv.Value.maxFrame > lastFrame)
                {
                    lastFrame = kv.Value.maxFrame;
                    lastEp = kv.Key;
                }
            }
            return (lastEp, lastFrame);
        }

        private void Save()
        {
            var data = new SaveContainer
            {
                nickname = Nickname,
                coins = Coins,
                episodes = _episodes.Select(kv => new EpisodeEntry { id = kv.Key, completed = kv.Value.completed, framesSeen = kv.Value.framesSeen, maxFrame = kv.Value.maxFrame }).ToList(),
                games = _games.Select(kv => new GameEntry { name = kv.Key, played = kv.Value.played, bestScore = kv.Value.bestScore }).ToList()
            };
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            var json = PlayerPrefs.GetString(SAVE_KEY, "");
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var data = JsonUtility.FromJson<SaveContainer>(json);
                Nickname = data.nickname ?? "";
                Coins = data.coins;
                _episodes = new Dictionary<string, EpisodeProgress>();
                if (data.episodes != null)
                {
                    foreach (var e in data.episodes)
                        _episodes[e.id] = new EpisodeProgress { completed = e.completed, framesSeen = e.framesSeen, maxFrame = e.maxFrame };
                }
                _games = new Dictionary<string, GameStats>();
                if (data.games != null)
                {
                    foreach (var g in data.games)
                        _games[g.name] = new GameStats { played = g.played, bestScore = g.bestScore };
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"PlayerProfile load failed: {e.Message}");
            }
        }

        [Serializable]
        private class SaveContainer
        {
            public string nickname;
            public int coins;
            public List<EpisodeEntry> episodes;
            public List<GameEntry> games;
        }

        [Serializable]
        private class EpisodeEntry
        {
            public string id;
            public bool completed;
            public int framesSeen;
            public int maxFrame;
        }

        [Serializable]
        private class GameEntry
        {
            public string name;
            public int played;
            public int bestScore;
        }
    }
}
