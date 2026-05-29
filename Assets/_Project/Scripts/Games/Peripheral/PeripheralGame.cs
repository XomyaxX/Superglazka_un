using System.Collections.Generic;
using Superglazka.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Superglazka.Games.Peripheral
{
    public class PeripheralGame : BaseMinigame
    {
        [Header("Peripheral")]
        [SerializeField] private RawImage _display;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Text _timerText;
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private GameObject _losePanel;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _skipButton;

        private Texture2D _texture;
        private List<Target> _targets = new();
        private List<Particle> _particles = new();
        private List<FloatingText> _floatingTexts = new();
        private float _spawnTimer;
        private float _gameTimer;
        private int _lives;
        private int _combo;
        private float _comboMultiplier;
        private float _baseWidth;
        private float _baseHeight;
        private Vector2 _centerZone;
        private float _deadZoneRadius;

        private enum TargetType { Normal, Golden, Fading, Moving, Trap }

        private class Target
        {
            public Vector2 pos;
            public TargetType type;
            public float lifetime;
            public float maxLifetime;
            public float radius;
            public Vector2 velocity;
            public bool active;
        }

        private class Particle
        {
            public Vector2 pos;
            public Vector2 velocity;
            public float lifetime;
            public Color color;
        }

        private class FloatingText
        {
            public Vector2 pos;
            public string text;
            public float lifetime;
            public Color color;
        }

        private void Start()
        {
            _continueButton?.onClick.AddListener(() => { WinGame(); ContinueToEpisode(); });
            _skipButton?.onClick.AddListener(SkipGame);
            _texture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            _texture.filterMode = FilterMode.Bilinear;
            if (_display != null)
                _display.texture = _texture;
            StartGame();
        }

        public override void StartGame()
        {
            base.StartGame();
            var config = GameDifficulty.Instance?.GetConfig("peripheral");
            _gameTimer = config?.duration ?? 60f;
            _lives = config?.lives ?? 3;
            _targets.Clear();
            _particles.Clear();
            _floatingTexts.Clear();
            _spawnTimer = 0f;
            _combo = 0;
            _comboMultiplier = 1f;
            Score = 0;
            _baseWidth = _texture.width;
            _baseHeight = _texture.height;
            _centerZone = new Vector2(_baseWidth * 0.5f, _baseHeight * 0.5f);
            _deadZoneRadius = Mathf.Min(_baseWidth, _baseHeight) * 0.18f;
            _winPanel?.SetActive(false);
            _losePanel?.SetActive(false);
        }

        protected override void OnUpdate()
        {
            if (!IsPlaying) return;
            _gameTimer -= Time.deltaTime;
            if (_gameTimer <= 0f)
            {
                WinGame();
                return;
            }

            _spawnTimer -= Time.deltaTime;
            var config = GameDifficulty.Instance?.GetConfig("peripheral");
            float spawnRate = config?.spawnRate ?? 1.5f;
            int maxTargets = config?.maxTargets ?? 3;

            if (_spawnTimer <= 0f && _targets.Count < maxTargets)
            {
                SpawnTarget();
                _spawnTimer = spawnRate * Random.Range(0.7f, 1.3f);
            }

            UpdateTargets();
            UpdateParticles();
            UpdateFloatingTexts();
            DrawFrame();

            if (_scoreText != null)
                _scoreText.text = $"Score: {Score}";
            if (_timerText != null)
                _timerText.text = $"Time: {Mathf.CeilToInt(_gameTimer)}";
        }

        private void SpawnTarget()
        {
            var t = new Target();
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(_deadZoneRadius + 20f, Mathf.Min(_baseWidth, _baseHeight) * 0.45f);
            t.pos = _centerZone + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
            t.type = (TargetType)Random.Range(0, System.Enum.GetValues(typeof(TargetType)).Length);
            t.maxLifetime = t.type switch
            {
                TargetType.Golden => 1.5f,
                TargetType.Fading => 2.5f,
                _ => 3f,
            };
            t.lifetime = t.maxLifetime;
            t.radius = 18f;
            t.velocity = t.type == TargetType.Moving ? Random.insideUnitCircle.normalized * 40f : Vector2.zero;
            t.active = true;
            _targets.Add(t);
        }

        private void UpdateTargets()
        {
            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                var t = _targets[i];
                t.lifetime -= Time.deltaTime;
                if (t.type == TargetType.Moving)
                {
                    t.pos += t.velocity * Time.deltaTime;
                    if (t.pos.x < 20 || t.pos.x > _baseWidth - 20) t.velocity.x *= -1f;
                    if (t.pos.y < 20 || t.pos.y > _baseHeight - 20) t.velocity.y *= -1f;
                }
                if (t.lifetime <= 0f)
                {
                    if (t.type != TargetType.Trap)
                        _combo = Mathf.Max(0, _combo - 1);
                    _targets.RemoveAt(i);
                }
            }
        }

        private void UpdateParticles()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.pos += p.velocity * Time.deltaTime;
                p.lifetime -= Time.deltaTime;
                if (p.lifetime <= 0f) _particles.RemoveAt(i);
            }
        }

        private void UpdateFloatingTexts()
        {
            for (int i = _floatingTexts.Count - 1; i >= 0; i--)
            {
                var ft = _floatingTexts[i];
                ft.pos.y += 30f * Time.deltaTime;
                ft.lifetime -= Time.deltaTime;
                if (ft.lifetime <= 0f) _floatingTexts.RemoveAt(i);
            }
        }

        private void DrawFrame()
        {
            int w = _texture.width;
            int h = _texture.height;
            Color[] pixels = new Color[w * h];
            // Vignette background
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - w * 0.5f) / (w * 0.5f);
                    float dy = (y - h * 0.5f) / (h * 0.5f);
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float v = Mathf.Clamp01(1f - dist * 0.8f);
                    pixels[y * w + x] = new Color(0.02f, 0.02f, 0.06f) * v;
                }
            }
            // Center fixation dot
            DrawCircle(ref pixels, w, h, _centerZone, 6f, Color.white);
            // Dead zone ring
            DrawRing(ref pixels, w, h, _centerZone, _deadZoneRadius, 1f, new Color(1, 1, 1, 0.1f));

            foreach (var t in _targets)
            {
                Color color = t.type switch
                {
                    TargetType.Normal => new Color(0.4f, 0.8f, 1f, 1f),
                    TargetType.Golden => new Color(1f, 0.84f, 0f, 1f),
                    TargetType.Fading => new Color(0.8f, 0.2f, 0.8f, Mathf.PingPong(t.lifetime * 3f, 1f)),
                    TargetType.Moving => new Color(0.2f, 1f, 0.4f, 1f),
                    TargetType.Trap => new Color(1f, 0.2f, 0.2f, 1f),
                    _ => Color.white,
                };
                DrawCircle(ref pixels, w, h, t.pos, t.radius, color);
                float lifeRatio = t.lifetime / t.maxLifetime;
                DrawRing(ref pixels, w, h, t.pos, t.radius + 4f, 2f, Color.Lerp(Color.red, Color.green, lifeRatio) * 0.5f);
            }

            foreach (var p in _particles)
            {
                DrawCircle(ref pixels, w, h, p.pos, 3f, p.color * p.lifetime);
            }

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

        private void DrawRing(ref Color[] pixels, int w, int h, Vector2 center, float radius, float thickness, Color color)
        {
            int cx = Mathf.RoundToInt(center.x);
            int cy = Mathf.RoundToInt(center.y);
            int r = Mathf.CeilToInt(radius + thickness);
            for (int y = -r; y <= r; y++)
            {
                for (int x = -r; x <= r; x++)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px < 0 || px >= w || py < 0 || py >= h) continue;
                    float d = Mathf.Abs(Mathf.Sqrt(x * x + y * y) - radius);
                    if (d <= thickness)
                    {
                        float a = (1f - d / thickness) * color.a;
                        int idx = py * w + px;
                        pixels[idx] = Color.Lerp(pixels[idx], color, a);
                    }
                }
            }
        }

        public void OnTap(Vector2 screenPos)
        {
            if (!IsPlaying) return;

            Vector2 localPos = screenPos;
            if (_display != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _display.rectTransform, screenPos, null, out localPos);
                Rect r = _display.rectTransform.rect;
                localPos.x = (localPos.x - r.x) / r.width * _baseWidth;
                localPos.y = (localPos.y - r.y) / r.height * _baseHeight;
            }

            float distToCenter = Vector2.Distance(localPos, _centerZone);
            if (distToCenter < _deadZoneRadius)
            {
                // Dead zone tap
                return;
            }

            bool hit = false;
            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                var t = _targets[i];
                if (!t.active) continue;
                if (Vector2.Distance(localPos, t.pos) < t.radius + 10f)
                {
                    hit = true;
                    ProcessHit(t, i, localPos);
                    break;
                }
            }

            if (!hit)
            {
                _combo = Mathf.Max(0, _combo - 1);
            }
        }

        private void ProcessHit(Target t, int index, Vector2 pos)
        {
            _targets.RemoveAt(index);
            int basePoints = t.type switch
            {
                TargetType.Normal => 10,
                TargetType.Golden => 50,
                TargetType.Fading => 30,
                TargetType.Moving => 25,
                TargetType.Trap => 0,
                _ => 10,
            };

            if (t.type == TargetType.Trap)
            {
                _lives--;
                _combo = 0;
                HapticService.Instance?.VibratePattern(new long[] { 30, 50, 30 });
                if (_lives <= 0)
                {
                    LoseGame();
                    return;
                }
            }
            else
            {
                _combo++;
                _comboMultiplier = 1f + Mathf.Min(_combo, 15) * 0.1f;
                float speedBonus = t.lifetime / t.maxLifetime * 5f;
                int points = Mathf.RoundToInt((basePoints + speedBonus) * _comboMultiplier);
                Score += points;
                HapticService.Instance?.VibrateSuccess();
                SpawnParticles(pos, t.type == TargetType.Golden ? Color.yellow : new Color(0.4f, 0.8f, 1f));
            }
        }

        private void SpawnParticles(Vector2 pos, Color color)
        {
            for (int i = 0; i < 8; i++)
            {
                _particles.Add(new Particle
                {
                    pos = pos,
                    velocity = Random.insideUnitCircle * 60f,
                    lifetime = 0.5f,
                    color = color
                });
            }
        }

        protected override void WinGame()
        {
            _winPanel?.SetActive(true);
            base.WinGame();
        }

        protected override void LoseGame()
        {
            _losePanel?.SetActive(true);
            base.LoseGame();
        }
    }
}
