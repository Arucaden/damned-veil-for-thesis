using System.Collections.Generic;
using SDebug = System.Diagnostics;
using UnityEngine;
using DamnedVeil.ProceduralLogic.Models;
using DamnedVeil.ProceduralLogic.PathGeneration;
using DamnedVeil.ProceduralLogic.CSP;
using ProjectLightsOut.Managers;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Gameplay;

namespace DamnedVeil.ProceduralLogic.Orchestrator
{
    public class ProceduralEnemySpawner : Singleton<ProceduralEnemySpawner>
    {
        [Header("References")]
        [SerializeField] private SpecularPathGenerator pathGenerator;
        [SerializeField] private CSPValidator cspValidator;

        [Header("Spawning Settings")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private int maxAttempts = 100;
        [SerializeField] private float minPathLength = 5f;
        [SerializeField] private Projectile bulletPrefab;

        [Header("Visualization")]
        [SerializeField] private LineRenderer pathLineRenderer;
        [SerializeField] private bool showPath = true;
        [SerializeField] private Color pathColor = Color.cyan;

        [Header("Debug")]
        [SerializeField] private bool logDebugInfo = true;

        private List<GameObject> spawnedEnemies = new List<GameObject>();
        private SpecularPathData currentPath;
        private bool hasSpawned = false;
        private ProceduralWaveSettings lastSettings;

        // Debug data
        private int lastAttemptCount = 0;
        private double generationTimeMs = 0;
        private double spTimeMs = 0;
        private double cspTimeMs = 0;

        public bool SpawnWave(ProceduralWaveSettings settings)
        {
            if (playerTransform == null)
            {
                Debug.LogError("[ProceduralEnemySpawner] Player transform not assigned!");
                return false;
            }

            float effectiveMinPathLength = settings.MinPathLength > 0 ? settings.MinPathLength : minPathLength;
            int effectiveMaxBounces = settings.MaxBounces > 0 ? settings.MaxBounces : -1;

            if (bulletPrefab != null && effectiveMaxBounces > 0)
            {
                effectiveMaxBounces = Mathf.Min(effectiveMaxBounces, bulletPrefab.MaxRicochetCount);
            }
            lastSettings = settings;
            
            ClearSpawnedEnemies();

            Vector2 playerPosition = playerTransform.position;
            int attempts = 0;

            var totalWatch = SDebug.Stopwatch.StartNew();
            double accSpTime = 0;
            double accCspTime = 0;

            while (attempts < maxAttempts)
            {
                attempts++;

                float angle = UnityEngine.Random.Range(0f, 360f);

                var spWatch = SDebug.Stopwatch.StartNew();
                SpecularPathData path = pathGenerator.GeneratePathAtAngle(playerPosition, angle, effectiveMaxBounces);
                spWatch.Stop();
                accSpTime += spWatch.Elapsed.TotalMilliseconds;

                if (path.TotalLength < effectiveMinPathLength)
                {
                    continue;
                }

                var cspWatch = SDebug.Stopwatch.StartNew();
                List<EnemySpawnData> enemyPositions = cspValidator.Solve(
                    path,
                    playerPosition,
                    settings.EnemyCount,
                    settings.SafeZoneRadius,
                    settings.MinEnemySpacing,
                    settings.EndPathBuffer,
                    settings.WallBufferRadius,
                    settings.MaxEnemiesPerSegment
                );
                cspWatch.Stop();
                accCspTime += cspWatch.Elapsed.TotalMilliseconds;

                if (enemyPositions != null && enemyPositions.Count >= settings.EnemyCount)
                {
                    SpawnEnemiesAtPositions(enemyPositions, settings.EnemyRatios);
                    currentPath = path;

                    if (showPath && pathLineRenderer != null)
                    {
                        DrawPath(path);
                    }

                    totalWatch.Stop();
                    lastAttemptCount = attempts;
                    generationTimeMs = totalWatch.Elapsed.TotalMilliseconds;
                    spTimeMs = accSpTime;
                    cspTimeMs = accCspTime;

                    if (logDebugInfo)
                        UnityEngine.Debug.Log($"[ProceduralEnemySpawner] Success after {attempts} attempts! Spawned {enemyPositions.Count} enemies. Total: {generationTimeMs:F2}ms SP: {spTimeMs:F2}ms CSP: {cspTimeMs:F2}ms");

                    hasSpawned = true;
                    return true;
                }
            }

            totalWatch.Stop();
            lastAttemptCount = attempts;
            generationTimeMs = totalWatch.Elapsed.TotalMilliseconds;
            spTimeMs = accSpTime;
            cspTimeMs = accCspTime;

            UnityEngine.Debug.LogWarning($"[ProceduralEnemySpawner] Failed to generate valid level after {maxAttempts} attempts!");
            return false;
        }

        private void SpawnEnemiesAtPositions(List<EnemySpawnData> positions, List<ProceduralEnemyRatio> enemyRatios)
        {
            if (enemyRatios == null || enemyRatios.Count == 0)
            {
                Debug.LogError("[ProceduralEnemySpawner] EnemyRatios list is empty! Cannot spawn enemies.");
                return;
            }

            int totalWeight = 0;
            foreach (var ratio in enemyRatios)
            {
                totalWeight += ratio.Ratio;
            }

            List<GameObject> spawnPool = new List<GameObject>();
            float[] exactCounts = new float[enemyRatios.Count];
            int[] intCounts = new int[enemyRatios.Count];
            int totalSpawned = 0;

            for (int i = 0; i < enemyRatios.Count; i++)
            {
                exactCounts[i] = ((float)enemyRatios[i].Ratio / totalWeight) * positions.Count;
                intCounts[i] = Mathf.FloorToInt(exactCounts[i]);
                totalSpawned += intCounts[i];
            }

            int remaining = positions.Count - totalSpawned;
            for (int r = 0; r < remaining; r++)
            {
                float maxFraction = -1f;
                int maxIndex = 0;
                for (int i = 0; i < enemyRatios.Count; i++)
                {
                    float fraction = exactCounts[i] - intCounts[i];
                    if (fraction > maxFraction)
                    {
                        maxFraction = fraction;
                        maxIndex = i;
                    }
                }
                intCounts[maxIndex]++;
                exactCounts[maxIndex] -= 1f;
            }

            for (int i = 0; i < enemyRatios.Count; i++)
            {
                for (int j = 0; j < intCounts[i]; j++)
                {
                    spawnPool.Add(enemyRatios[i].EnemyPrefab);
                }
            }

            for (int i = 0; i < spawnPool.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, spawnPool.Count);
                GameObject temp = spawnPool[i];
                spawnPool[i] = spawnPool[randomIndex];
                spawnPool[randomIndex] = temp;
            }

            for (int i = 0; i < positions.Count; i++)
            {
                var spawnData = positions[i];
                GameObject prefab = spawnPool[i];
                
                GameObject enemyObj = Instantiate(
                    prefab,
                    spawnData.Position,
                    Quaternion.identity,
                    transform
                );
                enemyObj.name = $"Enemy_Segment{spawnData.OnSegmentIndex}_{i}";
                spawnedEnemies.Add(enemyObj);

                // Trigger spawn animation (disables collider/sprite, plays VFX, re-enables after delay)
                Enemy enemyComponent = enemyObj.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    enemyComponent.Spawn();
                }
            }
        }

