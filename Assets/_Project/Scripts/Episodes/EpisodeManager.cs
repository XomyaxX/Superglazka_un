using System;
using System.Collections;
using System.Collections.Generic;
using Superglazka.Data;
using Superglazka.Services;
using UnityEngine;

namespace Superglazka.Episodes
{
    public class EpisodeManager : MonoBehaviour
    {
        public static EpisodeManager Instance { get; private set; }

        [SerializeField] private EpisodeDatabase _database;
        [SerializeField] private EpisodeUI _episodeUI;

        public EpisodeData CurrentEpisode { get; private set; }
        public FrameData CurrentFrame { get; private set; }
        public int CurrentFrameIndex { get; private set; }
        public bool IsFrameComplete { get; private set; }

        public event Action<FrameData> OnFrameShown;
        public event Action OnFrameComplete;
        public event Action OnEpisodeEnded;
        public event Action<string> OnGameRequired;

        private bool _audioEnded;
        private bool _textEnded;
        private bool _gamePending;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            string episodeId = PlayerPrefs.GetString("current_episode", "");
            if (string.IsNullOrEmpty(episodeId))
            {
                Debug.LogError("No episode ID set!");
                return;
            }
            LoadEpisode(episodeId);
        }

        public void LoadEpisode(string episodeId)
        {
            CurrentEpisode = _database.GetEpisode(episodeId);
            if (CurrentEpisode == null)
            {
                Debug.LogError($"Episode {episodeId} not found in database.");
                return;
            }
            var progress = PlayerProfile.Instance?.GetProgress(episodeId);
            CurrentFrameIndex = progress?.maxFrame ?? 0;
            ShowFrame(CurrentFrameIndex);
        }

        public void ShowFrame(int index)
        {
            if (CurrentEpisode == null || index < 0 || index >= CurrentEpisode.frames.Count) return;
            CurrentFrameIndex = index;
            CurrentFrame = CurrentEpisode.frames[index];
            IsFrameComplete = false;
            _audioEnded = CurrentFrame.narrationAudio == null;
            _textEnded = false;
            _gamePending = !string.IsNullOrEmpty(CurrentFrame.gameId);

            AudioManager.Instance?.StopNarration();
            if (CurrentFrame.narrationAudio != null)
            {
                AudioManager.Instance?.PlayNarration(CurrentFrame.narrationAudio);
                StartCoroutine(WaitForNarration());
            }

            var mood = MoodDetector.DetectMood(CurrentFrame);
            AudioManager.Instance?.PlayMusic(mood);

            _episodeUI?.ShowFrame(CurrentFrame);
            OnFrameShown?.Invoke(CurrentFrame);

            if (_audioEnded)
                CheckFrameEnd();
        }

        private IEnumerator WaitForNarration()
        {
            yield return new WaitUntil(() => AudioManager.Instance == null || !AudioManager.Instance.IsNarrationPlaying);
            _audioEnded = true;
            CheckFrameEnd();
        }

        public void MarkTextComplete()
        {
            _textEnded = true;
            CheckFrameEnd();
        }

        private void CheckFrameEnd()
        {
            if (_audioEnded && _textEnded && !IsFrameComplete)
            {
                IsFrameComplete = true;
                if (_gamePending)
                {
                    _episodeUI?.ShowGameIsland(CurrentFrame.gameId, CurrentFrame.transitionTextKey);
                }
                else
                {
                    _episodeUI?.ShowNextButton();
                }
                OnFrameComplete?.Invoke();
            }
        }

        public void NextFrame()
        {
            if (!IsFrameComplete && _gamePending) return;
            PlayerProfile.Instance?.MarkFrameSeen(CurrentEpisode.id, CurrentFrameIndex);
            AchievementManager.Instance?.Check(AchievementType.Frame);

            if (CurrentFrameIndex + 1 >= CurrentEpisode.frames.Count)
            {
                EndEpisode();
                return;
            }
            ShowFrame(CurrentFrameIndex + 1);
        }

        public void PrevFrame()
        {
            if (CurrentFrameIndex <= 0) return;
            ShowFrame(CurrentFrameIndex - 1);
        }

        public void LaunchGame()
        {
            if (string.IsNullOrEmpty(CurrentFrame?.gameId)) return;
            AudioManager.Instance?.StopMusic(fast: true);
            OnGameRequired?.Invoke(CurrentFrame.gameId);
            Core.GameManager.Instance?.StartMinigame(CurrentFrame.gameId);
        }

        public void SkipGame()
        {
            _gamePending = false;
            IsFrameComplete = true;
            _episodeUI?.ShowNextButton();
        }

        public void AdvanceFromGame()
        {
            _gamePending = false;
            IsFrameComplete = true;
            NextFrame();
        }

        private void EndEpisode()
        {
            PlayerProfile.Instance?.CompleteEpisode(CurrentEpisode.id);
            AchievementManager.Instance?.Check(AchievementType.Episode, CurrentEpisode.id);
            _episodeUI?.ShowEndScreen();
            OnEpisodeEnded?.Invoke();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
