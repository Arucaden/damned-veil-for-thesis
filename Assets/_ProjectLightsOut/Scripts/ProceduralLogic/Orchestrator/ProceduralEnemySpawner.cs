using System.Collections.Generic;
using UnityEngine;
using DamnedVeil.ProceduralLogic.Models;
using DamnedVeil.ProceduralLogic.PathGeneration;
using DamnedVeil.ProceduralLogic.CSP;
using ProjectLightsOut.Managers;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Gameplay;

namespace DamnedVeil.ProceduralLogic.Orchestrator
{
    /// <summary>
    /// Level Orchestrator (Module C) - Coordinates path generation and CSP validation
    /// to spawn enemies that can be eliminated with a single ricochet shot.
    /// </summary>
    public class ProceduralEnemySpawner : Singleton<ProceduralEnemySpawner>
    {
        [Header("References")]
        [SerializeField] private SpecularPathGenerator pathGenerator;
        [SerializeField] private CSPValidator cspValidator;

        [Header("Spawning Settings")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private int maxAttempts = 100;
        [SerializeField] private float minPathLength = 5f;

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

        /// <summary>
        /// Attempts to spawn enemies using the provided settings.
        /// </summary>
        /// <returns>True if successful, false if failed after max attempts</returns>
        public bool SpawnWave(ProceduralWaveSettings settings)
        {
            if (playerTransform == null)
            {
                Debug.LogError("[ProceduralEnemySpawner] Player transform not assigned!");
                return false;
            }

            // Use local overrides if provided, don't mutate inspector defaults
            float effectiveMinPathLength = settings.MinPathLength > 0 ? settings.MinPathLength : minPathLength;
            int effectiveMaxBounces = settings.MaxBounces > 0 ? settings.MaxBounces : -1;
            lastSettings = settings; // Cache for Respawn
            
            ClearSpawnedEnemies();

            Vector2 playerPosition = playerTransform.position;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                attempts++;

                // 1. Specular Path Phase - Generate random path
                float angle = UnityEngine.Random.Range(0f, 360f);
                SpecularPathData path = pathGenerator.GeneratePathAtAngle(playerPosition, angle, effectiveMaxBounces);

                // Validate path length
                if (path.TotalLength < effectiveMinPathLength)
                {
                    continue;
                }

                // 2. CSP Phase - Validate and get enemy positions
                List<EnemySpawnData> enemyPositions = cspValidator.Solve(
                    path,
                    playerPosition,
                    settings.EnemyCount,
                    settings.SafeZoneRadius,
                    settings.MinEnemySpacing,
                    settings.EndPathBuffer,
                    settings.WallBufferRadius
                );

                if (enemyPositions != null && enemyPositions.Count >= settings.EnemyCount)
                {
                    // 3. Spawning Phase - Instantiate enemies
                    SpawnEnemiesAtPositions(enemyPositions, settings.EnemyPool);
                    currentPath = path;

                    // Visualize the path - ONLY IN EDITOR or if debug enabled
                    if (showPath && pathLineRenderer != null)
                    {
                        DrawPath(path);
                    }

                    if (logDebugInfo)
                        Debug.Log($"[ProceduralEnemySpawner] Success after {attempts} attempts! Spawned {enemyPositions.Count} enemies.");

                    hasSpawned = true;
                    return true;
                }
            }

            Debug.LogWarning($"[ProceduralEnemySpawner] Failed to generate valid level after {maxAttempts} attempts!");
            return false;
        }

        /// <summary>
        /// Spawns enemy prefabs at the validated positions.
        /// </summary>
        private void SpawnEnemiesAtPositions(List<EnemySpawnData> positions, List<GameObject> enemyPool)
        {
            if (enemyPool == null || enemyPool.Count == 0)
            {
                Debug.LogError("[ProceduralEnemySpawner] EnemyPool is empty! Cannot spawn enemies.");
                return;
            }

            for (int i = 0; i < positions.Count; i++)
            {
                var spawnData = positions[i];
                GameObject prefab = enemyPool[UnityEngine.Random.Range(0, enemyPool.Count)];
                
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

        /// <summary>
        /// Clears all previously spawned enemies.
        /// </summary>
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

        /// <summary>
        /// Draws the current path using the LineRenderer.
        /// </summary>
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

        /// <summary>
        /// Hides the path visualization.
        /// </summary>
        public void HidePath()
        {
            if (pathLineRenderer != null)
            {
                pathLineRenderer.enabled = false;
            }
        }

        /// <summary>
        /// Re-spawns enemies with a new random configuration.
        /// </summary>
        public bool Respawn()
        {
            if (lastSettings.EnemyCount > 0)
                return SpawnWave(lastSettings);
            
            Debug.LogWarning("[ProceduralEnemySpawner] Cannot Respawn: No previous wave settings found.");
            return false;
        }

        // Public accessors
        public bool HasSpawned => hasSpawned;
        public int SpawnedEnemyCount => spawnedEnemies.Count;
        public SpecularPathData CurrentPath => currentPath;
        public List<GameObject> SpawnedEnemies => new List<GameObject>(spawnedEnemies);

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
            // Draw safe zone around player
            if (playerTransform != null && cspValidator != null)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
                Gizmos.DrawWireSphere(playerTransform.position, cspValidator.SafeZoneRadius);
            }
        }
#endif
    }
}
