using Superglazka.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Superglazka.Core
{
    public enum GameState
    {
        MainMenu, EpisodeSelect, EpisodeViewer, Minigame, Settings, Profile
    }

    public class GameManager : MonoSingleton<GameManager>
    {
        public GameState CurrentState { get; private set; }
        public event System.Action<GameState, GameState> OnStateChanged;

        [SerializeField] private string _mainMenuScene = "MainMenu";
        [SerializeField] private string _episodeScene = "EpisodeViewer";
        [SerializeField] private string _gameplayScenePrefix = "Game_";

        protected override void OnAwake()
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate > 60 ? 60 : Screen.currentResolution.refreshRate;
            QualitySettings.vSyncCount = 0;
            Input.multiTouchEnabled = true;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        public void GoToMainMenu()
        {
            SetState(GameState.MainMenu);
            SceneManager.LoadScene(_mainMenuScene);
        }

        public void GoToEpisode(string episodeId)
        {
            SetState(GameState.EpisodeViewer);
            PlayerPrefs.SetString("current_episode", episodeId);
            PlayerPrefs.Save();
            SceneManager.LoadScene(_episodeScene);
        }

        public void StartMinigame(string gameId)
        {
            SetState(GameState.Minigame);
            SceneManager.LoadScene(_gameplayScenePrefix + gameId);
        }

        public void ReturnFromMinigame()
        {
            SetState(GameState.EpisodeViewer);
            SceneManager.LoadScene(_episodeScene);
        }

        private void SetState(GameState newState)
        {
            var old = CurrentState;
            CurrentState = newState;
            OnStateChanged?.Invoke(old, newState);
        }
    }
}
