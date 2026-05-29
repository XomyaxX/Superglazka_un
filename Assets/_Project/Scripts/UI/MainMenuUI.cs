using Superglazka.Data;
using Superglazka.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Superglazka.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Main Menu")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _booksButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _profileButton;
        [SerializeField] private Button _achievementsButton;
        [SerializeField] private Button _leaderboardButton;
        [SerializeField] private Text _coinsText;
        [SerializeField] private Text _welcomeText;
        [SerializeField] private GameObject _booksPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _profilePanel;
        [SerializeField] private Transform _booksContainer;
        [SerializeField] private GameObject _bookButtonPrefab;

        [SerializeField] private EpisodeDatabase _database;

        private void Start()
        {
            _continueButton?.onClick.AddListener(OnContinue);
            _booksButton?.onClick.AddListener(() => TogglePanel(_booksPanel));
            _settingsButton?.onClick.AddListener(() => TogglePanel(_settingsPanel));
            _profileButton?.onClick.AddListener(() => TogglePanel(_profilePanel));
            _achievementsButton?.onClick.AddListener(OnAchievements);
            _leaderboardButton?.onClick.AddListener(OnLeaderboard);

            UpdateUI();
            BuildBooksPanel();
            DailyRewardManager.Instance?.CheckNewDay();
        }

        private void UpdateUI()
        {
            if (_coinsText != null && PlayerProfile.Instance != null)
                _coinsText.text = $"{PlayerProfile.Instance.Coins}";
            if (_welcomeText != null)
            {
                string name = PlayerProfile.Instance?.Nickname;
                _welcomeText.text = string.IsNullOrEmpty(name)
                    ? (LocalizationService.Instance?.T("welcome") ?? "Добро пожаловать!")
                    : $"{LocalizationService.Instance?.T("hello") ?? "Привет"}, {name}!";
            }
        }

        private void BuildBooksPanel()
        {
            if (_booksContainer == null || _bookButtonPrefab == null || _database == null) return;
            foreach (Transform child in _booksContainer)
                Destroy(child.gameObject);

            foreach (var book in _database.Books)
            {
                var go = Instantiate(_bookButtonPrefab, _booksContainer);
                var btn = go.GetComponent<Button>();
                var txt = go.GetComponentInChildren<Text>();
                if (txt != null)
                    txt.text = LocalizationService.Instance?.Translate(book.titleKey) ?? book.titleKey;
                if (btn != null)
                {
                    string bookId = book.id;
                    btn.onClick.AddListener(() => ShowEpisodes(bookId));
                }
            }
        }

        private void ShowEpisodes(string bookId)
        {
            var book = _database.GetBook(bookId);
            if (book == null) return;
            // In a real implementation, show episode selection grid
            // For now, start first unlocked episode
            foreach (var ep in book.episodes)
            {
                var progress = PlayerProfile.Instance?.GetProgress(ep.id);
                if (!ep.isLocked || (progress?.completed ?? false) || PlayerProfile.Instance.Coins >= ep.requiredCoins)
                {
                    Core.GameManager.Instance?.GoToEpisode(ep.id);
                    return;
                }
            }
        }

        private void OnContinue()
        {
            HapticService.Instance?.VibrateLight();
            var (epId, _) = PlayerProfile.Instance?.GetLastPosition() ?? (null, 0);
            if (!string.IsNullOrEmpty(epId))
                Core.GameManager.Instance?.GoToEpisode(epId);
            else
                TogglePanel(_booksPanel);
        }

        private void TogglePanel(GameObject panel)
        {
            HapticService.Instance?.VibrateLight();
            bool active = panel.activeSelf;
            _booksPanel?.SetActive(false);
            _settingsPanel?.SetActive(false);
            _profilePanel?.SetActive(false);
            panel.SetActive(!active);
        }

        private void OnAchievements()
        {
            HapticService.Instance?.VibrateLight();
            // Show achievements panel
        }

        private void OnLeaderboard()
        {
            HapticService.Instance?.VibrateLight();
            // Show leaderboard panel
        }
    }
}
