using ProjectLightsOut.Managers;

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
            GoToMainMenu();
        }

        public async void GoToMainMenu()
        {
            if (State == AppState.MainMenu) return;
            State = AppState.Loading;
            await SceneLoader.SwitchToAsync("MainMenu");
            State = AppState.MainMenu;
        }

        public async void StartGameplay()
        {
            if (State == AppState.Gameplay) return;
            State = AppState.Loading;
            await SceneLoader.SwitchToAsync("0-0");
            GameManager.Instance.OnChangeGameState(new OnChangeGameState(GameState.InGame));
            State = AppState.Gameplay;
        }

        public async void GoToLevelSelect(string level)
        {
            if (State == AppState.Loading) return;
            State = AppState.Loading;
            await SceneLoader.SwitchToAsync(level);
            State = AppState.Gameplay;
        }
    }
}
