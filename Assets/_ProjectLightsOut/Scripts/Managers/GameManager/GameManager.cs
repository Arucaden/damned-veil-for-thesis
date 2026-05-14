using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        Tutorial
    }

    public class GameManager : Singleton<GameManager>
    {
        private Coroutine resetTimeScaleCoroutine;
        private GameState gameState = GameState.Playing;
        public GameState CurrentGameState => gameState;
        private bool isPaused = false;

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            EventManager.AddListener<OnSlowTime>(OnSlowTimeEvent);
            EventManager.AddListener<OnChangeGameState>(OnChangeGameState);
            EventManager.AddListener<OnGameOver>(OnGameOverEvent);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnSlowTime>(OnSlowTimeEvent);
            EventManager.RemoveListener<OnChangeGameState>(OnChangeGameState);
            EventManager.RemoveListener<OnGameOver>(OnGameOverEvent);
        }

        private void OnGameOverEvent(OnGameOver evt)
        {
            gameState = GameState.GameOver;
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
            if (gameState != GameState.Playing) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void OnChangeGameState(OnChangeGameState evt)
        {
            switch (evt.GameState)
            {
                case GameState.Playing:
                    EventManager.Broadcast(new OnResetScore());
                    EventManager.Broadcast(new OnPlayBGM("Gameplay"));
                    Cursor.visible = false;
                    break;
                case GameState.GameOver:
                    EventManager.Broadcast(new OnPlayBGM("GameOver"));
                    Cursor.visible = true;
                    break;
                case GameState.Paused:
                    break;
                case GameState.Tutorial:
                    Time.timeScale = 0f;
                    Cursor.visible = true;
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

        #region Pause
        private void TogglePause()
        {
            if (gameState != GameState.Playing)
                return;

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

        public async void RestartGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
            
            Cursor.visible = false;
            
            AppStateManager.Instance.RestartGameplay(LevelManager.Instance.LevelName);
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
            gameState = GameState.Playing;
            EventManager.Broadcast(new OnPause(false));
            EventManager.Broadcast(new OnPlayerEnableShooting(true));
            Cursor.visible = false;
        }

        public void QuitToMainMenu()
        {
            isPaused = false;
            Time.timeScale = 1f;
            
            AppStateManager.Instance.GoToMainMenu();
        }
            
        #endregion

    }
}