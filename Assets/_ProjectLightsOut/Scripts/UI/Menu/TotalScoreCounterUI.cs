using System;
using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TotalScoreCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI bonusText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI playtext;
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI areaClearText;
    private Vector2 areaClearTextOriginalPosition;
    private Action OnCountingBonusComplete;
    private Coroutine scoreUpCoroutine;
    private Vector2 bonusTextOriginalPosition;
    private bool isCounting;
    private bool isPressed = false;
    private int score;

    private void Awake()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        bonusTextOriginalPosition = bonusText.rectTransform.anchoredPosition;
        areaClearTextOriginalPosition = areaClearText.rectTransform.anchoredPosition;
        playtext.alpha = 0;

        playButton.onClick.AddListener(ClickProceedNextLevel);
    }

    private void OnEnable()
    {
        EventManager.AddListener<OnPostScore>(OnPostScore);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<OnPostScore>(OnPostScore);
    }

    private void OnPostScore(OnPostScore evt)
    {
        StartCoroutine(Result());
        scoreUpCoroutine = StartCoroutine(CountScore(evt));
    }

    private void SkipCountAnimation()
    {
        isCounting = false;
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        StopAllCoroutines();
        bonusText.alpha = 0;
        totalScoreText.text = score.ToString();
        StartCoroutine(BlinkPlayText());
    }

    private IEnumerator Result()
    {
        yield return new WaitForSeconds(0.4f);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        float time = 0;
        float duration = 1f;
        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, time / duration);
            yield return null;
        }
    }

    private IEnumerator CountScore(OnPostScore evt)
    {
        StartCoroutine(AreaClearAnimation());

        score = evt.Score + evt.LevelBonus + evt.TimeBonus + evt.BulletBonus;
        isCounting = true;

        yield return new WaitForSeconds(0.7f);

        yield return StartCoroutine(ScoreUpAnimation(evt.Score));
        yield return new WaitForSeconds(0.3f);
        bonusText.text = "+ Level Score";
        StartCoroutine(ScoreBonusAnimation());
        yield return StartCoroutine(ScoreUpAnimation(evt.Score + evt.LevelBonus));
        yield return new WaitForSeconds(0.3f);
        bonusText.text = "+ Time Bonus";
        StartCoroutine(ScoreBonusAnimation());
        yield return StartCoroutine(ScoreUpAnimation(evt.Score + evt.LevelBonus + evt.TimeBonus));
        yield return new WaitForSeconds(0.3f);
        bonusText.text = "+ Magic Bullet Remaining";
        StartCoroutine(ScoreBonusAnimation());
        yield return StartCoroutine(ScoreUpAnimation(evt.Score + evt.LevelBonus + evt.TimeBonus + evt.BulletBonus));
        
        isCounting = false;
        StartCoroutine(BlinkPlayText());

        yield return new WaitForSeconds(1f);

        float time = 0;
        float duration = 0.5f;

        while (time < duration)
        {
            time += Time.deltaTime;
            bonusText.alpha = Mathf.Lerp(1, 0, time / duration);
            yield return null;
        }
    }

    private IEnumerator ScoreUpAnimation(int score)
    {
        int currentScore = totalScoreText.text == "" ? 0 : int.Parse(totalScoreText.text);
        float time = 0;
        float duration = 0.5f;

        while (time < duration)
        {
            time += Time.deltaTime;
            totalScoreText.text = Mathf.RoundToInt(Mathf.Lerp(currentScore, score, time / duration)).ToString();
            EventManager.Broadcast(new OnPlaySFX("Point"));
            yield return null;
        }

        totalScoreText.text = score.ToString();
        OnCountingBonusComplete?.Invoke();
    }

    private IEnumerator ScoreBonusAnimation()
    {
        bonusText.alpha = 0;
        bonusText.rectTransform.anchoredPosition = new Vector2(bonusTextOriginalPosition.x, bonusTextOriginalPosition.y + 50);
        float time = 0;
        float duration = 0.5f;

        EventManager.Broadcast(new OnPlaySFX("Bonus"));

        while (time < duration)
        {
            time += Time.deltaTime;
            bonusText.alpha = Mathf.Lerp(0, 1, time / duration);
            bonusText.rectTransform.anchoredPosition = Vector2.Lerp(bonusText.rectTransform.anchoredPosition, bonusTextOriginalPosition, time / duration);
            yield return null;
        }

        bonusText.alpha = 1;
        bonusText.rectTransform.anchoredPosition = bonusTextOriginalPosition;
    }

    private IEnumerator AreaClearAnimation()
    {
        areaClearText.alpha = 0;
        areaClearText.rectTransform.anchoredPosition = new Vector2(areaClearTextOriginalPosition.x, areaClearTextOriginalPosition.y + 100);
        float time = 0;
        float duration = 1f;

        while (time < duration)
        {
            time += Time.deltaTime;
            areaClearText.alpha = Mathf.Lerp(0, 1, time / duration);
            areaClearText.rectTransform.anchoredPosition = Vector2.Lerp(areaClearText.rectTransform.anchoredPosition, areaClearTextOriginalPosition, time / duration);
            yield return null;
        }

        areaClearText.alpha = 1;
        areaClearText.rectTransform.anchoredPosition = areaClearTextOriginalPosition;
    }

    public void ClickProceedNextLevel()
    {
        if (isPressed) return;

        if (isCounting)
        {
            SkipCountAnimation();
            return;
        }

        isPressed = true;
        EventManager.Broadcast(new OnPlaySFX("Boom1"));
        EventManager.Broadcast(new OnCompleteCountingScore());
    }

    private IEnumerator BlinkPlayText()
        {
            float time = 0f;
            float duration = 1f;

            while (true)
            {
                time = 0f;
                while (time < duration)
                {
                    time += Time.deltaTime;
                    float t = time / duration;
                    playtext.alpha = Mathf.Lerp(0f, 1f, t);
                    yield return null;
                }

                yield return new WaitForSeconds(0.1f);
                time = 0f;

                while (time < duration)
                {
                    time += Time.deltaTime;
                    float t = time / duration;
                    playtext.alpha = Mathf.Lerp(1f, 0.2f, t);
                    yield return null;
                }

                yield return new WaitForSeconds(0.1f);
            }
        }
}
