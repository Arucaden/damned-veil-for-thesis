using System.Collections;
using ProjectLightsOut.Managers;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectLightsOut.DevUtils
{
    public enum AppState
    {
        Boot,
        MainMenu,
        Loading,
        Gameplay
    }
    public class AppStateManager : Singleton<AppStateManager>
    {
        public AppState State { get; private set; } = AppState.Boot;

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
#if UNITY_EDITOR
            {
                StartCoroutine(ShowSplashScreen());
            }
            #else
            {
                GoToMainMenu();
            }
            #endif
        }

        private IEnumerator ShowSplashScreen()
        {
            SplashScreen.Begin();
            while (!SplashScreen.isFinished)
            {
                SplashScreen.Draw();
                yield return null;
            }
            GoToMainMenu();
        }

        public async void GoToMainMenu()
        {
            try
            {
                if (State == AppState.MainMenu) return;
                
                State = AppState.Loading;
                await SceneLoader.SwitchToAsync("MainMenu");
                
                EventManager.Broadcast(new OnChangeGameState(GameState.GameOver));
                EventManager.Broadcast(new OnPlayBGM("MainMenu"));
                
                State = AppState.MainMenu;
            }
            catch (System.Exception e) { Debug.LogException(e); }
        }

        public async void StartGameplay()
        {
            try
            {
                if (State == AppState.Gameplay) return;
                
                State = AppState.Loading;
                await SceneLoader.SwitchToAsync("0-0");
                
                EventManager.Broadcast(new OnChangeGameState(GameState.Playing));
                
                State = AppState.Gameplay;
            }
            catch (System.Exception e) { Debug.LogException(e); }
        }

        public async void GoToLevelSelect(string level)
        {
            try
            {
                if (State == AppState.Loading) return;
                
                State = AppState.Loading;
                await SceneLoader.SwitchToAsync(level);
                
                if (State != AppState.Gameplay)
                {
                    EventManager.Broadcast(new OnChangeGameState(GameState.Playing));
                }
                
                State = AppState.Gameplay;
            }
            catch (System.Exception e) { Debug.LogException(e); }
        }

        public async void RestartGameplay(string level)
        {
            try
            {
                if (State == AppState.Gameplay)
                {
                    State = AppState.Loading;
                    await SceneLoader.SwitchToAsync(level);
                    EventManager.Broadcast(new OnChangeGameState(GameState.Playing));
                    State = AppState.Gameplay;
                }
            }
            catch (System.Exception e) { Debug.LogException(e); }
        }
    }
}
