using Superglazka.Services;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Superglazka.UI
{
    public class TutorialUI : MonoBehaviour
    {
        [System.Serializable]
        public class TutorialStep
        {
            public string targetPath;
            public string textKey;
            public TextAnchor position;
        }

        [System.Serializable]
        public class TutorialData
        {
            public string gameId;
            public List<TutorialStep> steps;
        }

        [SerializeField] private List<TutorialData> _tutorials;
        [SerializeField] private GameObject _overlay;
        [SerializeField] private GameObject _highlightPrefab;
        [SerializeField] private Text _tooltipText;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _skipButton;

        private int _currentStep;
        private TutorialData _currentTutorial;
        private GameObject _currentHighlight;

        private void Start()
        {
            _nextButton?.onClick.AddListener(NextStep);
            _skipButton?.onClick.AddListener(Skip);
            _overlay?.SetActive(false);
        }

        public void StartTutorial(string gameId)
        {
            if (TutorialManager.Instance != null && !TutorialManager.Instance.ShouldShow(gameId))
                return;

            _currentTutorial = _tutorials.Find(t => t.gameId == gameId);
            if (_currentTutorial == null || _currentTutorial.steps.Count == 0)
                return;

            _currentStep = 0;
            _overlay?.SetActive(true);
            ShowStep();
        }

        private void ShowStep()
        {
            if (_currentStep >= _currentTutorial.steps.Count)
            {
                Finish();
                return;
            }

            var step = _currentTutorial.steps[_currentStep];
            if (_tooltipText != null)
                _tooltipText.text = LocalizationService.Instance?.Translate(step.textKey) ?? step.textKey;

            if (_currentHighlight != null)
                Destroy(_currentHighlight);

            var target = GameObject.Find(step.targetPath);
            if (target != null && _highlightPrefab != null)
            {
                _currentHighlight = Instantiate(_highlightPrefab, _overlay.transform);
                var rt = _currentHighlight.GetComponent<RectTransform>();
                var targetRt = target.GetComponent<RectTransform>();
                if (rt != null && targetRt != null)
                {
                    rt.position = targetRt.position;
                    rt.sizeDelta = targetRt.sizeDelta;
                }
            }
        }

        private void NextStep()
        {
            _currentStep++;
            HapticService.Instance?.VibrateLight();
            ShowStep();
        }

        private void Skip()
        {
            HapticService.Instance?.VibrateLight();
            Finish();
        }

        private void Finish()
        {
            if (_currentTutorial != null)
                TutorialManager.Instance?.MarkCompleted(_currentTutorial.gameId);
            if (_currentHighlight != null)
                Destroy(_currentHighlight);
            _overlay?.SetActive(false);
        }
    }
}
