using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Gameplay;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    public class LevelManager : Singleton<LevelManager>
    {
        [SerializeField] private LevelDataSO levelData;
        public static LevelDataSO LevelData => Instance.levelData;

        public string LevelName => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        private Transform bossTransform;
        public static Transform BossTransform => Instance.bossTransform;

        private bool isPlayerShootEnabled = false;
        public static bool IsPlayerShootEnabled
        {
            get => Instance.isPlayerShootEnabled;
            private set => Instance.isPlayerShootEnabled = value;
        }

        private void OnEnable()
        {
            EventManager.AddListener<OnBossRegister>(OnBossRegister);
            EventManager.AddListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnBossRegister>(OnBossRegister);
            EventManager.RemoveListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
        }

        private void OnBossRegister(OnBossRegister evt)
        {
            bossTransform = evt.Boss.transform;
        }

        private void OnPlayerEnableShooting(OnPlayerEnableShooting evt)
        {
            isPlayerShootEnabled = evt.IsEnabled;
        }

        public static void SpawnEnemyWave(WaveDataSO waveData)
        {
            var waveManager = Instance.transform.parent.GetComponentInChildren<WaveManager>();
            
            if (waveManager == null)
            {
                Debug.LogError("[LevelManager] Cannot spawn enemy wave because no WaveManager component exists in the scene! Please add a WaveManager to your LevelManager object.");
                return;
            }

            Instance.StartCoroutine(waveManager.SpawnWave(waveData));
        }
    }
}