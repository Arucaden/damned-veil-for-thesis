using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    public class OnSlowTime : GameEvent
    {
        public float TimeScale;
        public float Duration;
        public OnSlowTime(float timeScale, float duration)
        {
            TimeScale = timeScale;
            Duration = duration;
        }

        public OnSlowTime()
        {
            TimeScale = 0.5f;
            Duration = 0.5f;
        }
    }

    public class OnChangeGameState : GameEvent
    {
        public GameState GameState;
        public OnChangeGameState(GameState gameState)
        {
            GameState = gameState;
        }
    }

    public class OnChangeScene : GameEvent
    {
        public string SceneName;
        public float Delay;

        public OnChangeScene(string sceneName, float delay = 1f)
        {
            SceneName = sceneName;
            Delay = delay;
        }
    }

    public class OnFadeBlack : GameEvent
    { }

    public class OnGameOver : GameEvent
    { }

    public class OnPause : GameEvent
    {
        public bool IsPaused;
        public OnPause(bool isPaused)
        {
            IsPaused = isPaused;
        }    
    }
}
