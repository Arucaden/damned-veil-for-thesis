using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{

    [SerializeField]
    GameObject _pauseUI;
    private void OnPauseEvent(OnPause pauseEvent)
    {
        if (pauseEvent.IsPaused)
        {
            Time.timeScale = 0f;
            _pauseUI.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            _pauseUI.SetActive(false);
        }
    }
    private void OnEnable()
    {
        EventManager.AddListener<OnPause>(OnPauseEvent);
    }
    private void OnDisable()
    {
        EventManager.RemoveListener<OnPause>(OnPauseEvent);
    }
    public void OnResumePressed()
    {
        Time.timeScale = 1f;
        _pauseUI.SetActive(false);
        GameManager.Instance.ResumeGame();
    }

    public void OnRestartPressed()
    {
        Time.timeScale = 1f;
        _pauseUI.SetActive(false);
        GameManager.Instance.RestartGame();
    }

    public void OnQuitPressed()
    {
        Time.timeScale = 1f;
        _pauseUI.SetActive(false);
        GameManager.Instance.QuitToMainMenu();
    }
}
