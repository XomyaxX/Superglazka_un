using Superglazka.Data;
using Superglazka.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Superglazka.UI
{
    public class LeaderboardUI : MonoBehaviour
    {
        [SerializeField] private Transform _listContainer;
        [SerializeField] private GameObject _entryPrefab;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _weeklyButton;
        [SerializeField] private Button _allTimeButton;
        [SerializeField] private Text _titleText;

        [SerializeField] private string _gameId = "runner";
        private bool _weekly = true;

        private void OnEnable()
        {
            Refresh();
        }

        private void Start()
        {
            _closeButton?.onClick.AddListener(() => gameObject.SetActive(false));
            _weeklyButton?.onClick.AddListener(() => { _weekly = true; Refresh(); });
            _allTimeButton?.onClick.AddListener(() => { _weekly = false; Refresh(); });
        }

        private void Refresh()
        {
            if (_listContainer == null || _entryPrefab == null) return;
            foreach (Transform child in _listContainer)
                Destroy(child.gameObject);

            var entries = LeaderboardManager.Instance?.GetTop5(_gameId, _weekly);
            if (entries == null) return;

            int rank = 1;
            foreach (var entry in entries)
            {
                var go = Instantiate(_entryPrefab, _listContainer);
                var rankText = go.transform.Find("Rank")?.GetComponent<Text>();
                var nameText = go.transform.Find("Name")?.GetComponent<Text>();
                var scoreText = go.transform.Find("Score")?.GetComponent<Text>();

                if (rankText != null) rankText.text = $"#{rank}";
                if (nameText != null) nameText.text = entry.name;
                if (scoreText != null) scoreText.text = $"{entry.score}";
                rank++;
            }

            if (_titleText != null)
                _titleText.text = LocalizationService.Instance?.Translate(_weekly ? "weekly" : "all_time") ?? (_weekly ? "Неделя" : "Всё время");
        }

        public void SetGame(string gameId)
        {
            _gameId = gameId;
            Refresh();
        }
    }
}
