using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using TMPro;
using UnityEngine;

namespace ProjectLightsOut.UI
{
    public class HUDLevelNameUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelNameText;
        [SerializeField] private Vector2 retractPosition;
        [SerializeField] private CanvasGroup canvasGroup;

        private RectTransform rectTransform;
        private Vector2 originalPosition;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalPosition = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = retractPosition;

            if (levelNameText == null)
                Debug.LogError("[HUDLevelNameUI] levelNameText is not assigned!");
        }

        private void Start()
        {
            string displayName = LevelManager.LevelData != null && !string.IsNullOrEmpty(LevelManager.LevelData.DisplayName)
                ? LevelManager.LevelData.DisplayName
                : LevelManager.Instance.LevelName;

            levelNameText.text = $"{displayName}";
        }

        private void OnEnable()
        {
            EventManager.AddListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
        }

        private void OnPlayerEnableShooting(OnPlayerEnableShooting e)
        {
            if (e.IsEnabled)
                StartCoroutine(Extend());
            else
                StartCoroutine(Retract());
        }

        private IEnumerator Extend()
        {
            float time = 0;
            float duration = 0.5f;
            Vector2 currentPos = rectTransform.anchoredPosition;
            canvasGroup.alpha = 0;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                rectTransform.anchoredPosition = Vector2.Lerp(currentPos, originalPosition, time / duration);
                canvasGroup.alpha = Mathf.Lerp(0, 1f, time / duration);
                yield return null;
            }

            canvasGroup.alpha = 1;
        }

        private IEnumerator Retract()
        {
            float time = 0;
            float duration = 0.5f;
            canvasGroup.alpha = 1;
            Vector2 currentPos = rectTransform.anchoredPosition;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                rectTransform.anchoredPosition = Vector2.Lerp(currentPos, retractPosition, time / duration);
                canvasGroup.alpha = Mathf.Lerp(1, 0f, time / duration);
                yield return null;
            }

            canvasGroup.alpha = 0;
        }
    }
}
