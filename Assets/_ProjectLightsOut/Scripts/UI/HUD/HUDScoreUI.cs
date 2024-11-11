using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using TMPro;
using UnityEngine;

namespace ProjectLightsOut.UI
{
    public class HUDScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Vector2 retractPosition;
        [SerializeField] private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private Vector2 originalPosition;
        private int realScore;
        private Coroutine scoreUpCoroutine;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalPosition = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = retractPosition;

            if (scoreText == null)
            {
                Debug.LogError("Score Text is not assigned in HUDScoreUI");
            }
        }

        private void Start()
        {
            scoreText.text = ScoreManager.Score.ToString();
        }

        private void OnEnable()
        {
            EventManager.AddListener<OnScoreChange>(OnScoreChange);
            EventManager.AddListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnScoreChange>(OnScoreChange);
            EventManager.RemoveListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
        }

        private void OnPlayerEnableShooting(OnPlayerEnableShooting e)
        {
            if (e.IsEnabled)
            {
                StartCoroutine(Extend());
            }

            else
            {
                StartCoroutine(Retract());
            }
        }

        private void OnScoreChange(OnScoreChange e)
        {
            if (scoreUpCoroutine != null)
            {
                scoreText.text = realScore.ToString();
                StopCoroutine(scoreUpCoroutine);
            }

            realScore = e.Score;
            scoreUpCoroutine = StartCoroutine(ScoreUpAnimation(e.Score));
        }

        private IEnumerator ScoreUpAnimation(int score)
        {
            int currentScore = scoreText.text == "" ? 0 : int.Parse(scoreText.text);
            float time = 0;
            float duration = 0.5f;

            while (time < duration)
            {
                time += Time.deltaTime;
                scoreText.text = Mathf.RoundToInt(Mathf.Lerp(currentScore, score, time / duration)).ToString();
                yield return null;
            }

            scoreText.text = score.ToString();
            scoreText.text = realScore.ToString();
        }

        private IEnumerator Retract()
        {
            float time = 0;
            float duration = 0.5f;
            canvasGroup.alpha = 1;

            Vector2 currentPos = rectTransform.anchoredPosition;

            while (time < duration)
            {
                time += Time.deltaTime;
                rectTransform.anchoredPosition = Vector2.Lerp(currentPos, retractPosition, time / duration);
                canvasGroup.alpha = Mathf.Lerp(1, 0f, time / duration);
                yield return null;
            }

            canvasGroup.alpha = 0;
        }

        private IEnumerator Extend()
        {
            float time = 0;
            float duration = 0.5f;
            Vector2 currentPos = rectTransform.anchoredPosition;
            canvasGroup.alpha = 0;

            while (time < duration)
            {
                time += Time.deltaTime;
                rectTransform.anchoredPosition = Vector2.Lerp(currentPos, originalPosition, time / duration);
                canvasGroup.alpha = Mathf.Lerp(0, 1f, time / duration);
                yield return null;
            }

            canvasGroup.alpha = 1;
        }
    }
}
