using Superglazka.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Superglazka.Core
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private string _nextScene = "MainMenu";

        private void Awake()
        {
            // Ensure all core services exist
            EnsureService<SaveSystem>();
            EnsureService<AudioManager>();
            EnsureService<HapticService>();
            EnsureService<LocalizationService>();
            EnsureService<GameInput>();
            EnsureService<PlayerProfile>();
            EnsureService<AchievementManager>();
            EnsureService<LeaderboardManager>();
            EnsureService<DailyRewardManager>();
            EnsureService<GameDifficulty>();
            EnsureService<TutorialManager>();
            EnsureService<GameManager>();
            EnsureService<MobileOptimizer>();

            // Load saved settings
            var settings = Resources.Load<Data.GameSettings>("GameSettings");
            settings?.Load();

            SceneManager.LoadScene(_nextScene);
        }

        private void EnsureService<T>() where T : MonoBehaviour
        {
            if (FindFirstObjectByType<T>() == null)
            {
                var go = new GameObject(typeof(T).Name);
                go.AddComponent<T>();
                DontDestroyOnLoad(go);
            }
        }
    }
}
