using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    public enum GameState
    {
        MainMenu,
        InGame,
        Paused,
        GameOver
    }

    public class GameManager : Singleton<GameManager>
    {
        private Coroutine resetTimeScaleCoroutine;
        private GameState gameState = GameState.MainMenu;
        private Action OnSceneLoadComplete;
        private bool isPaused = false;
        public static bool IsPaused => Instance.isPaused;

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            EventManager.AddListener<OnSlowTime>(OnSlowTimeEvent);
            EventManager.AddListener<OnChangeGameState>(OnChangeGameState);
            EventManager.AddListener<OnChangeScene>(OnChangeScene);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnSlowTime>(OnSlowTimeEvent);
            EventManager.RemoveListener<OnChangeGameState>(OnChangeGameState);
            EventManager.RemoveListener<OnChangeScene>(OnChangeScene);
        }

        private void Start()
        {
            if (LevelManager.Instance == null)
            {
                EventManager.Broadcast(new OnPlayBGM("MainMenu", fadeIn: 10f));
            }
        }

        private void Update()
        {
            if (gameState != GameState.InGame) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        // private void OnSceneLoaded()
        // {
        //     OnSceneLoadComplete?.Invoke();
        //     OnSceneLoadComplete = null;
        // }

        public void OnChangeGameState(OnChangeGameState evt)
        {
            switch (evt.GameState)
            {
                case GameState.MainMenu:
                    EventManager.Broadcast(new OnResetScore());
                    EventManager.Broadcast(new OnPlayBGM("MainMenu"));
                    Cursor.visible = true;
                    break;
                case GameState.InGame:
                    // StartCoroutine(ChangeScene(1f, "0-0"));
                    EventManager.Broadcast(new OnPlayBGM("Gameplay"));
                    Cursor.visible = false;
                    break;
                case GameState.GameOver:
                    EventManager.Broadcast(new OnPlayBGM("GameOver"));
                    break;
            }

            gameState = evt.GameState;
        }

        private void OnSlowTimeEvent(OnSlowTime evt)
        {
            SlowTime(evt.TimeScale, evt.Duration);
        }

        public static void SlowTime(float timeScale, float duration)
        {
            if (Instance.resetTimeScaleCoroutine != null)
            {
                Instance.StopCoroutine(Instance.resetTimeScaleCoroutine);
                Time.timeScale = 1f;
            }

            Time.timeScale = timeScale;
            Instance.resetTimeScaleCoroutine = Instance.StartCoroutine(Instance.ResetTimeScale(duration));
        }

        private IEnumerator ResetTimeScale(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }

        private void OnChangeScene(OnChangeScene evt)
        {
            // StartCoroutine(ChangeScene(evt.Delay, evt.SceneName));
            AppStateManager.Instance.GoToLevelSelect(evt.SceneName);
        }

        // private IEnumerator ChangeScene(float delay, string sceneName)
        // {
        //     yield return new WaitForSeconds(delay);
        //     UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        //     UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => OnSceneLoaded();
        // }

        #region Pause
        private void TogglePause()
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
            EventManager.Broadcast(new OnPause(isPaused));
            EventManager.Broadcast(new OnPlayerEnableShooting(!isPaused));
            Cursor.visible = isPaused;
        }

        public void RestartGame()
        {
            EventManager.Broadcast(new OnChangeGameState(GameState.InGame));
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
        }

        public void QuitToMainMenu()
        {
            EventManager.Broadcast(new OnChangeScene("Menu", 0f));
            EventManager.Broadcast(new OnChangeGameState(GameState.MainMenu));
            Time.timeScale = 1f;
        }
            
        #endregion

    }
}