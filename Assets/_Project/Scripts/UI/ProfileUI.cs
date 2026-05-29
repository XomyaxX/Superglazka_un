using Superglazka.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Superglazka.UI
{
    public class ProfileUI : MonoBehaviour
    {
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _coinsText;
        [SerializeField] private InputField _nameInput;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Transform _statsContainer;
        [SerializeField] private GameObject _statPrefab;

        private void OnEnable()
        {
            Refresh();
        }

        private void Start()
        {
            _saveButton?.onClick.AddListener(SaveName);
            _closeButton?.onClick.AddListener(() => gameObject.SetActive(false));
        }

        private void Refresh()
        {
            var profile = PlayerProfile.Instance;
            if (profile == null) return;
            if (_nameText != null) _nameText.text = profile.Nickname;
            if (_coinsText != null) _coinsText.text = $"{profile.Coins}";
            if (_nameInput != null) _nameInput.text = profile.Nickname;

            if (_statsContainer != null)
            {
                foreach (Transform child in _statsContainer)
                    Destroy(child.gameObject);

                // Episode stats
                var data = profile.GetProfileData();
                foreach (var kv in data.episodes)
                {
                    var go = Instantiate(_statPrefab, _statsContainer);
                    var txt = go.GetComponent<Text>();
                    if (txt != null)
                        txt.text = $"{kv.Key}: {kv.Value.framesSeen}/{kv.Value.maxFrame} {(kv.Value.completed ? "✓" : "")}";
                }
                // Game stats
                foreach (var kv in data.games)
                {
                    var go = Instantiate(_statPrefab, _statsContainer);
                    var txt = go.GetComponent<Text>();
                    if (txt != null)
                        txt.text = $"{kv.Key}: {kv.Value.bestScore} (x{kv.Value.played})";
                }
            }
        }

        private void SaveName()
        {
            if (_nameInput != null && !string.IsNullOrWhiteSpace(_nameInput.text))
            {
                PlayerProfile.Instance?.SetNickname(_nameInput.text.Trim());
                Refresh();
                HapticService.Instance?.VibrateSuccess();
            }
        }
    }
}
