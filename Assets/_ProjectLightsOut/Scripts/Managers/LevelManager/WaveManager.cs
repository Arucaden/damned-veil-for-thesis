using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Gameplay;
using UnityEngine;
using DamnedVeil.ProceduralLogic.Orchestrator;

namespace ProjectLightsOut.Managers
{
    /// <summary>
    /// Manages wave spawning, enemy registration, and wave completion detection.
    /// Extracted from LevelManager to follow Single Responsibility Principle.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        private List<Enemy> enemies = new List<Enemy>();
        public List<Enemy> Enemies => enemies;

        private List<Enemy> deadEnemies = new List<Enemy>();
        public List<Enemy> DeadEnemies => deadEnemies;

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

        /// <summary>
        /// Called by LevelFlowController after the intro sequence to kick off
        /// the first wave if all pre-placed enemies are already dead.
        /// </summary>
        public void TriggerInitialWaveCheck()
        {
            CheckAllEnemiesDead(null);
        }

        /// <summary>
        /// After all enemies in the current wave are dead, spawn the next wave
        /// or trigger level completion.
        /// </summary>

        public bool AllWavesDefeated { get; private set; }

        public delegate bool WaveLoopCondition();
        
        /// <summary>
        /// External riddles can hook into this. If this returns true, the current wave
        /// will infinitely loop instead of completing combat.
        /// </summary>
        public WaveLoopCondition ShouldLoopWaves;

        private void CheckAllEnemiesDead(Enemy enemyDead)
        {
            if (LevelManager.LevelData.IsBossLevel) return;
            if (AllWavesDefeated) return;

            if (enemies.Count == 0)
            {
                if (LevelManager.LevelData.Waves.Count > currentWave)
                {
                    StartCoroutine(SpawnWave(LevelManager.LevelData.Waves[currentWave]));
                    currentWave++;
                }
                else
                {
                    // Check if an external system (like PillarsRiddle) is forcing a loop!
                    if (ShouldLoopWaves != null && ShouldLoopWaves.Invoke())
                    {
                        // Spawn the LAST wave over again!
                        int lastWaveIndex = currentWave - 1;
                        if (lastWaveIndex >= 0)
                        {
                            StartCoroutine(SpawnWave(LevelManager.LevelData.Waves[lastWaveIndex]));
                            return; // Stop here, do not finish combat!
                        }
                    }

                    AllWavesDefeated = true;
                    EventManager.Broadcast(new OnCombatWavesCompleted(enemyDead));
                }
            }
        }

        /// <summary>
        /// Spawns a wave of enemies from a WaveDataSO, supporting both
        /// manual and procedural wave types.
        /// </summary>
        public IEnumerator SpawnWave(WaveDataSO waveData)
        {
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
                yield break;
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
