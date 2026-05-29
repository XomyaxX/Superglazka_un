using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Superglazka.UI
{
    public class SceneTransition : MonoBehaviour
    {
        [SerializeField] private Image _fadeImage;
        [SerializeField] private float _fadeDuration = 0.5f;

        public static SceneTransition Instance { get; private set; }

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

        public IEnumerator FadeIn()
        {
            if (_fadeImage == null) yield break;
            _fadeImage.gameObject.SetActive(true);
            float timer = 0f;
            while (timer < _fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                _fadeImage.color = new Color(0, 0, 0, 1f - timer / _fadeDuration);
                yield return null;
            }
            _fadeImage.color = new Color(0, 0, 0, 0);
            _fadeImage.gameObject.SetActive(false);
        }

        public IEnumerator FadeOut()
        {
            if (_fadeImage == null) yield break;
            _fadeImage.gameObject.SetActive(true);
            float timer = 0f;
            while (timer < _fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                _fadeImage.color = new Color(0, 0, 0, timer / _fadeDuration);
                yield return null;
            }
            _fadeImage.color = new Color(0, 0, 0, 1);
        }
    }
}
