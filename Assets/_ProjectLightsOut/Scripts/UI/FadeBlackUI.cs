using System;
using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

public class FadeBlackUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool FadeInOnStart = false;
    private Action OnFadeBlackComplete;

    private void Awake()
    {
        if (FadeInOnStart)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            StartCoroutine(FadeIn(1f, 2f));
        }

        else
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void OnEnable()
    {
        EventManager.AddListener<OnChangeScene>(OnChangeScene);
        EventManager.AddListener<OnCompleteCountingScore>(OnCompleteCountingScore);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<OnChangeScene>(OnChangeScene);
        EventManager.RemoveListener<OnCompleteCountingScore>(OnCompleteCountingScore);
    }

    private void OnCompleteCountingScore(OnCompleteCountingScore e)
    {
        StartCoroutine(FadeBlack(1f));
    }

    private void OnChangeScene(OnChangeScene e)
    {
        if (e.SceneName == "Menu")
        {
            StartCoroutine(FadeBlack(1f));
            OnFadeBlackComplete += () => {EventManager.Broadcast(new OnChangeGameState(GameState.MainMenu)); OnFadeBlackComplete = null;};
        }
    }

    private IEnumerator FadeIn(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            canvasGroup.alpha = 1 - timeElapsed / duration;
            yield return null;
        }
        canvasGroup.alpha = 0;
    }

    private IEnumerator FadeBlack(float duration)
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            canvasGroup.alpha = timeElapsed / duration;
            yield return null;
        }
        canvasGroup.alpha = 1;
        OnFadeBlackComplete?.Invoke();
    }
}
