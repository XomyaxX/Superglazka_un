using System.Collections.Generic;
using UnityEngine;

namespace Superglazka.Services
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        private HashSet<string> _completed = new();

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

        public bool ShouldShow(string gameId)
        {
            if (!PlayerPrefs.HasKey("superglazka_tutorial_enabled"))
                return true;
            if (PlayerPrefs.GetInt("superglazka_tutorial_enabled", 1) == 0)
                return false;
            return !_completed.Contains(gameId);
        }

        public void MarkCompleted(string gameId)
        {
            _completed.Add(gameId);
            Save();
        }

        public void Reset(string gameId)
        {
            _completed.Remove(gameId);
            Save();
        }

        private void Save()
        {
            var list = new List<string>(_completed);
            PlayerPrefs.SetString("superglazka_tutorials", string.Join(",", list));
            PlayerPrefs.Save();
        }

        private void Load()
        {
            var str = PlayerPrefs.GetString("superglazka_tutorials", "");
            if (!string.IsNullOrEmpty(str))
                _completed = new HashSet<string>(str.Split(','));
        }
    }
}
