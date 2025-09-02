using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Gameplay;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    public class LevelManager : Singleton<LevelManager>
    {
        [SerializeField] private LevelDataSO levelData;
        public static LevelDataSO LevelData { get => Instance.levelData; }
        [SerializeField] private List<Transform> startWaypoints = new List<Transform>();
        [SerializeField] private List<Transform> endWaypoints = new List<Transform>();
        [SerializeField] private bool instantlyZoomAtStart = false;
        [SerializeField] private float zoomLevel = 0.3f;
        private List<Enemy> enemies = new List<Enemy>();
        public List<Enemy> Enemies { get => enemies; }
        private List<Enemy> deadEnemies = new List<Enemy>();
        public List<Enemy> DeadEnemies { get => deadEnemies; }
        private Transform bossTransform;
        public static Transform BossTransform { get => Instance.bossTransform; }
        private int activeProjectiles = 0;
        private bool isLevelComplete = false;
        private int currentWave = 0;
        private float timeElapsed = 0f;
        private int bulletRemaining = 0;
        private bool isPlayerShootEnabled = false;
        private bool isGameOver = false;
        public static bool IsPlayerShootEnabled { get => Instance.isPlayerShootEnabled; private set => Instance.isPlayerShootEnabled = value; }

        private void OnEnable()
        {
            if (levelData == null)
            {
                Debug.LogError("LevelManager: LevelDataSO is not set.");
            }
            
            EventManager.AddListener<OnEnemyRegister>(OnEnemyRegister);
            EventManager.AddListener<OnEnemyDead>(OnEnemyDead);
            EventManager.AddListener<OnProjectileShoot>(OnProjectileShoot);
            EventManager.AddListener<OnProjectileDestroy>(OnProjectileDestroy);
            EventManager.AddListener<OnPlayerFinishMove>(OnPlayerFinishMove);
            EventManager.AddListener<OnCompleteCountingScore>(OnCompleteCountingScore);
            EventManager.AddListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
            EventManager.AddListener<OnTriggerGameOver>(TriggerGameOver);
            EventManager.AddListener<OnTriggerLevelComplete>(OnTriggerLevelComplete);
            EventManager.AddListener<OnBossRegister>(OnBossRegister);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnEnemyRegister>(OnEnemyRegister);
            EventManager.RemoveListener<OnEnemyDead>(OnEnemyDead);
            EventManager.RemoveListener<OnProjectileShoot>(OnProjectileShoot);
            EventManager.RemoveListener<OnProjectileDestroy>(OnProjectileDestroy);
            EventManager.RemoveListener<OnPlayerFinishMove>(OnPlayerFinishMove);
            EventManager.RemoveListener<OnCompleteCountingScore>(OnCompleteCountingScore);
            EventManager.RemoveListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
            EventManager.RemoveListener<OnTriggerGameOver>(TriggerGameOver);
            EventManager.RemoveListener<OnTriggerLevelComplete>(OnTriggerLevelComplete);
            EventManager.RemoveListener<OnBossRegister>(OnBossRegister);
        }

        private void OnBossRegister(OnBossRegister evt)
        {
            bossTransform = evt.Boss.transform;
        }

        private void OnPlayerEnableShooting(OnPlayerEnableShooting evt)
        {
            isPlayerShootEnabled = evt.IsEnabled;
        }

        private void Update()
        {
            timeElapsed += Time.deltaTime;
        }

        private void Start()
        {
            Cursor.visible = false;
            
            if (!AudioManager.IsBGMPlaying)
            {
                EventManager.Broadcast(new OnPlayBGM("Gameplay", fadeIn: 1f));
            }

            if (levelData.IsBossLevel)
            {
                EventManager.Broadcast(new OnPlayBGM("Boss", fadeIn: 0f));
            }

            if (startWaypoints.Count > 0)
            {
                StartCoroutine(StartLevel());
            }

            else
            {
                OnPlayerFinishMove(new OnPlayerFinishMove());
            }
        }

        private IEnumerator StartLevel()
        {
            EventManager.Broadcast(new OnPlayerEnableShooting(false));

            // if (GameManager.WaitForLag)
            // {
            //     yield return new WaitForSeconds(2f);
            //     GameManager.WaitForLag = false;
            // }

            yield return new WaitForSeconds(1f);

            if (instantlyZoomAtStart) 
            {
                EventManager.Broadcast(new OnZoom(zoomLevel, 0f));
                EventManager.Broadcast(new OnSpotting(startWaypoints[startWaypoints.Count-1], 0f));
            }

            else 
            {
                EventManager.Broadcast(new OnZoom(zoomLevel, 1.7f));
                EventManager.Broadcast(new OnSpotting(startWaypoints[startWaypoints.Count-1], 1.7f));
            }

            EventManager.Broadcast(new OnPlayerMove(true, startWaypoints));
        }

        private void OnPlayerFinishMove(OnPlayerFinishMove evt)
        {
            if (!isLevelComplete)
            {
                StartCoroutine(FinishStartMove());
            }
        }

        private void OnCompleteCountingScore(OnCompleteCountingScore evt)
        {
            string nextLevel = levelData.NextLevelScenes[Random.Range(0, levelData.NextLevelScenes.Count)];
            EventManager.Broadcast(new OnFadeBlack());
            EventManager.Broadcast(new OnChangeScene(nextLevel, 2f));
        }

        private IEnumerator FinishStartMove()
        {

            if (levelData.IsBossLevel)
            {
                EventManager.Broadcast(new OnReadyBoss());
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
            EventManager.Broadcast(new OnSpottingEnd());
            EventManager.Broadcast(new OnZoomEnd(1f));
            yield return new WaitForSeconds(1.5f);

            EventManager.Broadcast(new OnPlayerEnableShooting(true));

            CheckAllEnemiesDead(null);
        }

        private void OnEnemyRegister(OnEnemyRegister evt)
        {
            enemies.Add(evt.Enemy);
            //Debug.Log($"LevelManager: Enemy registered. Total enemies remaining: {enemies.Count}");
        }

        private void OnEnemyDead(OnEnemyDead evt)
        {
            enemies.Remove(evt.Enemy);
            deadEnemies.Add(evt.Enemy);
            
            //Debug.Log($"LevelManager: Enemy dead. Total enemies remaining: {enemies.Count}");
            CheckAllEnemiesDead(evt.Enemy);
        }

        private void OnProjectileShoot(OnProjectileShoot evt)
        {
            activeProjectiles++;
            bulletRemaining = evt.BulletLeft;
            //Debug.Log($"LevelManager: Projectile shot. Total active projectiles: {activeProjectiles}");
        }

        private void OnProjectileDestroy(OnProjectileDestroy evt)
        {
            activeProjectiles--;
            //Debug.Log($"LevelManager: Projectile destroyed. Total active projectiles: {activeProjectiles}");

            if (activeProjectiles == 0 && isLevelComplete)
            {
                StartCoroutine(LevelComplete());
            }

            if (activeProjectiles == 0 && enemies.Count > 0 && bulletRemaining <= 0)
            {
                if (levelData.IsBossLevel)
                {

                }

                else
                {
                    StartCoroutine(GameOver());
                }
            }
        }

        private void TriggerGameOver(OnTriggerGameOver evt)
        {
            StartCoroutine(GameOver());
        }

        private void OnTriggerLevelComplete(OnTriggerLevelComplete evt)
        {
            isLevelComplete = true;
        }

        private IEnumerator GameOver()
        {
            isGameOver = true;
            EventManager.Broadcast(new OnPlayBGM("GameOver", fadeIn: 1f));
            yield return new WaitForSeconds(1f);
            EventManager.Broadcast(new OnPlayerEnableShooting(false));
            yield return new WaitForSeconds(1f);
            EventManager.Broadcast(new OnGameOver());
        }

        private IEnumerator LevelComplete()
        {
            if (isGameOver) yield break;
            yield return new WaitForSeconds(1f);

            EventManager.Broadcast(new OnPlayerEnableShooting(false));

            yield return new WaitForSeconds(2f);

            EventManager.Broadcast(new OnPlayerMove(true, endWaypoints));
            EventManager.Broadcast(new OnLevelComplete(levelData.LevelScore, bulletRemaining, levelData.AceTime- timeElapsed));
        }

        private void CheckAllEnemiesDead(Enemy enemyDead)
        {
            if (levelData.IsBossLevel) return;
            
            if (enemies.Count == 0)
            {
                if (levelData.Waves.Count > currentWave)
                {
                    StartCoroutine(SpawnWave(levelData.Waves[currentWave]));
                    currentWave++;
                }

                else
                {
                    EventManager.Broadcast(new OnPlaySFX("Bell"));
                    StartCoroutine(LastEnemyZoom(enemyDead));
                    EventManager.Broadcast(new OnSlowTime(0.1f, 1.2f));
                    isLevelComplete = true;

                    if (activeProjectiles == 0)
                    {
                        StartCoroutine(LevelComplete());
                    }
                }
            }
        }

        private IEnumerator LastEnemyZoom(Enemy lastEnemy)
        {
            EventManager.Broadcast(new OnSpotting(lastEnemy.transform, 0.2f));
            EventManager.Broadcast(new OnZoom(-0.5f, 0.2f));

            yield return new WaitForSecondsRealtime(1.2f);

            EventManager.Broadcast(new OnSpottingEnd(0.4f));
            EventManager.Broadcast(new OnZoomEnd(0.4f));
        }

        private IEnumerator SpawnWave(WaveDataSO waveData)
        {
            foreach (var enemyData in waveData.Enemies)
            {
                yield return new WaitForSeconds(enemyData.SpawnDelay);
                if (isLevelComplete) yield break;
                Enemy enemy = Instantiate(enemyData.EnemyPrefab, enemyData.SpawnPosition, Quaternion.identity).GetComponent<Enemy>();
                enemy.Spawn();
                enemy.WaveData = waveData;
            }
        }

        public static void SpawnEnemyWave(WaveDataSO waveData)
        {
            Instance.StartCoroutine(Instance.SpawnWave(waveData));
        }
    }
}