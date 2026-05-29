using Superglazka.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Superglazka.Games.Blink
{
    public class BlinkGame : BaseMinigame
    {
        [Header("Blink Game")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RawImage _eyeImage;
        [SerializeField] private RectTransform _barMarker;
        [SerializeField] private RectTransform _barZone;
        [SerializeField] private Text _roundText;
        [SerializeField] private Text _instructionText;
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _skipButton;

        [Header("Render Texture")]
        [SerializeField] private RenderTexture _eyeRenderTexture;

        private int _round;
        private int _clicks;
        private int _targetClicks;
        private float _timer;
        private bool _holding;
        private float _holdTimer;
        private float _markerPos;
        private float _markerDir = 1f;
        private bool _waitingForRelease;

        private enum RoundType { Blink, Squeeze, Wide }
        private RoundType[] _rounds = { RoundType.Blink, RoundType.Squeeze, RoundType.Wide };

        private Texture2D _eyeTexture;
        private Color _scleraColor = new Color(0.95f, 0.95f, 0.95f);
        private Color _irisColor1 = new Color(0.02f, 0.71f, 0.83f);
        private Color _irisColor2 = new Color(0.49f, 0.23f, 0.93f);

        private void Start()
        {
            _continueButton?.onClick.AddListener(() => { WinGame(); ContinueToEpisode(); });
            _skipButton?.onClick.AddListener(SkipGame);
            _eyeTexture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            _eyeTexture.filterMode = FilterMode.Bilinear;
            if (_eyeImage != null)
                _eyeImage.texture = _eyeTexture;
            StartGame();
        }

        public override void StartGame()
        {
            base.StartGame();
            var config = GameDifficulty.Instance?.GetConfig("blink");
            _targetClicks = config?.targetClicks ?? 8;
            _round = 0;
            StartRound();
            _winPanel?.SetActive(false);
        }

        private void StartRound()
        {
            if (_round >= _rounds.Length)
            {
                ShowWin();
                return;
            }
            _clicks = 0;
            _timer = 0f;
            _holding = false;
            _holdTimer = 0f;
            _waitingForRelease = false;
            _markerPos = 0f;
            _markerDir = 1f;
            UpdateUI();
        }

        protected override void OnUpdate()
        {
            if (!IsPlaying) return;
            _timer += Time.deltaTime;

            switch (_rounds[_round])
            {
                case RoundType.Blink:
                    UpdateBlinkRound();
                    break;
                case RoundType.Squeeze:
                    UpdateSqueezeRound();
                    break;
                case RoundType.Wide:
                    UpdateWideRound();
                    break;
            }

            DrawEye();
        }

        private void UpdateBlinkRound()
        {
            if (_timer >= 5f && _clicks < _targetClicks)
            {
                LoseGame();
                return;
            }
            if (_clicks >= _targetClicks)
            {
                _round++;
                StartRound();
            }
        }

        private void UpdateSqueezeRound()
        {
            if (_holding)
            {
                _holdTimer += Time.deltaTime;
                if (_holdTimer >= (GameDifficulty.Instance?.GetConfig("blink").holdTime ?? 3f))
                {
                    _round++;
                    StartRound();
                    HapticService.Instance?.VibrateSuccess();
                }
            }
            else if (_timer > 10f)
            {
                LoseGame();
            }
        }

        private void UpdateWideRound()
        {
            _markerPos += _markerDir * (GameDifficulty.Instance?.GetConfig("blink").speed ?? 1f) * Time.deltaTime;
            if (_markerPos > 1f) { _markerPos = 1f; _markerDir = -1f; }
            if (_markerPos < 0f) { _markerPos = 0f; _markerDir = 1f; }

            if (_barMarker != null)
            {
                float width = _barMarker.parent.GetComponent<RectTransform>().rect.width;
                _barMarker.anchoredPosition = new Vector2((_markerPos - 0.5f) * width, 0);
            }

            if (_timer > 10f)
            {
                LoseGame();
            }
        }

        private void DrawEye()
        {
            if (_eyeTexture == null) return;
            int w = _eyeTexture.width;
            int h = _eyeTexture.height;
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0.05f, 0.05f, 0.1f);

            Vector2 center = new Vector2(w * 0.5f, h * 0.5f);
            float radius = w * 0.35f;

            float blink = 1f;
            if (_rounds[_round] == RoundType.Blink)
                blink = 1f - Mathf.Abs(Mathf.Sin(_timer * 2f)) * 0.1f;
            else if (_rounds[_round] == RoundType.Squeeze)
                blink = _holding ? 0.05f : 1f;
            else if (_rounds[_round] == RoundType.Wide)
                blink = 1.15f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - center.x) / radius;
                    float dy = (y - center.y) / (radius * blink);
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    int idx = y * w + x;

                    if (dist < 1f)
                    {
                        pixels[idx] = _scleraColor;
                        float irisDist = Mathf.Sqrt(dx * dx + dy * dy);
                        if (irisDist < 0.5f)
                        {
                            pixels[idx] = Color.Lerp(_irisColor1, _irisColor2, irisDist * 2f);
                            if (irisDist < 0.22f)
                                pixels[idx] = Color.black;
                            else if (irisDist < 0.3f && x > center.x && y > center.y)
                                pixels[idx] = Color.white * 0.8f;
                        }
                    }
                }
            }
            _eyeTexture.SetPixels(pixels);
            _eyeTexture.Apply(false);
        }

        private void UpdateUI()
        {
            if (_roundText != null)
                _roundText.text = $"Раунд {_round + 1}/3";
            if (_instructionText != null)
            {
                _instructionText.text = _rounds[_round] switch
                {
                    RoundType.Blink => $"Кликайте {_targetClicks} раз!",
                    RoundType.Squeeze => "Зажмите и удерживайте!",
                    RoundType.Wide => "Попадите в зелёную зону!",
                    _ => ""
                };
            }
            _barMarker?.gameObject.SetActive(_rounds[_round] == RoundType.Wide);
            _barZone?.gameObject.SetActive(_rounds[_round] == RoundType.Wide);
        }

        private void OnMouseDown()
        {
            if (!IsPlaying) return;
            ProcessPress();
        }

        private void OnMouseUp()
        {
            if (!IsPlaying) return;
            ProcessRelease();
        }

        public void OnPointerDown()
        {
            if (!IsPlaying) return;
            ProcessPress();
        }

        public void OnPointerUp()
        {
            if (!IsPlaying) return;
            ProcessRelease();
        }

        private void ProcessPress()
        {
            switch (_rounds[_round])
            {
                case RoundType.Blink:
                    _clicks++;
                    HapticService.Instance?.VibrateLight();
                    break;
                case RoundType.Squeeze:
                    _holding = true;
                    break;
                case RoundType.Wide:
                    if (_markerPos >= 0.4f && _markerPos <= 0.6f)
                    {
                        _round++;
                        StartRound();
                        HapticService.Instance?.VibrateSuccess();
                    }
                    else
                    {
                        HapticService.Instance?.VibratePattern(new long[] { 30, 30 });
                    }
                    break;
            }
        }

        private void ProcessRelease()
        {
            if (_rounds[_round] == RoundType.Squeeze)
            {
                if (_holdTimer < (GameDifficulty.Instance?.GetConfig("blink").holdTime ?? 3f) - 0.2f)
                {
                    // Early release = fail
                    LoseGame();
                }
                _holding = false;
                _holdTimer = 0f;
            }
        }

        private void ShowWin()
        {
            _winPanel?.SetActive(true);
            Score = 100;
        }

        protected override void WinGame()
        {
            base.WinGame();
        }
    }
}
