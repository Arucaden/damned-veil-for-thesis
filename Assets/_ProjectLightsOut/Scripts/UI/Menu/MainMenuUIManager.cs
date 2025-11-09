using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    private bool isPressed = false;

    public void OnPlayButtonClick()
    {
        if (isPressed) return;

        isPressed = true;
        AppStateManager.Instance.StartGameplay();
        EventManager.Broadcast(new OnPlaySFX("Boom1"));
    }

    public void OnExitButtonClick()
    {
        Debug.Log("Exit button clicked");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void OnSettingsButtonClick()
    {
        Debug.Log("Settings button clicked");
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
