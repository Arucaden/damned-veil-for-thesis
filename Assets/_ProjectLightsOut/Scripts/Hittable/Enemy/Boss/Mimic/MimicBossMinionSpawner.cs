using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class MimicBossMinionSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MimicBoss mimicBoss;
        [SerializeField] private GameObject blackoutEnemyPrefab;
        [SerializeField] private Transform[] minionSpawnPoints;

        [Header("Wave Settings")]
        [Tooltip("Delay before the first wave starts after boss becomes invulnerable.")]
        [SerializeField] private float initialSpawnDelay = 3f;
        [Tooltip("Time between subsequent waves.")]
        [SerializeField] private float waveInterval = 10f;
        [Tooltip("Total number of waves to spawn before stopping. Set to 0 for infinite.")]
        [SerializeField] private int maxWaves = 0;
        
        [Header("Enemy Count Settings")]
        [SerializeField] private int minEnemiesPerWave = 1;
        [SerializeField] private int maxEnemiesPerWave = 2;

        private Coroutine spawnCoroutine;
        private List<Enemy> activeMinions = new List<Enemy>();
        private bool wasVulnerable = true;
        private bool isFirstSpawn = true;

        private void Start()
        {
            if (mimicBoss == null) mimicBoss = GetComponentInParent<MimicBoss>();
            wasVulnerable = mimicBoss != null ? mimicBoss.IsVulnerable : true;
            
            if (!wasVulnerable)
            {
                StartSpawning();
            }
        }

        private void Update()
        {
            if (mimicBoss == null || mimicBoss.Health <= 0)
            {
                if (spawnCoroutine != null)
                {
                    StopSpawning();
                    ClearMinions();
                }
                return;
            }

            bool isCurrentlyVulnerable = mimicBoss.IsVulnerable;

            if (isCurrentlyVulnerable && !wasVulnerable)
            {
                StopSpawning();
                ClearMinions();
            }
            else if (!isCurrentlyVulnerable && wasVulnerable)
            {
                StartSpawning();
            }

            wasVulnerable = isCurrentlyVulnerable;
        }

        private void StartSpawning()
        {
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(SpawnRoutine(isFirstSpawn));
            isFirstSpawn = false;
        }

        private void StopSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }

        private void ClearMinions()
        {
            foreach (var minion in activeMinions)
            {
                if (minion != null && minion.gameObject != null)
                {
                    EventManager.Broadcast(new OnEnemyDead(minion));
                    Destroy(minion.gameObject);
                }
            }
            activeMinions.Clear();
        }

        private IEnumerator SpawnRoutine(bool useInitialDelay)
        {
            if (blackoutEnemyPrefab == null || minionSpawnPoints == null || minionSpawnPoints.Length == 0) yield break;

            if (useInitialDelay)
            {
                yield return new WaitForSeconds(initialSpawnDelay);
            }
            else
            {
                yield return new WaitForSeconds(waveInterval);
            }

            int currentWave = 0;

            while (maxWaves <= 0 || currentWave < maxWaves)
            {
                int spawnCount = UnityEngine.Random.Range(minEnemiesPerWave, maxEnemiesPerWave + 1);

                List<Transform> availablePoints = new List<Transform>(minionSpawnPoints);

                for (int i = 0; i < spawnCount && availablePoints.Count > 0; i++)
                {
                    int randIndex = UnityEngine.Random.Range(0, availablePoints.Count);
                    Transform pt = availablePoints[randIndex];
                    availablePoints.RemoveAt(randIndex);

                    GameObject minionObj = Instantiate(blackoutEnemyPrefab, pt.position, Quaternion.identity);
                    Enemy spawnedEnemy = minionObj.GetComponent<Enemy>();
                    
                    if (spawnedEnemy != null)
                    {
                        spawnedEnemy.Spawn();
                        activeMinions.Add(spawnedEnemy);
                    }
                }

                currentWave++;

                if (maxWaves > 0 && currentWave >= maxWaves) break;

                while (true)
                {
                    activeMinions.RemoveAll(m => m == null || m.Health <= 0);
                    if (activeMinions.Count == 0) break;
                    yield return null;
                }

                yield return new WaitForSeconds(waveInterval);
            }
        }
    }
}
