using Superglazka.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Superglazka.Games.Tracker
{
    public class TrackerGame : BaseMinigame
    {
        [Header("Tracker")]
        [SerializeField] private RawImage _display;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Text _statusText;
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _skipButton;

        private Texture2D _texture;
        private float _phaseTimer;
        private int _currentPhase;
        private int _totalPhases;
        private float _phaseDuration = 8f;
        private float _pauseDuration = 1.5f;
        private bool _inPause;
        private Vector2 _ballPos;
        private Vector2[] _trail;
        private int _trailLength = 6;

        private enum Pattern { UpDown, LeftRight, Clockwise, CounterClockwise, Figure8, ZigZag, Spiral, Random }
        private Pattern[] _patterns;

        private void Start()
        {
            _continueButton?.onClick.AddListener(() => { WinGame(); ContinueToEpisode(); });
            _skipButton?.onClick.AddListener(SkipGame);
            _texture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            _texture.filterMode = FilterMode.Bilinear;
            if (_display != null)
                _display.texture = _texture;
            _trail = new Vector2[_trailLength];
            StartGame();
        }

        public override void StartGame()
        {
            base.StartGame();
            var config = GameDifficulty.Instance?.GetConfig("tracker");
            _totalPhases = config?.rounds ?? 4;
            _phaseDuration = config?.duration ?? 8f;
            _currentPhase = 0;
            _phaseTimer = 0f;
            _inPause = true;
            _patterns = new[] { Pattern.UpDown, Pattern.LeftRight, Pattern.Clockwise, Pattern.Figure8 };
            _winPanel?.SetActive(false);
            UpdateStatus();
        }

        protected override void OnUpdate()
        {
            if (!IsPlaying) return;

            _phaseTimer += Time.deltaTime;

            if (_inPause)
            {
                if (_phaseTimer >= _pauseDuration)
                {
                    _inPause = false;
                    _phaseTimer = 0f;
                }
                DrawBackground();
                return;
            }

            float t = _phaseTimer / _phaseDuration;
            _ballPos = GetPatternPosition(_patterns[_currentPhase % _patterns.Length], t);

            // Update trail
            for (int i = _trailLength - 1; i > 0; i--)
                _trail[i] = _trail[i - 1];
            _trail[0] = _ballPos;

            DrawFrame();

            if (_progressBar != null)
                _progressBar.value = ((_currentPhase * _phaseDuration + _phaseTimer) / (_totalPhases * _phaseDuration));

            if (_phaseTimer >= _phaseDuration)
            {
                _currentPhase++;
                if (_currentPhase >= _totalPhases)
                {
                    ShowWin();
                    return;
                }
                _inPause = true;
                _phaseTimer = 0f;
                UpdateStatus();
                HapticService.Instance?.VibrateLight();
            }
        }

        private Vector2 GetPatternPosition(Pattern pattern, float t)
        {
            float w = _texture.width;
            float h = _texture.height;
            Vector2 center = new Vector2(w * 0.5f, h * 0.5f);
            float radius = w * 0.35f;
            float angle = t * Mathf.PI * 2f;

            switch (pattern)
            {
                case Pattern.UpDown:
                    return center + new Vector2(0, Mathf.Sin(angle) * radius);
                case Pattern.LeftRight:
                    return center + new Vector2(Mathf.Sin(angle) * radius, 0);
                case Pattern.Clockwise:
                    return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                case Pattern.CounterClockwise:
                    return center + new Vector2(Mathf.Cos(-angle), Mathf.Sin(-angle)) * radius;
                case Pattern.Figure8:
                    return center + new Vector2(Mathf.Sin(angle * 2f) * radius, Mathf.Sin(angle) * radius * 0.6f);
                case Pattern.ZigZag:
                    float zx = Mathf.PingPong(t * 4f, 2f) - 1f;
                    float zy = t * 2f - 1f;
                    return center + new Vector2(zx * radius, zy * radius);
                case Pattern.Spiral:
                    float sr = t * radius;
                    return center + new Vector2(Mathf.Cos(angle * 3f), Mathf.Sin(angle * 3f)) * sr;
                case Pattern.Random:
                default:
                    return center + new Vector2(
                        Mathf.PerlinNoise(t * 3f, 0) * 2f - 1f,
                        Mathf.PerlinNoise(0, t * 3f) * 2f - 1f
                    ) * radius;
            }
        }

        private void DrawBackground()
        {
            int w = _texture.width;
            int h = _texture.height;
            Color[] pixels = new Color[w * h];
            Color bg = new Color(0.02f, 0.02f, 0.06f);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = bg;
            _texture.SetPixels(pixels);
            _texture.Apply(false);
        }

        private void DrawFrame()
        {
            int w = _texture.width;
            int h = _texture.height;
            DrawBackground();

            Color[] pixels = _texture.GetPixels();
            // Draw trail
            for (int i = _trailLength - 1; i >= 0; i--)
            {
                if (_trail[i] == Vector2.zero) continue;
                float alpha = 0.3f - i * 0.04f;
                DrawCircle(ref pixels, w, h, _trail[i], 16 - i * 2, new Color(0.02f, 0.71f, 0.83f, alpha));
            }
            // Draw ball
            DrawCircle(ref pixels, w, h, _ballPos, 20, new Color(0.65f, 0.95f, 0.99f, 1f));
            DrawCircle(ref pixels, w, h, _ballPos, 14, new Color(0.02f, 0.71f, 0.83f, 1f));
            DrawCircle(ref pixels, w, h, _ballPos, 6, new Color(0.49f, 0.23f, 0.93f, 1f));
            // Highlight
            DrawCircle(ref pixels, w, h, _ballPos + new Vector2(4, 4), 4, new Color(1, 1, 1, 0.6f));

            _texture.SetPixels(pixels);
            _texture.Apply(false);
        }

        private void DrawCircle(ref Color[] pixels, int w, int h, Vector2 center, float radius, Color color)
        {
            int cx = Mathf.RoundToInt(center.x);
            int cy = Mathf.RoundToInt(center.y);
            int r = Mathf.CeilToInt(radius);
            for (int y = -r; y <= r; y++)
            {
                for (int x = -r; x <= r; x++)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px < 0 || px >= w || py < 0 || py >= h) continue;
                    float d = Mathf.Sqrt(x * x + y * y) / radius;
                    if (d <= 1f)
                    {
                        float a = (1f - d) * color.a;
                        int idx = py * w + px;
                        pixels[idx] = Color.Lerp(pixels[idx], color, a);
                    }
                }
            }
        }

        private void UpdateStatus()
        {
            if (_statusText != null)
                _statusText.text = $"Фаза {_currentPhase + 1}/{_totalPhases}";
        }

        private void ShowWin()
        {
            Score = 100;
            _winPanel?.SetActive(true);
        }

        protected override void WinGame()
        {
            base.WinGame();
        }
    }
}
