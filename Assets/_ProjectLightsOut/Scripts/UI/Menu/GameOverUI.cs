using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI playtext;
    [SerializeField] private TextMeshProUGUI retryText;
    [SerializeField] private TextMeshProUGUI scoreText;
    private Vector2 gameOverTextOriginalPosition;
    private Color originalColor;
    private bool isPressed = false;

    private void Awake()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        gameOverTextOriginalPosition = gameOverText.rectTransform.anchoredPosition;
        originalColor = playtext.color;
    }

    private void OnEnable()
    {
        EventManager.AddListener<OnGameOver>(OnGameOver);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<OnGameOver>(OnGameOver);
    }

    public void OnPointerEnter(TextMeshProUGUI text)
    {
        text.color = Color.white;
    }

    public void OnPointerExit(TextMeshProUGUI text)
    {
        text.color = originalColor;
    }

    private void OnGameOver(OnGameOver e)
    {
        Cursor.visible = true;
        scoreText.text = ScoreManager.Score.ToString();
        StartCoroutine(GameOverAnimation());
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnRetryButtonClicked()
    {
        if (isPressed) return;

        isPressed = true;
        EventManager.Broadcast(new OnPlaySFX("Boom1"));
        EventManager.Broadcast(new OnFadeBlack());
        EventManager.Broadcast(new OnPlayBGM("Gameplay"));

        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (currentSceneName == "0-0")
        {
            currentSceneName = "0-1";
        }
        EventManager.Broadcast(new OnChangeScene(currentSceneName, 3f));
        EventManager.Broadcast(new OnRollbackScore());
    }

    public void OnTitleScreenButtonClicked()
    {
        if (isPressed) return;

        isPressed = true;
        EventManager.Broadcast(new OnPlaySFX("Boom1"));
        EventManager.Broadcast(new OnChangeScene("Menu", 3f));
    }

    private IEnumerator GameOverAnimation()
    {
        gameOverText.alpha = 0;
        gameOverText.rectTransform.anchoredPosition = new Vector2(gameOverTextOriginalPosition.x, gameOverTextOriginalPosition.y + 100);
        float time = 0;
        float duration = 1f;

        while (time < duration)
        {
            time += Time.deltaTime;
            gameOverText.alpha = Mathf.Lerp(0, 1, time / duration);
            canvasGroup.alpha = Mathf.Lerp(0, 1, time / duration);
            gameOverText.rectTransform.anchoredPosition = Vector2.Lerp(gameOverText.rectTransform.anchoredPosition, gameOverTextOriginalPosition, time / duration);
            yield return null;
        }

        gameOverText.alpha = 1;
        gameOverText.rectTransform.anchoredPosition = gameOverTextOriginalPosition;
    }
}
