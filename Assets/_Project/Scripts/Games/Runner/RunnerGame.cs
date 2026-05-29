using System.Collections.Generic;
using Superglazka.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Superglazka.Games.Runner
{
    public class RunnerGame : BaseMinigame
    {
        [Header("Runner")]
        [SerializeField] private RawImage _display;
        [SerializeField] private Text _statsText;
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private GameObject _losePanel;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private Slider _progressSlider;

        private Texture2D _texture;
        private float _playerY;
        private float _playerVelY;
        private bool _isGrounded;
        private bool _canDoubleJump;
        private float _playerX;
        private float _distance;
        private float _targetDistance;
        private float _speed;
        private float _speedCap;
        private int _lives;
        private bool _shield;
        private float _shieldTimer;
        private float _slowTimer;
        private float _gameTimer;
        private int _stars;
        private int _bonusHearts;
        private int _bonusStars;
        private int _bonusShields;

        private List<Obstacle> _obstacles = new();
        private List<Bonus> _bonuses = new();
        private List<Particle> _particles = new();
        private float _obstacleTimer;
        private float _bgOffset;

        private class Obstacle
        {
            public float x;
            public float y;
            public float width;
            public float height;
            public string type;
            public bool hit;
        }

        private class Bonus
        {
            public float x;
            public float y;
            public string type;
            public bool collected;
        }

        private class Particle
        {
            public Vector2 pos;
            public Vector2 vel;
            public float life;
            public Color color;
        }

        private void Start()
        {
            _continueButton?.onClick.AddListener(() => { WinGame(); ContinueToEpisode(); });
            _skipButton?.onClick.AddListener(SkipGame);
            _texture = new Texture2D(512, 256, TextureFormat.RGBA32, false);
            _texture.filterMode = FilterMode.Bilinear;
            if (_display != null)
                _display.texture = _texture;
            StartGame();
        }

        public override void StartGame()
        {
            base.StartGame();
            var config = GameDifficulty.Instance?.GetConfig("runner");
            _targetDistance = config?.targetDist ?? 2000f;
            _speed = config?.speed ?? 5f;
            _speedCap = config?.speedCap ?? 12f;
            _lives = 3;
            _shield = false;
            _shieldTimer = 0f;
            _slowTimer = 0f;
            _distance = 0f;
            _playerY = 0f;
            _playerVelY = 0f;
            _isGrounded = true;
            _canDoubleJump = true;
            _gameTimer = 0f;
            _stars = 3;
            _bonusHearts = 0;
            _bonusStars = 0;
            _bonusShields = 0;
            _obstacles.Clear();
            _bonuses.Clear();
            _particles.Clear();
            _obstacleTimer = 1.5f;
            _winPanel?.SetActive(false);
            _losePanel?.SetActive(false);
        }

        protected override void OnUpdate()
        {
            if (!IsPlaying) return;
            _gameTimer += Time.deltaTime;
            float dt = Time.deltaTime;
            float currentSpeed = _slowTimer > 0f ? _speed * 0.5f : _speed;
            _slowTimer -= dt;
            _shieldTimer -= dt;
            if (_shieldTimer <= 0f) _shield = false;

            _distance += currentSpeed * dt * 60f;
            _bgOffset += currentSpeed * dt * 10f;
            _speed = Mathf.Min(_speed + dt * 0.05f, _speedCap);

            // Player physics
            _playerVelY -= 30f * dt;
            _playerY += _playerVelY * dt;
            if (_playerY <= 0f)
            {
                _playerY = 0f;
                _playerVelY = 0f;
                _isGrounded = true;
                _canDoubleJump = true;
            }
            else _isGrounded = false;

            // Spawn obstacles
            _obstacleTimer -= dt;
            if (_obstacleTimer <= 0f)
            {
                SpawnObstacleOrBonus();
                _obstacleTimer = Random.Range(1f, 2.5f) * (1f / (_speed / 5f));
            }

            UpdateEntities(dt);
            UpdateParticles(dt);
            CheckCollisions();
            DrawFrame();

            if (_progressSlider != null)
                _progressSlider.value = _distance / _targetDistance;

            if (_statsText != null)
                _statsText.text = $"♥{_lives}  ★{_stars}/3";

            if (_distance >= _targetDistance)
            {
                CalculateStars();
                WinGame();
                return;
            }

            if (_lives <= 0)
            {
                CalculateStars();
                LoseGame();
                return;
            }
        }

        public void Jump()
        {
            if (!IsPlaying) return;
            if (_isGrounded)
            {
                _playerVelY = 12f;
                _isGrounded = false;
                SpawnParticles(new Vector2(_playerX, _playerY), new Color(1f, 0.5f, 0f), 5);
            }
            else if (_canDoubleJump)
            {
                _playerVelY = 9f;
                _canDoubleJump = false;
                SpawnParticles(new Vector2(_playerX, _playerY), new Color(1f, 0.5f, 0f), 8);
            }
        }

        private void SpawnObstacleOrBonus()
        {
            float roll = Random.value;
            if (roll < 0.75f)
            {
                string[] types = { "can", "cup", "vase", "sugar" };
                _obstacles.Add(new Obstacle
                {
                    x = 520f,
                    y = 0f,
                    width = 24f,
                    height = 30f + Random.Range(0f, 20f),
                    type = types[Random.Range(0, types.Length)],
                    hit = false
                });
            }
            else
            {
                string[] types = { "heart", "star", "shield" };
                _bonuses.Add(new Bonus
                {
                    x = 520f,
                    y = Random.Range(40f, 120f),
                    type = types[Random.Range(0, types.Length)],
                    collected = false
                });
            }
        }

        private void UpdateEntities(float dt)
        {
            float moveSpeed = _slowTimer > 0f ? _speed * 0.5f : _speed;
            for (int i = _obstacles.Count - 1; i >= 0; i--)
            {
                var o = _obstacles[i];
                o.x -= moveSpeed * dt * 60f;
                if (o.x < -50f) _obstacles.RemoveAt(i);
            }
            for (int i = _bonuses.Count - 1; i >= 0; i--)
            {
                var b = _bonuses[i];
                b.x -= moveSpeed * dt * 60f;
                if (b.x < -50f) _bonuses.RemoveAt(i);
            }
        }

        private void UpdateParticles(float dt)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.pos += p.vel * dt;
                p.vel.y -= 20f * dt;
                p.life -= dt;
                if (p.life <= 0f) _particles.RemoveAt(i);
            }
        }

        private void CheckCollisions()
        {
            float px = _playerX;
            float py = _playerY;
            float pw = 24f;
            float ph = 36f;

            foreach (var o in _obstacles)
            {
                if (o.hit) continue;
                if (RectOverlap(px, py, pw, ph, o.x, o.y, o.width, o.height))
                {
                    o.hit = true;
                    if (_shield)
                    {
                        _shield = false;
                        SpawnParticles(new Vector2(px, py), Color.cyan, 10);
                    }
                    else
                    {
                        _lives--;
                        _stars = Mathf.Max(1, _stars - 1);
                        SpawnParticles(new Vector2(px, py), Color.red, 12);
                        HapticService.Instance?.VibratePattern(new long[] { 50, 30 });
                    }
                }
            }

            foreach (var b in _bonuses)
            {
                if (b.collected) continue;
                if (RectOverlap(px, py, pw, ph, b.x, b.y, 20f, 20f))
                {
                    b.collected = true;
                    switch (b.type)
                    {
                        case "heart":
                            _lives = Mathf.Min(3, _lives + 1);
                            _bonusHearts++;
                            break;
                        case "star":
                            _slowTimer = 3f;
                            _bonusStars++;
                            break;
                        case "shield":
                            _shield = true;
                            _shieldTimer = 5f;
                            _bonusShields++;
                            break;
                    }
                    SpawnParticles(new Vector2(b.x, b.y), Color.yellow, 8);
                    HapticService.Instance?.VibrateSuccess();
                }
            }
        }

        private bool RectOverlap(float x1, float y1, float w1, float h1, float x2, float y2, float w2, float h2)
        {
            return x1 < x2 + w2 && x1 + w1 > x2 && y1 < y2 + h2 && y1 + h1 > y2;
        }

        private void CalculateStars()
        {
            if (_lives >= 3 && _bonusHearts > 0 && _bonusShields > 0) _stars = 3;
            else if (_lives >= 2) _stars = 2;
            else _stars = 1;
            Score = _stars * 100;
        }

        private void DrawFrame()
        {
            int w = _texture.width;
            int h = _texture.height;
            Color[] pixels = new Color[w * h];
            // Sky
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0.1f, 0.15f, 0.25f);
            // Parallax ground lines
            for (int x = 0; x < w; x++)
            {
                float lineX = (x + _bgOffset * 0.5f) % 40f;
                if (lineX < 2f)
                {
                    for (int y = 0; y < 20; y++)
                        pixels[y * w + x] = new Color(0.3f, 0.3f, 0.35f);
                }
            }
            // Ground
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < 8; y++)
                    pixels[y * w + x] = new Color(0.4f, 0.3f, 0.2f);
            }

            // Obstacles
            foreach (var o in _obstacles)
            {
                Color c = o.type switch
                {
                    "can" => new Color(0.6f, 0.6f, 0.65f),
                    "cup" => new Color(0.8f, 0.4f, 0.2f),
                    "vase" => new Color(0.3f, 0.6f, 0.5f),
                    "sugar" => new Color(0.9f, 0.9f, 0.85f),
                    _ => Color.gray,
                };
                FillRect(ref pixels, w, h, (int)o.x, (int)o.y, (int)o.width, (int)o.height, c);
            }

            // Bonuses
            foreach (var b in _bonuses)
            {
                if (b.collected) continue;
                Color c = b.type switch
                {
                    "heart" => Color.red,
                    "star" => Color.yellow,
                    "shield" => Color.cyan,
                    _ => Color.white,
                };
                FillRect(ref pixels, w, h, (int)(b.x - 10), (int)(b.y - 10), 20, 20, c);
            }

            // Player
            int pxi = Mathf.RoundToInt(_playerX);
            int pyi = Mathf.RoundToInt(_playerY);
            FillRect(ref pixels, w, h, pxi - 12, pyi, 24, 36, new Color(0.9f, 0.9f, 0.95f));
            if (_shield)
            {
                DrawCircle(ref pixels, w, h, new Vector2(pxi, pyi + 18), 28f, new Color(0f, 0.8f, 1f, 0.3f + Mathf.Sin(Time.time * 6f) * 0.1f));
            }

            // Particles
            foreach (var p in _particles)
            {
                int px = Mathf.RoundToInt(p.pos.x);
                int py = Mathf.RoundToInt(p.pos.y);
                if (px >= 0 && px < w && py >= 0 && py < h)
                    pixels[py * w + px] = p.color * p.life;
            }

            // Speed lines
            if (_slowTimer <= 0f)
            {
                for (int i = 0; i < 5; i++)
                {
                    int lx = Mathf.RoundToInt(Random.Range(0, w));
                    int ly = Mathf.RoundToInt(Random.Range(30, h - 10));
                    for (int xx = 0; xx < 8; xx++)
                        if (lx - xx >= 0)
                            pixels[ly * w + (lx - xx)] = new Color(1, 1, 1, 0.1f);
                }
            }

            _texture.SetPixels(pixels);
            _texture.Apply(false);
        }

        private void FillRect(ref Color[] pixels, int w, int h, int x, int y, int width, int height, Color color)
        {
            for (int yy = y; yy < y + height && yy < h; yy++)
            {
                if (yy < 0) continue;
                for (int xx = x; xx < x + width && xx < w; xx++)
                {
                    if (xx < 0) continue;
                    pixels[yy * w + xx] = color;
                }
            }
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

        private void SpawnParticles(Vector2 pos, Color color, int count)
        {
            for (int i = 0; i < count; i++)
            {
                _particles.Add(new Particle
                {
                    pos = pos,
                    vel = new Vector2(Random.Range(-40f, 40f), Random.Range(20f, 80f)),
                    life = 0.4f,
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
