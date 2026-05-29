using System;
using UnityEngine;

namespace Superglazka.Services
{
    public class DailyRewardManager : MonoBehaviour
    {
        public static DailyRewardManager Instance { get; private set; }

        [SerializeField] private int[] _rewards = { 10, 15, 20, 25, 30, 50, 50 };

        public event Action<int, int> OnRewardClaimed;

        private const string SAVE_KEY = "superglazka_daily";
        private DateTime _lastDate;
        private int _streak;
        private bool _claimedToday;

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

        public (int streak, int nextStreak, bool canClaim, bool claimedToday, int reward) GetStatus()
        {
            var today = DateTime.Now.Date;
            bool canClaim = !_claimedToday;
            int reward = _rewards[Mathf.Min(_streak, _rewards.Length - 1)];
            return (_streak, Mathf.Min(_streak + 1, _rewards.Length), canClaim, _claimedToday, reward);
        }

        public int? Claim()
        {
            var today = DateTime.Now.Date;
            if (_claimedToday) return null;

            if (_lastDate != default && (today - _lastDate).Days == 1)
                _streak++;
            else if (_lastDate == default || (today - _lastDate).Days > 1)
                _streak = 0;

            _claimedToday = true;
            _lastDate = today;
            int reward = _rewards[Mathf.Min(_streak, _rewards.Length - 1)];
            PlayerProfile.Instance?.AddCoins(reward);
            OnRewardClaimed?.Invoke(_streak, reward);
            Save();
            return reward;
        }

        public void CheckNewDay()
        {
            var today = DateTime.Now.Date;
            if (_lastDate != today)
                _claimedToday = false;
        }

        private void Save()
        {
            var data = new SaveData
            {
                lastDate = _lastDate.ToString("yyyy-MM-dd"),
                streak = _streak,
                claimed = _claimedToday
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
                var data = JsonUtility.FromJson<SaveData>(json);
                if (DateTime.TryParse(data.lastDate, out var date))
                    _lastDate = date;
                _streak = data.streak;
                _claimedToday = data.claimed;
                CheckNewDay();
            }
            catch { }
        }

        [Serializable]
        private class SaveData
        {
            public string lastDate;
            public int streak;
            public bool claimed;
        }
    }
}
