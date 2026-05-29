using UnityEngine;

namespace Superglazka.Services
{
    public class HapticService : MonoBehaviour
    {
        public static HapticService Instance { get; private set; }

        [SerializeField] private bool _enabled = true;
        [SerializeField] private bool _reduceMotion;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetEnabled(bool enabled) => _enabled = enabled;
        public void SetReduceMotion(bool reduce) => _reduceMotion = reduce;

        public void VibrateSuccess()
        {
            if (!_enabled || _reduceMotion) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        public void VibratePattern(long[] pattern)
        {
            if (!_enabled || _reduceMotion) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            // Android supports patterns via Unity API indirectly; use single vibrate for simplicity
            if (pattern != null && pattern.Length > 0)
                Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        public void VibrateLight()
        {
            if (!_enabled || _reduceMotion) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
