using ProjectLightsOut.DevUtils;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    public class ScoreManager : Singleton<ScoreManager>
    {
        private int score = 0;
        public static int Score { get => Instance.score; private set { Instance.score = value; EventManager.Broadcast(new OnScoreChange(Instance.score)); } }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            EventManager.AddListener<OnAddScore>(AddScore);
            EventManager.AddListener<OnLevelComplete>(OnLevelComplete);
            EventManager.AddListener<OnResetScore>(ResetScore);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnAddScore>(AddScore);
            EventManager.RemoveListener<OnLevelComplete>(OnLevelComplete);
            EventManager.RemoveListener<OnResetScore>(ResetScore);
        }

        private void ResetScore(OnResetScore evt)
        {
            score = 0;
        }

        private void AddScore(OnAddScore evt)
        {
            Score += evt.Score;
        }

        private void OnLevelComplete(OnLevelComplete evt)
        {
            int timeBonus = (int)(evt.LevelTimeRemaining * 100);
            if (timeBonus < 0) timeBonus = 0;
            int bulletBonus = evt.BulletRemaining * 1000;
            EventManager.Broadcast(new OnPostScore(Score, timeBonus, evt.LevelBonus, bulletBonus));
            Score += timeBonus + evt.LevelBonus + bulletBonus;
        }
    }
}
