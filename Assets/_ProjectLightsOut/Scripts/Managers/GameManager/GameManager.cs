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
        GameOver
    }

    public class GameManager : Singleton<GameManager>
    {
        private Coroutine resetTimeScaleCoroutine;
        private GameState gameState = GameState.MainMenu;
        private bool waitForLag = true;
        public static bool WaitForLag { get => Instance.waitForLag; set => Instance.waitForLag = value; }
        private Action OnSceneLoadComplete;
        
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
            EventManager.Broadcast(new OnPlayBGM("MainMenu", fadeIn:10f));
            waitForLag = true;
        }

        private void OnSceneLoaded()
        {
            OnSceneLoadComplete?.Invoke();
            OnSceneLoadComplete = null;
        }

        private void OnChangeScene(OnChangeScene evt)
        {
            StartCoroutine(ChangeScene(evt.Delay, evt.SceneName));
        }

        private void OnChangeGameState(OnChangeGameState evt)
        {
            switch (evt.GameState)
            {
                case GameState.MainMenu:
                    EventManager.Broadcast(new OnResetScore());
                    EventManager.Broadcast(new OnPlayBGM("MainMenu"));
                    break;
                case GameState.InGame:
                    StartCoroutine(ChangeScene(1f, "0-0"));
                    EventManager.Broadcast(new OnPlayBGM("Gameplay"));
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

        private IEnumerator ChangeScene(float delay, string sceneName)
        {
            yield return new WaitForSeconds(delay);
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => OnSceneLoaded();
        }
    }
}