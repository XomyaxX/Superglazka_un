using System.Collections;
using Superglazka.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Superglazka.Games.Gym
{
    public class GymGame : BaseMinigame
    {
        [Header("Gym")]
        [SerializeField] private Image _bossImage;
        [SerializeField] private Slider _bossHpSlider;
        [SerializeField] private Text _phaseText;
        [SerializeField] private Text _instructionText;
        [SerializeField] private Button _actionButton;
        [SerializeField] private RectTransform _aimArea;
        [SerializeField] private RectTransform _aimCursor;
        [SerializeField] private RectTransform _aimTarget;
        [SerializeField] private GameObject _laserBeam;
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private GameObject _losePanel;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _skipButton;

        private int _phase;
        private int _bossHp;
        private int _bossMaxHp;
        private int[] _hitsRequired;
        private int _hitsDone;
        private int _attempts;
        private float _phaseTimer;
        private bool _charging;
        private float _chargeTime;
        private float _targetHoldTime = 2f;
        private float _aimHoldTime;
        private bool _aimLocked;
        private float _laserDuration;

        private enum PhaseType { Laser, Aim, Tears }
        private PhaseType CurrentPhase => (PhaseType)_phase;

        private void Start()
        {
            _continueButton?.onClick.AddListener(() => { WinGame(); ContinueToEpisode(); });
            _skipButton?.onClick.AddListener(SkipGame);
            _actionButton?.onClick.AddListener(OnActionPressed);
            StartGame();
        }

        public override void StartGame()
        {
            base.StartGame();
            var config = GameDifficulty.Instance?.GetConfig("gym");
            _hitsRequired = config?.targetHits ?? new[] { 8, 5, 4 };
            _phase = 0;
            _hitsDone = 0;
            _attempts = 0;
            _charging = false;
            _chargeTime = 0f;
            _aimHoldTime = 0f;
            _aimLocked = false;
            Score = 0;
            SetupPhase();
            _winPanel?.SetActive(false);
            _losePanel?.SetActive(false);
            _actionButton.gameObject.SetActive(true);
        }

        private void SetupPhase()
        {
            if (_phase >= 3)
            {
                ShowWin();
                return;
            }
            _bossHp = 100;
            _bossMaxHp = 100;
            _hitsDone = 0;
            _attempts = 0;
            _charging = false;
            _chargeTime = 0f;
            _aimHoldTime = 0f;
            _aimLocked = false;
            _phaseTimer = 0f;
            UpdateUI();
            UpdatePhaseUI();
        }

        protected override void OnUpdate()
        {
            if (!IsPlaying) return;
            _phaseTimer += Time.deltaTime;

            if (CurrentPhase == PhaseType.Aim)
            {
                UpdateAimPhase();
            }

            if (_bossHpSlider != null)
                _bossHpSlider.value = (float)_bossHp / _bossMaxHp;
        }

        private void UpdateAimPhase()
        {
            if (_aimLocked)
            {
                _aimHoldTime += Time.deltaTime;
                if (_aimHoldTime >= 1.5f)
                {
                    RegisterHit(1.2f);
                    _aimLocked = false;
                    _aimHoldTime = 0f;
                    MoveAimTarget();
                    HapticService.Instance?.VibrateSuccess();
                }
                return;
            }

            if (GameInput.Instance != null && GameInput.Instance.TouchHeld)
            {
                Vector2 localPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _aimArea, GameInput.Instance.TouchPosition, null, out localPos);
                _aimCursor.anchoredPosition = localPos;

                float dist = Vector2.Distance(localPos, _aimTarget.anchoredPosition);
                if (dist < 30f)
                {
                    _aimLocked = true;
                    _aimHoldTime = 0f;
                }
            }
        }

        private void OnActionPressed()
        {
            if (!IsPlaying) return;
            switch (CurrentPhase)
            {
                case PhaseType.Laser:
                    if (!_charging)
                    {
                        _charging = true;
                        _chargeTime = 0f;
                        StartCoroutine(LaserChargeRoutine());
                    }
                    break;
                case PhaseType.Tears:
                    _attempts++;
                    _hitsDone++;
                    if (_hitsDone >= _hitsRequired[_phase])
                    {
                        RegisterHit(1f);
                        if (_hitsDone >= _hitsRequired[_phase] * 4)
                        {
                            _phase++;
                            SetupPhase();
                        }
                    }
                    if (_attempts % 5 == 0)
                        SpawnWaterfall();
                    HapticService.Instance?.VibrateLight();
                    break;
            }
        }

        private IEnumerator LaserChargeRoutine()
        {
            float timer = 0f;
            while (_charging && timer < _targetHoldTime + 1f)
            {
                timer += Time.deltaTime;
                if (timer >= _targetHoldTime)
                {
                    // Success if still holding
                }
                yield return null;
            }
            if (_charging && timer >= _targetHoldTime - 0.2f && timer <= _targetHoldTime + 0.5f)
            {
                _charging = false;
                _hitsDone++;
                RegisterHit(1f);
                ShowLaserEffect();
                if (_hitsDone >= _hitsRequired[_phase])
                {
                    _phase++;
                    SetupPhase();
                }
            }
            else if (_charging)
            {
                _charging = false;
                // Undercharge
            }
        }

        public void OnActionReleased()
        {
            if (CurrentPhase == PhaseType.Laser && _charging)
            {
                _charging = false;
                StopAllCoroutines();
            }
        }

        private void RegisterHit(float multiplier)
        {
            int damage = Mathf.RoundToInt(100f / _hitsRequired[_phase] * multiplier);
            _bossHp = Mathf.Max(0, _bossHp - damage);
            StartCoroutine(ShakeBoss());
            if (_bossHp <= 0)
            {
                _phase++;
                SetupPhase();
            }
        }

        private IEnumerator ShakeBoss()
        {
            Vector2 orig = _bossImage.rectTransform.anchoredPosition;
            for (int i = 0; i < 8; i++)
            {
                _bossImage.rectTransform.anchoredPosition = orig + Random.insideUnitCircle * 8f;
                yield return new WaitForSeconds(0.03f);
            }
            _bossImage.rectTransform.anchoredPosition = orig;
        }

        private void ShowLaserEffect()
        {
            _laserBeam?.SetActive(true);
            Invoke(nameof(HideLaser), 0.3f);
        }

        private void HideLaser() => _laserBeam?.SetActive(false);

        private void SpawnWaterfall()
        {
            // Visual waterfall particles
        }

        private void MoveAimTarget()
        {
            if (_aimArea == null) return;
            Rect r = _aimArea.rect;
            _aimTarget.anchoredPosition = new Vector2(
                Random.Range(-r.width * 0.4f, r.width * 0.4f),
                Random.Range(-r.height * 0.4f, r.height * 0.4f)
            );
        }

        private void UpdateUI()
        {
            if (_phaseText != null)
                _phaseText.text = $"Фаза {_phase + 1}/3";
            if (_instructionText != null)
            {
                _instructionText.text = CurrentPhase switch
                {
                    PhaseType.Laser => "Зажмите кнопку для зарядки лазера!",
                    PhaseType.Aim => "Наведите прицел на мишень и удерживайте!",
                    PhaseType.Tears => "Быстро нажимайте кнопку!",
                    _ => ""
                };
            }
            _aimArea?.gameObject.SetActive(CurrentPhase == PhaseType.Aim);
            _aimCursor?.gameObject.SetActive(CurrentPhase == PhaseType.Aim);
            _aimTarget?.gameObject.SetActive(CurrentPhase == PhaseType.Aim);
        }

        private void UpdatePhaseUI()
        {
            if (_bossHpSlider != null)
            {
                _bossHpSlider.value = 1f;
                _bossHpSlider.gameObject.SetActive(CurrentPhase != PhaseType.Tears);
            }
        }

        private void ShowWin()
        {
            Score = 1500 + _hitsDone * 15;
            _winPanel?.SetActive(true);
        }

        protected override void WinGame()
        {
            base.WinGame();
        }
    }
}
