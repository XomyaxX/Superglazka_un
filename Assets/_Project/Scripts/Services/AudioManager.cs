using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Superglazka.Services
{
    public enum MusicMood
    {
        Cosmic, Joyful, Tension, Peaceful, Magical,
        Triumphant, Warm, Mystery, Epic, Sad, Action
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _musicSource2;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _narrationSource;

        [Header("Music Clips")]
        [SerializeField] private List<MusicEntry> _musicClips;

        [Header("Settings")]
        [Range(0f, 1f)] [SerializeField] private float _musicVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float _sfxVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float _narrationVolume = 1f;
        [SerializeField] private float _crossfadeDuration = 1.5f;

        private AudioSource _activeMusic;
        private AudioSource _inactiveMusic;
        private Coroutine _crossfadeCoroutine;
        private Dictionary<MusicMood, AudioClip> _musicDict;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _activeMusic = _musicSource;
            _inactiveMusic = _musicSource2;
            BuildMusicDictionary();
            ApplyVolumes();
        }

        private void BuildMusicDictionary()
        {
            _musicDict = new Dictionary<MusicMood, AudioClip>();
            foreach (var entry in _musicClips)
            {
                if (entry.clip != null)
                    _musicDict[entry.mood] = entry.clip;
            }
        }

        public void PlayMusic(MusicMood mood, bool loop = true)
        {
            if (!_musicDict.TryGetValue(mood, out var clip) || clip == null)
            {
                Debug.LogWarning($"Music clip for mood {mood} not found.");
                return;
            }
            if (_activeMusic.isPlaying && _activeMusic.clip == clip) return;
            StartCrossfade(clip, loop);
        }

        public void StopMusic(bool fast = false)
        {
            if (fast)
            {
                _activeMusic.Stop();
                _inactiveMusic.Stop();
            }
            else
            {
                StartCoroutine(FadeOut(_activeMusic, _crossfadeDuration));
            }
        }

        public void PlaySFX(AudioClip clip, float pitchVariation = 0f)
        {
            if (clip == null) return;
            _sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            _sfxSource.PlayOneShot(clip, _sfxVolume);
        }

        public void PlayNarration(AudioClip clip)
        {
            if (clip == null) return;
            _narrationSource.Stop();
            _narrationSource.clip = clip;
            _narrationSource.volume = _narrationVolume;
            _narrationSource.Play();
        }

        public void StopNarration()
        {
            _narrationSource.Stop();
        }

        public bool IsNarrationPlaying => _narrationSource.isPlaying;
        public float NarrationTime => _narrationSource.time;
        public float NarrationLength => _narrationSource.clip != null ? _narrationSource.clip.length : 0f;

        public void SetMusicVolume(float vol)
        {
            _musicVolume = Mathf.Clamp01(vol);
            ApplyVolumes();
        }

        public void SetSfxVolume(float vol)
        {
            _sfxVolume = Mathf.Clamp01(vol);
            _sfxSource.volume = _sfxVolume;
        }

        public void SetNarrationVolume(float vol)
        {
            _narrationVolume = Mathf.Clamp01(vol);
            _narrationSource.volume = _narrationVolume;
        }

        private void ApplyVolumes()
        {
            _musicSource.volume = _musicVolume;
            _musicSource2.volume = _musicVolume;
            _sfxSource.volume = _sfxVolume;
            _narrationSource.volume = _narrationVolume;
        }

        private void StartCrossfade(AudioClip newClip, bool loop)
        {
            if (_crossfadeCoroutine != null)
                StopCoroutine(_crossfadeCoroutine);
            _crossfadeCoroutine = StartCoroutine(Crossfade(newClip, loop));
        }

        private IEnumerator Crossfade(AudioClip newClip, bool loop)
        {
            _inactiveMusic.clip = newClip;
            _inactiveMusic.loop = loop;
            _inactiveMusic.volume = 0f;
            _inactiveMusic.Play();

            float timer = 0f;
            while (timer < _crossfadeDuration)
            {
                float t = timer / _crossfadeDuration;
                _activeMusic.volume = Mathf.Lerp(_musicVolume, 0f, t);
                _inactiveMusic.volume = Mathf.Lerp(0f, _musicVolume, t);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            _activeMusic.Stop();
            _activeMusic.volume = _musicVolume;
            (_activeMusic, _inactiveMusic) = (_inactiveMusic, _activeMusic);
        }

        private IEnumerator FadeOut(AudioSource source, float duration)
        {
            float startVol = source.volume;
            float timer = 0f;
            while (timer < duration)
            {
                source.volume = Mathf.Lerp(startVol, 0f, timer / duration);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            source.Stop();
            source.volume = startVol;
        }

        [System.Serializable]
        public class MusicEntry
        {
            public MusicMood mood;
            public AudioClip clip;
        }
    }
}
