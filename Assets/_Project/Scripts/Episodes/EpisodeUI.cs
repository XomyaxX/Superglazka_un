using Superglazka.Data;
using Superglazka.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Superglazka.Episodes
{
    public class EpisodeUI : MonoBehaviour
    {
        [Header("Frame Display")]
        [SerializeField] private Image _bgImage;
        [SerializeField] private RectTransform _frameContainer;
        [SerializeField] private TextMeshProUGUI _narrationText;
        [SerializeField] private TextMeshProUGUI _subtitleText;

        [Header("Navigation")]
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private GameObject _progressDotsContainer;
        [SerializeField] private GameObject _progressDotPrefab;

        [Header("Game Island")]
        [SerializeField] private GameObject _gameIslandPanel;
        [SerializeField] private TextMeshProUGUI _gameTitleText;
        [SerializeField] private Button _playGameButton;
        [SerializeField] private Button _skipGameButton;

        [Header("End Screen")]
        [SerializeField] private GameObject _endScreenPanel;
        [SerializeField] private TextMeshProUGUI _endText;
        [SerializeField] private Button _endContinueButton;

        [Header("Settings")]
        [SerializeField] private float _typewriterSpeed = 40f;
        [SerializeField] private bool _autoAdvance;

        private float _typewriterTimer;
        private int _typewriterIndex;
        private string _currentNarration;
        private bool _isTyping;

        private void Start()
        {
            _nextButton?.onClick.AddListener(OnNextClicked);
            _prevButton?.onClick.AddListener(OnPrevClicked);
            _menuButton?.onClick.AddListener(OnMenuClicked);
            _playGameButton?.onClick.AddListener(OnPlayGameClicked);
            _skipGameButton?.onClick.AddListener(OnSkipGameClicked);
            _endContinueButton?.onClick.AddListener(OnEndContinueClicked);
            HideAllPanels();
        }

        public void ShowFrame(FrameData frame)
        {
            HideAllPanels();
            _nextButton.gameObject.SetActive(false);

            Sprite sprite = frame.bgImageMobile != null && Application.isMobilePlatform
                ? frame.bgImageMobile
                : frame.bgImage;
            if (_bgImage != null && sprite != null)
                _bgImage.sprite = sprite;

            _currentNarration = LocalizationService.Instance?.Translate(frame.narrationKey) ?? frame.narrationKey;
            _typewriterIndex = 0;
            _typewriterTimer = 0f;
            _isTyping = true;
            _narrationText.text = "";
            _subtitleText.text = "";

            UpdateProgressDots();
        }

        private void Update()
        {
            if (_isTyping && !string.IsNullOrEmpty(_currentNarration))
            {
                _typewriterTimer += Time.deltaTime;
                int charsToShow = Mathf.FloorToInt(_typewriterTimer * _typewriterSpeed);
                if (charsToShow > _typewriterIndex)
                {
                    _typewriterIndex = Mathf.Min(charsToShow, _currentNarration.Length);
                    _narrationText.text = _currentNarration.Substring(0, _typewriterIndex);
                    _subtitleText.text = _narrationText.text;
                    if (_typewriterIndex >= _currentNarration.Length)
                    {
                        _isTyping = false;
                        EpisodeManager.Instance?.MarkTextComplete();
                    }
                }
            }
        }

        public void ShowNextButton()
        {
            _nextButton.gameObject.SetActive(true);
        }

        public void ShowGameIsland(string gameId, string transitionKey)
        {
            _gameIslandPanel.SetActive(true);
            _gameTitleText.text = LocalizationService.Instance?.Translate(transitionKey) ?? "Мини-игра!";
        }

        public void ShowEndScreen()
        {
            _endScreenPanel.SetActive(true);
            _endText.text = LocalizationService.Instance?.Translate("episode_complete") ?? "Глава завершена!";
        }

        private void HideAllPanels()
        {
            _gameIslandPanel?.SetActive(false);
            _endScreenPanel?.SetActive(false);
            _nextButton?.gameObject.SetActive(false);
        }

        private void UpdateProgressDots()
        {
            if (_progressDotsContainer == null || _progressDotPrefab == null) return;
            foreach (Transform child in _progressDotsContainer.transform)
                Destroy(child.gameObject);

            var ep = EpisodeManager.Instance?.CurrentEpisode;
            var mgr = EpisodeManager.Instance;
            if (ep == null || mgr == null) return;
            for (int i = 0; i < ep.frames.Count; i++)
            {
                var dot = Instantiate(_progressDotPrefab, _progressDotsContainer.transform);
                var img = dot.GetComponent<Image>();
                if (img != null)
                    img.color = i == mgr.CurrentFrameIndex ? Color.white : new Color(1, 1, 1, 0.4f);
            }
        }

        private void OnNextClicked()
        {
            HapticService.Instance?.VibrateLight();
            EpisodeManager.Instance?.NextFrame();
        }

        private void OnPrevClicked()
        {
            HapticService.Instance?.VibrateLight();
            EpisodeManager.Instance?.PrevFrame();
        }

        private void OnMenuClicked()
        {
            HapticService.Instance?.VibrateLight();
            Core.GameManager.Instance?.GoToMainMenu();
        }

        private void OnPlayGameClicked()
        {
            HapticService.Instance?.VibrateSuccess();
            EpisodeManager.Instance?.LaunchGame();
        }

        private void OnSkipGameClicked()
        {
            HapticService.Instance?.VibrateLight();
            EpisodeManager.Instance?.SkipGame();
        }

        private void OnEndContinueClicked()
        {
            HapticService.Instance?.VibrateSuccess();
            Core.GameManager.Instance?.GoToMainMenu();
        }
    }
}
