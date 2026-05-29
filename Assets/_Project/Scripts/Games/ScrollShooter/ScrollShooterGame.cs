using System.Collections.Generic;
using Superglazka.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Superglazka.Games.ScrollShooter
{
    public class ScrollShooterGame : BaseMinigame
    {
        [Header("Scroll Shooter 3D")]
        [SerializeField] private Camera _gameCamera;
        [SerializeField] private Transform _playerBase;
        [SerializeField] private Transform _floor;
        [SerializeField] private Material _floorMaterial;
        [SerializeField] private Material _enemyMaterial;
        [SerializeField] private Material _bossMaterial;
        [SerializeField] private Material _playerMaterial;
        [SerializeField] private Material _bulletMaterial;
        [SerializeField] private GameObject _winPanel;
        [SerializeField] private GameObject _losePanel;
        [SerializeField] private Text _hudText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _skipButton;

        [Header("Prefabs")]
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private GameObject _bossPrefab;
        [SerializeField] private GameObject _panelPrefab;

        private List<GameObject> _bullets = new();
        private List<GameObject> _enemies = new();
        private List<GameObject> _panels = new();
        private GameObject _currentBoss;
        private float _playerX;
        private int _shooters = 1;
        private float _fireRate = 0.5f;
        private float _fireTimer;
        private int _baseHp = 100;
        private int _wave = 1;
        private float _waveTimer;
        private bool _bossSpawned;
        private float _scrollSpeed = 8f;
        private float _floorOffset;
        private float _bossSpawnTime = 25f;
        private int _bossHp;
        private int _bossMaxHp;

        private readonly float[] _lanes = { -2.5f, 0f, 2.5f };
        // Object pools can be initialized if needed for heavy spawning
        // private ObjectPool<GameObject> _bulletPool;
        // private ObjectPool<GameObject> _enemyPool;

        private void Start()
        {
            _continueButton?.onClick.AddListener(() => { WinGame(); ContinueToEpisode(); });
            _skipButton?.onClick.AddListener(SkipGame);
            StartGame();
        }

        public override void StartGame()
        {
            base.StartGame();
            var config = GameDifficulty.Instance?.GetConfig("scrollshooter");
            _wave = config?.wave ?? 1;
            _baseHp = config?.hp ?? 100;
            _bossHp = 200 + _wave * 50;
            _bossMaxHp = _bossHp;
            _shooters = 1;
            _fireRate = 0.5f;
            _bossSpawned = false;
            _waveTimer = 0f;
            _fireTimer = 0f;
            _scrollSpeed = 8f + _wave;
            Score = 0;

            ClearEntities();
            SpawnPanels();
            UpdateFloorMaterial();
            _winPanel?.SetActive(false);
            _losePanel?.SetActive(false);
        }

        private void ClearEntities()
        {
            foreach (var b in _bullets) if (b != null) Destroy(b);
            foreach (var e in _enemies) if (e != null) Destroy(e);
            foreach (var p in _panels) if (p != null) Destroy(p);
            if (_currentBoss != null) Destroy(_currentBoss);
            _bullets.Clear();
            _enemies.Clear();
            _panels.Clear();
        }

        private void SpawnPanels()
        {
            // Spawn shooter + firerate panels ahead
            for (int i = 0; i < 3; i++)
            {
                var panel = Instantiate(_panelPrefab, new Vector3(_lanes[Random.Range(0, 3)], 0.5f, 30f + i * 20f), Quaternion.identity);
                panel.GetComponent<Renderer>().material = _bulletMaterial;
                _panels.Add(panel);
            }
        }

        protected override void OnUpdate()
        {
            if (!IsPlaying) return;
            _waveTimer += Time.deltaTime;

            // Input
            if (GameInput.Instance != null && GameInput.Instance.TouchHeld)
            {
                Ray ray = _gameCamera.ScreenPointToRay(GameInput.Instance.TouchPosition);
                if (Physics.Raycast(ray, out var hit, 100f, LayerMask.GetMask("Floor")))
                {
                    _playerX = Mathf.Clamp(hit.point.x, -3.2f, 3.2f);
                }
            }
            _playerBase.position = Vector3.Lerp(_playerBase.position, new Vector3(_playerX, 0, 0), Time.deltaTime * 15f);

            // Floor scrolling
            _floorOffset += _scrollSpeed * Time.deltaTime;
            if (_floorMaterial != null)
                _floorMaterial.SetFloat("_ScrollY", _floorOffset);

            // Shooting
            _fireTimer -= Time.deltaTime;
            if (_fireTimer <= 0f)
            {
                FireBullets();
                _fireTimer = _fireRate;
            }

            // Spawn enemies
            if (!_bossSpawned && _waveTimer >= _bossSpawnTime)
            {
                SpawnBoss();
            }
            else if (!_bossSpawned && Random.value < 0.02f + _wave * 0.005f)
            {
                SpawnEnemy();
            }

            UpdateEntities();
            CheckCollisions();
            UpdateHUD();

            if (_baseHp <= 0)
            {
                Score = _wave * 50;
                LoseGame();
                return;
            }
        }

        private void FireBullets()
        {
            float spread = 0.3f;
            for (int i = 0; i < _shooters; i++)
            {
                float xOffset = (i - (_shooters - 1) * 0.5f) * spread;
                var bullet = Instantiate(_bulletPrefab,
                    _playerBase.position + new Vector3(xOffset, 0.5f, 1f),
                    Quaternion.identity);
                bullet.GetComponent<Renderer>().material = _bulletMaterial;
                _bullets.Add(bullet);
            }
        }

        private void SpawnEnemy()
        {
            float lane = _lanes[Random.Range(0, 3)];
            var enemy = Instantiate(_enemyPrefab, new Vector3(lane, 0.5f, 40f), Quaternion.identity);
            enemy.GetComponent<Renderer>().material = _enemyMaterial;
            var data = enemy.AddComponent<EnemyData>();
            data.hp = 20 + _wave * 5;
            data.speed = _scrollSpeed;
            _enemies.Add(enemy);
        }

        private void SpawnBoss()
        {
            _bossSpawned = true;
            float lane = _lanes[1];
            _currentBoss = Instantiate(_bossPrefab, new Vector3(lane, 2f, 35f), Quaternion.identity);
            _currentBoss.transform.localScale = Vector3.one * 3f;
            _currentBoss.GetComponent<Renderer>().material = _bossMaterial;
            var data = _currentBoss.AddComponent<EnemyData>();
            data.hp = _bossHp;
            data.speed = _scrollSpeed * 0.3f;
            data.isBoss = true;
        }

        private void UpdateEntities()
        {
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                var b = _bullets[i];
                if (b == null) { _bullets.RemoveAt(i); continue; }
                b.transform.position += Vector3.forward * 20f * Time.deltaTime;
                if (b.transform.position.z > 50f)
                {
                    Destroy(b);
                    _bullets.RemoveAt(i);
                }
            }

            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var e = _enemies[i];
                if (e == null) { _enemies.RemoveAt(i); continue; }
                var data = e.GetComponent<EnemyData>();
                e.transform.position -= Vector3.forward * data.speed * Time.deltaTime;
                if (e.transform.position.z < -5f)
                {
                    _baseHp -= 10;
                    Destroy(e);
                    _enemies.RemoveAt(i);
                    HapticService.Instance?.VibratePattern(new long[] { 30, 30 });
                }
            }

            if (_currentBoss != null)
            {
                var data = _currentBoss.GetComponent<EnemyData>();
                _currentBoss.transform.position -= Vector3.forward * data.speed * Time.deltaTime;
                _currentBoss.transform.position = new Vector3(
                    Mathf.Sin(Time.time) * 2f,
                    _currentBoss.transform.position.y,
                    _currentBoss.transform.position.z
                );
            }

            for (int i = _panels.Count - 1; i >= 0; i--)
            {
                var p = _panels[i];
                if (p == null) { _panels.RemoveAt(i); continue; }
                p.transform.position -= Vector3.forward * _scrollSpeed * Time.deltaTime;
                if (p.transform.position.z < -5f)
                {
                    Destroy(p);
                    _panels.RemoveAt(i);
                }
            }
        }

        private void CheckCollisions()
        {
            for (int b = _bullets.Count - 1; b >= 0; b--)
            {
                var bullet = _bullets[b];
                if (bullet == null) continue;
                Vector3 bp = bullet.transform.position;
                bool hit = false;

                for (int e = _enemies.Count - 1; e >= 0; e--)
                {
                    var enemy = _enemies[e];
                    if (enemy == null) continue;
                    if (Vector3.Distance(bp, enemy.transform.position) < 1.5f)
                    {
                        var data = enemy.GetComponent<EnemyData>();
                        data.hp -= 10;
                        hit = true;
                        if (data.hp <= 0)
                        {
                            Destroy(enemy);
                            _enemies.RemoveAt(e);
                            HapticService.Instance?.VibrateSuccess();
                        }
                        break;
                    }
                }

                if (!hit && _currentBoss != null)
                {
                    if (Vector3.Distance(bp, _currentBoss.transform.position) < 3f)
                    {
                        var data = _currentBoss.GetComponent<EnemyData>();
                        data.hp -= 10;
                        _bossHp = data.hp;
                        hit = true;
                        if (data.hp <= 0)
                        {
                            Destroy(_currentBoss);
                            _currentBoss = null;
                            _wave++;
                            Score = _wave * 100 + _shooters * 10;
                            WinGame();
                            return;
                        }
                    }
                }

                for (int p = _panels.Count - 1; p >= 0; p--)
                {
                    var panel = _panels[p];
                    if (panel == null) continue;
                    if (Vector3.Distance(bp, panel.transform.position) < 1.5f)
                    {
                        var type = panel.name.Contains("rate") ? "firerate" : "shooter";
                        if (type == "firerate") _fireRate = Mathf.Max(0.1f, _fireRate - 0.05f);
                        else _shooters = Mathf.Min(5, _shooters + 1);
                        Destroy(panel);
                        _panels.RemoveAt(p);
                        hit = true;
                        HapticService.Instance?.VibrateSuccess();
                        break;
                    }
                }

                if (hit)
                {
                    Destroy(bullet);
                    _bullets.RemoveAt(b);
                }
            }
        }

        private void UpdateHUD()
        {
            if (_hudText != null)
            {
                _hudText.text = $"Wave: {_wave} HP: {_baseHp}\nShoot: {_shooters} | Rate: {_fireRate:F2}";
            }
        }

        private void UpdateFloorMaterial()
        {
            if (_floorMaterial != null)
            {
                _floorMaterial.EnableKeyword("_EMISSION");
                _floorMaterial.SetColor("_EmissionColor", new Color(0.2f, 0f, 0.4f));
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

        private class EnemyData : MonoBehaviour
        {
            public int hp;
            public float speed;
            public bool isBoss;
        }
    }
}
