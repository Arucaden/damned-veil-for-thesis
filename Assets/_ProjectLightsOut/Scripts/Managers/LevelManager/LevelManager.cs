using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Gameplay;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    /// <summary>
    /// Slim data holder and static accessor for level configuration.
    /// Wave spawning, projectile tracking, and flow control are handled by
    /// sibling components: WaveManager, ProjectileTracker, LevelFlowController.
    /// </summary>
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

        /// <summary>
        /// Static convenience method for Boss to trigger wave spawning.
        /// Delegates to the WaveManager sibling component.
        /// </summary>
        public static void SpawnEnemyWave(WaveDataSO waveData)
        {
            var waveManager = Instance.GetComponent<WaveManager>();
            Instance.StartCoroutine(waveManager.SpawnWave(waveData));
        }
    }
}