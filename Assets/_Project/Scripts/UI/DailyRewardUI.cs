using Superglazka.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Superglazka.UI
{
    public class DailyRewardUI : MonoBehaviour
    {
        [SerializeField] private Text _streakText;
        [SerializeField] private Text _rewardText;
        [SerializeField] private Button _claimButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _claimedOverlay;

        private void OnEnable()
        {
            Refresh();
        }

        private void Start()
        {
            _claimButton?.onClick.AddListener(Claim);
            _closeButton?.onClick.AddListener(() => gameObject.SetActive(false));
        }

        private void Refresh()
        {
            var status = DailyRewardManager.Instance?.GetStatus() ?? (0, 0, false, false, 0);
            if (_streakText != null)
                _streakText.text = $"Стрик: {status.streak}";
            if (_rewardText != null)
                _rewardText.text = $"+{status.reward} монет";
            if (_claimButton != null)
                _claimButton.interactable = status.canClaim;
            if (_claimedOverlay != null)
                _claimedOverlay.SetActive(status.claimedToday);
        }

        private void Claim()
        {
            var reward = DailyRewardManager.Instance?.Claim();
            if (reward.HasValue)
            {
                HapticService.Instance?.VibrateSuccess();
                Refresh();
            }
        }
    }
}
