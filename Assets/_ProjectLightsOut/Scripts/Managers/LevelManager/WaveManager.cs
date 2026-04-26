using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Gameplay;
using UnityEngine;
using DamnedVeil.ProceduralLogic.Orchestrator;

namespace ProjectLightsOut.Managers
{
    public class WaveManager : MonoBehaviour
    {
        private List<Enemy> enemies = new List<Enemy>();
        public List<Enemy> Enemies => enemies;

        private List<Enemy> deadEnemies = new List<Enemy>();
        public List<Enemy> DeadEnemies => deadEnemies;

        [SerializeField] private float waveTransitionDelay = 1.5f;

        private int currentWave = 0;

        private void OnEnable()
        {
            EventManager.AddListener<OnEnemyRegister>(OnEnemyRegister);
            EventManager.AddListener<OnEnemyDead>(OnEnemyDead);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnEnemyRegister>(OnEnemyRegister);
            EventManager.RemoveListener<OnEnemyDead>(OnEnemyDead);
        }

        private void OnEnemyRegister(OnEnemyRegister evt)
        {
            enemies.Add(evt.Enemy);
        }

        private void OnEnemyDead(OnEnemyDead evt)
        {
            enemies.Remove(evt.Enemy);
            deadEnemies.Add(evt.Enemy);

            CheckAllEnemiesDead(evt.Enemy);
        }

        public void TriggerInitialWaveCheck()
        {
            CheckAllEnemiesDead(null);
        }


        public bool AllWavesDefeated { get; private set; }

        public delegate bool WaveLoopCondition();
        
        public WaveLoopCondition ShouldLoopWaves;

        private void CheckAllEnemiesDead(Enemy enemyDead)
        {
            if (LevelManager.LevelData.IsBossLevel) return;
            if (AllWavesDefeated) return;

            if (enemies.Count == 0)
            {
                float delay = currentWave == 0 ? 0f : waveTransitionDelay;

                if (LevelManager.LevelData.Waves.Count > currentWave)
                {
                    StartCoroutine(SpawnWave(LevelManager.LevelData.Waves[currentWave], delay));
                    currentWave++;
                }
                else
                {
                    if (ShouldLoopWaves != null && ShouldLoopWaves.Invoke())
                    {
                        int lastWaveIndex = currentWave - 1;
                        if (lastWaveIndex >= 0)
                        {
                            StartCoroutine(SpawnWave(LevelManager.LevelData.Waves[lastWaveIndex], delay));
                            return;
                        }
                    }

                    AllWavesDefeated = true;
                    EventManager.Broadcast(new OnCombatWavesCompleted(enemyDead));
                }
            }
        }

        public IEnumerator SpawnWave(WaveDataSO waveData, float initialDelay = 0f)
        {
            if (initialDelay > 0f)
            {
                yield return new WaitForSeconds(initialDelay);
            }

            if (waveData.IsProcedural)
            {
                if (ProceduralEnemySpawner.Instance != null)
                {
                    bool success = ProceduralEnemySpawner.Instance.SpawnWave(waveData.ProceduralSettings);
                    if (!success)
                    {
                        Debug.LogError($"[WaveManager] Failed to spawn procedural wave for {waveData.name}");
                    }
                }
                else
                {
                    Debug.LogError("[WaveManager] ProceduralEnemySpawner Instance is null! Cannot spawn procedural wave.");
                }
            }

            foreach (var enemyData in waveData.Enemies)
            {
                yield return new WaitForSeconds(enemyData.SpawnDelay);
                Enemy enemy = Instantiate(enemyData.EnemyPrefab, enemyData.SpawnPosition, Quaternion.identity).GetComponent<Enemy>();
                enemy.Spawn();
                enemy.WaveData = waveData;
            }
        }
    }
}
