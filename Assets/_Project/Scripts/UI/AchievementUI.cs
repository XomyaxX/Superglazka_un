using System.Collections.Generic;
using Superglazka.Data;
using Superglazka.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Superglazka.UI
{
    public class AchievementUI : MonoBehaviour
    {
        [SerializeField] private Transform _gridContainer;
        [SerializeField] private GameObject _achievementPrefab;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Text _counterText;

        [SerializeField] private List<AchievementData> _achievementList;

        private void OnEnable()
        {
            Refresh();
        }

        private void Start()
        {
            _closeButton?.onClick.AddListener(() => gameObject.SetActive(false));
            AchievementManager.Instance.OnAchievementUnlocked += OnUnlocked;
        }

        private void OnDestroy()
        {
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.OnAchievementUnlocked -= OnUnlocked;
        }

        private void OnUnlocked(AchievementData ach)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_gridContainer == null || _achievementPrefab == null) return;
            foreach (Transform child in _gridContainer)
                Destroy(child.gameObject);

            int unlocked = 0;
            foreach (var ach in _achievementList)
            {
                bool isUnlocked = AchievementManager.Instance?.IsUnlocked(ach.key) ?? false;
                if (isUnlocked) unlocked++;

                var go = Instantiate(_achievementPrefab, _gridContainer);
                var img = go.GetComponent<Image>();
                var title = go.transform.Find("Title")?.GetComponent<Text>();
                var desc = go.transform.Find("Desc")?.GetComponent<Text>();
                var icon = go.transform.Find("Icon")?.GetComponent<Image>();
                var lockOverlay = go.transform.Find("Lock")?.gameObject;

                if (title != null) title.text = LocalizationService.Instance?.Translate(ach.titleKey) ?? ach.titleKey;
                if (desc != null) desc.text = LocalizationService.Instance?.Translate(ach.descKey) ?? ach.descKey;
                if (icon != null && ach.icon != null) icon.sprite = ach.icon;
                if (img != null) img.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
                if (lockOverlay != null) lockOverlay.SetActive(!isUnlocked);
            }

            if (_counterText != null)
                _counterText.text = $"{unlocked}/{_achievementList.Count}";
        }
    }
}
