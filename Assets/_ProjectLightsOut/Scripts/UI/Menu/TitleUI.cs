using System;
using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TitleUI : MonoBehaviour
{
    [SerializeField] private Vector2 zoomedPosition;
    private Vector2 originalPosition;
    [SerializeField] private float zoomScale = 1.5f;
    [SerializeField] private CanvasGroup shadowImage;
    [SerializeField] private float shadowImageAlpha = 99f;
    [SerializeField] private TextMeshProUGUI playText;
    [SerializeField] private GameObject playButton;
    [SerializeField] private List<Light2D> moonlights;
    private float originalLightIntensity;
    private float originalScale = 1f;
    private RectTransform rectTransform;

    private void Awake()
    {
        originalLightIntensity = moonlights[0].intensity;
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale.x;
        shadowImage.alpha = 1f;
        rectTransform.localScale = Vector2.one * zoomScale;

        playText.alpha = 0f;
        playButton.SetActive(false);

        rectTransform.anchoredPosition = zoomedPosition;
        StartCoroutine(ZoomOut());

        moonlights.ForEach(moonlight => moonlight.intensity = 0f);
    }

    private IEnumerator ZoomOut()
    {
        yield return new WaitForSeconds(1f);

        StartCoroutine(LightAnimation());

        EventManager.Broadcast(new OnPlaySFX("Boom2"));

        yield return new WaitForSeconds(2f);
        float time = 0f;
        float duration = 0.6f;
        float colorAlpha = 0f;

        EventManager.Broadcast(new OnPlaySFX("Boom3"));

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            rectTransform.anchoredPosition = Vector2.Lerp(zoomedPosition, originalPosition, t);
            rectTransform.localScale = Vector3.one * Mathf.Lerp(zoomScale, originalScale * 0.9f, t);
            foreach (var moonlight in moonlights)
            {
                moonlight.intensity = Mathf.Lerp(originalLightIntensity, 0f, t);
            }
            colorAlpha = Mathf.Lerp(1f, shadowImageAlpha, t);
            shadowImage.alpha = colorAlpha;
            yield return null;
        }

        time = 0f;
        duration = 0.2f;

        // bounce effect
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            rectTransform.localScale = Vector3.one * Mathf.Lerp(originalScale * 0.9f, originalScale, t);
            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = new Vector3(originalScale, originalScale, originalScale);
        shadowImage.alpha = shadowImageAlpha;

        playButton.SetActive(true);
        StartCoroutine(BlinkPlayText());
    }

    private IEnumerator LightAnimation()
    {
        float time = 0f;
        float duration = 0.8f;

        // bounce effect
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            foreach (var moonlight in moonlights)
            {
                moonlight.intensity = Mathf.Lerp(0f, originalLightIntensity, t);
            }
            yield return null;
        }
    }

    private IEnumerator BlinkPlayText()
    {
        float time = 0f;
        float duration = 1f;

        while (time < 0.2f)
        {
            time += Time.deltaTime;
            float t = time / duration;
            playText.alpha = Mathf.Lerp(0f, 0.2f, t);
            yield return null;
        }

        while (true)
        {
            time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                playText.alpha = Mathf.Lerp(0.2f, 1f, t);
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
            time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                playText.alpha = Mathf.Lerp(1f, 0.2f, t);
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
