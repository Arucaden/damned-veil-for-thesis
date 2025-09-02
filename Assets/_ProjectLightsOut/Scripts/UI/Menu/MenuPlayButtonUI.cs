using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

public class MenuPlayButtonUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    private bool isPressed = false;

    public void OnClick()
    {
        if (isPressed) return;

        isPressed = true;
        AppStateManager.Instance.StartGameplay();
        // StartCoroutine(FadeOut());
        EventManager.Broadcast(new OnPlaySFX("Boom1"));
    }

    private IEnumerator FadeOut()
    {
        float duration = 1f;
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}
