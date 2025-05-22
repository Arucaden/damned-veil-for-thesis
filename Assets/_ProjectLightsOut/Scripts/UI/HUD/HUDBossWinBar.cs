using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;
using UnityEngine.UI;

public class HUDBossWinBar : MonoBehaviour
{
    [SerializeField] private Image chantBar;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Vector2 retractPosition;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float loseChantTime = 10f;
    private Color originalColor;
    private Vector2 originalPosition;
    private float fullBar;
    private Coroutine fillChantCoroutine;
    private float chantTime;
    private float lastChantTime;

    private void Awake()
    {
        fullBar = chantBar.rectTransform.sizeDelta.x;
        originalPosition = rectTransform.anchoredPosition;
        originalColor = chantBar.color;
        chantBar.rectTransform.sizeDelta = new Vector2(0, chantBar.rectTransform.sizeDelta.y);
        canvasGroup.alpha = 0;
        rectTransform.anchoredPosition = retractPosition;
    }

    private void Update()
    {
        if (chantTime > 0 && lastChantTime > 0)
        {
            lastChantTime -= Time.deltaTime;
        }

        else if (chantTime > 0)
        {
            chantTime -= Time.deltaTime;
            chantBar.rectTransform.sizeDelta = new Vector2(fullBar * (chantTime / loseChantTime), chantBar.rectTransform.sizeDelta.y);
        }
    }

    private void OnEnable()
    {
        EventManager.AddListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
        EventManager.AddListener<OnEnemyChant>(OnEnemyChant);
        EventManager.AddListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
        EventManager.RemoveListener<OnEnemyChant>(OnEnemyChant);
        EventManager.RemoveListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
    }

    private void OnPlayerEnableShooting(OnPlayerEnableShooting e)
    {
        if (!e.IsEnabled)
        {
            StartCoroutine(Retract());
            return;
        }

        StartCoroutine(Extend());
    }

    private void OnEnemyChant(OnEnemyChant e)
    {
        if (fillChantCoroutine != null)
        {
            StopCoroutine(fillChantCoroutine);
            chantBar.color = originalColor;
        }
        
        lastChantTime = 2f;
        chantTime += 0.2f;

        if (chantTime >= loseChantTime)
        {
            EventManager.Broadcast(new OnTriggerGameOver());
        }

        chantBar.rectTransform.sizeDelta = new Vector2(fullBar * (chantTime / loseChantTime), chantBar.rectTransform.sizeDelta.y);

        fillChantCoroutine = StartCoroutine(FillChant());
    }

    private IEnumerator FillChant()
    {
        float time = 0;
        float duration = 0.25f;

        while (time < duration)
        {
            time += Time.deltaTime;
            chantBar.color = Color.Lerp(originalColor, Color.red, time / duration);
            yield return null;
        }

        time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            chantBar.color = Color.Lerp(Color.red, originalColor, time / duration);
            yield return null;
        }

        fillChantCoroutine = null;
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
