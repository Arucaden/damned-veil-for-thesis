using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using TMPro;
using UnityEngine;

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

        AppStateManager.Instance.RestartGameplay(LevelManager.Instance.LevelName);
    }

    public void OnTitleScreenButtonClicked()
    {
        if (isPressed) return;

        isPressed = true;
        EventManager.Broadcast(new OnPlaySFX("Boom1"));
        EventManager.Broadcast(new OnChangeScene("Menu", 3f));
        Cursor.visible = false;

        AppStateManager.Instance.GoToMainMenu();
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
