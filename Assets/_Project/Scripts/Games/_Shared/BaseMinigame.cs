using Superglazka.Services;
using UnityEngine;

namespace Superglazka.Games
{
    public abstract class BaseMinigame : MonoBehaviour
    {
        [Header("Minigame")]
        [SerializeField] protected string _gameId;
        [SerializeField] protected int _coinReward = 100;
        [SerializeField] protected AudioClip _winSFX;
        [SerializeField] protected AudioClip _loseSFX;

        protected bool IsPlaying { get; private set; }
        protected int Score { get; set; }
        protected bool IsPaused { get; private set; }

        public virtual void StartGame()
        {
            IsPlaying = true;
            IsPaused = false;
            Score = 0;
            Time.timeScale = 1f;
        }

        public virtual void PauseGame()
        {
            IsPaused = true;
            Time.timeScale = 0f;
        }

        public virtual void ResumeGame()
        {
            IsPaused = false;
            Time.timeScale = 1f;
        }

        protected virtual void WinGame()
        {
            if (!IsPlaying) return;
            IsPlaying = false;
            PlayerProfile.Instance?.CompleteGame(_gameId, Score);
            PlayerProfile.Instance?.AddCoins(_coinReward);
            LeaderboardManager.Instance?.RecordScore(_gameId, Score);
            AchievementManager.Instance?.Check(AchievementType.Game, _gameId);
            GameDifficulty.Instance?.IncreaseLevel(_gameId);
            AudioManager.Instance?.PlaySFX(_winSFX);
            HapticService.Instance?.VibrateSuccess();
        }

        protected virtual void LoseGame()
        {
            if (!IsPlaying) return;
            IsPlaying = false;
            AudioManager.Instance?.PlaySFX(_loseSFX);
            HapticService.Instance?.VibratePattern(new long[] { 30, 50, 30 });
        }

        protected virtual void SkipGame()
        {
            IsPlaying = false;
            Core.GameManager.Instance?.ReturnFromMinigame();
        }

        protected virtual void ContinueToEpisode()
        {
            Core.GameManager.Instance?.ReturnFromMinigame();
        }

        protected virtual void Update()
        {
            if (!IsPlaying || IsPaused) return;
            OnUpdate();
        }

        protected virtual void FixedUpdate()
        {
            if (!IsPlaying || IsPaused) return;
            OnFixedUpdate();
        }

        protected abstract void OnUpdate();
        protected virtual void OnFixedUpdate() { }
    }
}