        public void ClearSpawnedEnemies()
        {
            foreach (var enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    if (Application.isPlaying)
                        Destroy(enemy);
                    else
                        DestroyImmediate(enemy);
                }
            }
            spawnedEnemies.Clear();
            hasSpawned = false;
        }

        private void DrawPath(SpecularPathData path)
        {
            if (pathLineRenderer == null || path == null)
                return;

            Vector3[] positions = new Vector3[path.PathPoints.Count];
            for (int i = 0; i < path.PathPoints.Count; i++)
            {
                positions[i] = new Vector3(
                    path.PathPoints[i].Position.x,
                    path.PathPoints[i].Position.y,
                    0f
                );
            }

            pathLineRenderer.positionCount = positions.Length;
            pathLineRenderer.SetPositions(positions);
            pathLineRenderer.startColor = pathColor;
            pathLineRenderer.endColor = pathColor;
            pathLineRenderer.enabled = true;
        }

        public void HidePath()
        {
            if (pathLineRenderer != null)
            {
                pathLineRenderer.enabled = false;
            }
        }

        public bool Respawn()
        {
            if (lastSettings.EnemyCount > 0)
                return SpawnWave(lastSettings);
            
            Debug.LogWarning("[ProceduralEnemySpawner] Cannot Respawn: No previous wave settings found.");
            return false;
        }

        public bool HasSpawned => hasSpawned;
        public int SpawnedEnemyCount => spawnedEnemies.Count;
        public SpecularPathData CurrentPath => currentPath;
        public List<GameObject> SpawnedEnemies => new List<GameObject>(spawnedEnemies);

        // Debug properties
        public int LastAttemptCount => lastAttemptCount;
        public int MaxAttempts => maxAttempts;
        public double GenerationTimeMs => generationTimeMs;
        public double SpTimeMs => spTimeMs;
        public double CspTimeMs => cspTimeMs;
        public ProceduralWaveSettings LastSettings => lastSettings;

        private void OnValidate()
        {
            if (pathGenerator == null)
                pathGenerator = GetComponent<SpecularPathGenerator>();
            if (cspValidator == null)
                cspValidator = GetComponent<CSPValidator>();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (playerTransform != null && lastSettings.SafeZoneRadius > 0f)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
                Gizmos.DrawWireSphere(playerTransform.position, lastSettings.SafeZoneRadius);
            }
        }
#endif
    }
}
