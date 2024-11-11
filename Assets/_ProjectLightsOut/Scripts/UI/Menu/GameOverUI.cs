using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI playtext;
    [SerializeField] private TextMeshProUGUI scoreText;
    private Vector2 gameOverTextOriginalPosition;
    private bool isPressed = false;

    private void Awake()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        gameOverTextOriginalPosition = gameOverText.rectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        EventManager.AddListener<OnGameOver>(OnGameOver);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<OnGameOver>(OnGameOver);
    }

    private void OnGameOver(OnGameOver e)
    {
        scoreText.text = ScoreManager.Score.ToString();
        StartCoroutine(GameOverAnimation());
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
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

        StartCoroutine(BlinkPlayText());

        gameOverText.alpha = 1;
        gameOverText.rectTransform.anchoredPosition = gameOverTextOriginalPosition;
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
