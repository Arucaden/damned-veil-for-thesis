using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Gameplay;
using ProjectLightsOut.Managers;
using UnityEngine;
using UnityEngine.UI;

public class HUDBossHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Vector2 retractPosition;
    [SerializeField] private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private float fullHealth;
    private Vector2 originalScale;
    private Color originalColor;
    private Boss boss;
    private Coroutine healthColorCoroutine;

    private void Awake()
    {
        fullHealth = healthBar.rectTransform.sizeDelta.x;
        originalPosition = rectTransform.anchoredPosition;
        healthBar.rectTransform.sizeDelta = new Vector2(0, healthBar.rectTransform.sizeDelta.y);
        canvasGroup.alpha = 0;
        rectTransform.anchoredPosition = retractPosition;
        originalScale = rectTransform.localScale;
        originalColor = healthBar.color;
    }

    private void OnEnable()
    {
        EventManager.AddListener<OnBossReady>(OnBossReady);
        EventManager.AddListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
        EventManager.AddListener<OnBossRegister>(OnBossRegister);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<OnBossReady>(OnBossReady);
        EventManager.RemoveListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
        EventManager.RemoveListener<OnBossRegister>(OnBossRegister);
    }

    private void OnBossRegister(OnBossRegister e)
    {
        boss = e.Boss;
        boss.OnBossDamaged += OnBossHurt;
        boss.OnBossHealed += OnBossHealed;
    }

    private void OnBossHealed()
    {
        int health = boss.Health;

        healthBar.rectTransform.sizeDelta = new Vector2(fullHealth * ((float)health / boss.MaxHealth), healthBar.rectTransform.sizeDelta.y);
        
        if (healthColorCoroutine != null)
        {
            StopCoroutine(healthColorCoroutine);
        }

        healthColorCoroutine = StartCoroutine(HealCoroutine());
    }

    private void OnBossHurt()
    {
        int health = boss.Health;

        healthBar.rectTransform.sizeDelta = new Vector2(fullHealth * ((float)health / boss.MaxHealth), healthBar.rectTransform.sizeDelta.y);
    }

    private IEnumerator HealCoroutine()
    {
        float time = 0;
        float duration = 0.25f;

        while (time < duration)
        {
            time += Time.deltaTime;
            healthBar.color = Color.Lerp(originalColor, Color.red, time / duration);
            yield return null;
        }

        time = 0;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            healthBar.color = Color.Lerp(Color.red, originalColor, time / duration);
            yield return null;
        }

        healthBar.color = originalColor;
    }

    private void OnPlayerEnableShooting(OnPlayerEnableShooting e)
    {
        if (!e.IsEnabled)
        {
            StartCoroutine(Retract());
        }
        else
        {
            // When re-enabling shooting (unpausing), extend the health bar back
            if (boss != null && healthBar.gameObject.activeSelf)
            {
                StartCoroutine(Extend());
            }
        }
    }

    private void OnBossReady(OnBossReady e)
    {
        healthBar.gameObject.SetActive(true);

        boss = e.Boss;

        StartCoroutine(BossReady());
    }

    private IEnumerator BossReady()
    {
        yield return StartCoroutine(Extend());

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(FillHealth());
    }

    private IEnumerator Retract()
    {
        float time = 0;
        float duration = 0.5f;
        canvasGroup.alpha = 1;

        Vector2 currentPos = rectTransform.anchoredPosition;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime; // Use unscaled time to work during pause
            rectTransform.anchoredPosition = Vector2.Lerp(currentPos, retractPosition, time / duration);
            canvasGroup.alpha = Mathf.Lerp(1, 0f, time / duration);
            yield return null;
        }

        canvasGroup.alpha = 0;
    }

    private IEnumerator Extend()
    {
        float time = 0;
        float duration = 0.9f;
        Vector2 currentPos = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0;
        healthBar.color = Color.red;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime; // Use unscaled time to work during pause
            rectTransform.anchoredPosition = Vector2.Lerp(currentPos, originalPosition, time / duration);
            rectTransform.localScale = Vector2.Lerp(originalScale, originalScale * 1.15f, time / duration);
            canvasGroup.alpha = Mathf.Lerp(0, 1f, time / duration);
            yield return null;
        }

        canvasGroup.alpha = 1;
    }

    private IEnumerator FillHealth()
    {
        float time = 0;
        float duration = 2f;

        while (time < duration)
        {
            time += Time.deltaTime;
            healthBar.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(0, fullHealth, time / duration), healthBar.rectTransform.sizeDelta.y);
            yield return null;
        }

        time = 0;
        while (time < 0.25f)
        {
            time += Time.deltaTime;
            rectTransform.localScale = Vector2.Lerp(originalScale * 1.15f, originalScale, time / 0.25f);
            healthBar.color = Color.Lerp(Color.red, originalColor, time / 0.25f);
            yield return null;
        }

        healthBar.rectTransform.sizeDelta = new Vector2(fullHealth, healthBar.rectTransform.sizeDelta.y);
    }
}
