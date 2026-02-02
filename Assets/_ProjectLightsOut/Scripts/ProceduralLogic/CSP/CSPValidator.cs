using System.Collections.Generic;
using UnityEngine;
using DamnedVeil.ProceduralLogic.Models;

namespace DamnedVeil.ProceduralLogic.CSP
{
    /// <summary>
    /// CSP (Constraint Satisfaction Problem) Validator for enemy spawn positions.
    /// Validates and selects enemy positions along the specular path while respecting constraints.
    /// </summary>
    public class CSPValidator : MonoBehaviour
    {
        [Header("Constraint Settings")]
        [Tooltip("Minimum distance from player (Safe Zone - C3)")]
        [SerializeField] private float safeZoneRadius = 3f;

        [Tooltip("Minimum distance between enemies (Spacing - C1)")]
        [SerializeField] private float minEnemySpacing = 2f;

        [Tooltip("Minimum number of enemies required (Min Count - C4)")]
        [SerializeField] private int minEnemyCount = 2;

        [Tooltip("Maximum number of enemies to spawn")]
        [SerializeField] private int maxEnemyCount = 5;

        [Header("Sampling Settings")]
        [Tooltip("Resolution for sampling points along the path (in world units)")]
        [SerializeField] private float samplingResolution = 0.5f;

        [Tooltip("Don't spawn enemies too close to path end")]
        [SerializeField] private float endPathBuffer = 1f;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color validPointColor = Color.green;
        [SerializeField] private Color invalidPointColor = Color.red;

        private List<Vector2> lastSampledPoints = new List<Vector2>();
        private List<Vector2> lastValidPoints = new List<Vector2>();

        /// <summary>
        /// Validates and selects enemy spawn positions from the given path.
        /// </summary>
        /// <param name="pathData">The specular path to place enemies on</param>
        /// <param name="playerPosition">Current player position for safe zone check</param>
        /// <returns>List of valid enemy spawn data, or null if constraints cannot be satisfied</returns>
        public List<EnemySpawnData> Solve(SpecularPathData pathData, Vector2 playerPosition)
        {
            if (pathData == null || pathData.PathPoints.Count < 2)
            {
                Debug.LogWarning("[CSPValidator] Invalid path data provided.");
                return null;
            }

            // Step 1: Sample points along the path
            List<(Vector2 position, int segmentIndex)> sampledPoints = SamplePointsAlongPath(pathData);
            lastSampledPoints.Clear();
            foreach (var p in sampledPoints)
            {
                lastSampledPoints.Add(p.position);
            }

            // Step 2: Filter by Safe Zone constraint (C3)
            List<(Vector2 position, int segmentIndex)> safeZoneFiltered = new List<(Vector2, int)>();
            foreach (var point in sampledPoints)
            {
                float distanceToPlayer = Vector2.Distance(point.position, playerPosition);
                if (distanceToPlayer > safeZoneRadius)
                {
                    safeZoneFiltered.Add(point);
                }
            }

            if (safeZoneFiltered.Count < minEnemyCount)
            {
                Debug.LogWarning($"[CSPValidator] Not enough points after safe zone filter: {safeZoneFiltered.Count}/{minEnemyCount}");
                return null;
            }

            // Step 3: Filter by end path buffer
            List<(Vector2 position, int segmentIndex)> bufferedPoints = new List<(Vector2, int)>();
            Vector2 pathEnd = pathData.PathPoints[pathData.PathPoints.Count - 1].Position;
            foreach (var point in safeZoneFiltered)
            {
                float distanceToEnd = Vector2.Distance(point.position, pathEnd);
                if (distanceToEnd > endPathBuffer)
                {
                    bufferedPoints.Add(point);
                }
            }

            if (bufferedPoints.Count < minEnemyCount)
            {
                Debug.LogWarning($"[CSPValidator] Not enough points after end buffer filter: {bufferedPoints.Count}/{minEnemyCount}");
                return null;
            }

            // Step 4: Select random points respecting spacing constraint (C1)
            List<EnemySpawnData> selectedEnemies = SelectSpacedPoints(bufferedPoints);

            lastValidPoints.Clear();
            foreach (var enemy in selectedEnemies)
            {
                lastValidPoints.Add(enemy.Position);
            }

            // Step 5: Validate minimum enemy count (C4)
            if (selectedEnemies.Count < minEnemyCount)
            {
                Debug.LogWarning($"[CSPValidator] Could not place enough enemies: {selectedEnemies.Count}/{minEnemyCount}");
                return null;
            }

            Debug.Log($"[CSPValidator] Successfully placed {selectedEnemies.Count} enemies.");
            return selectedEnemies;
        }

        /// <summary>
        /// Samples points along the path at the specified resolution.
        /// </summary>
        private List<(Vector2 position, int segmentIndex)> SamplePointsAlongPath(SpecularPathData pathData)
        {
            List<(Vector2, int)> sampledPoints = new List<(Vector2, int)>();

            for (int i = 0; i < pathData.SegmentCount; i++)
            {
                var (start, end) = pathData.GetSegment(i);
                float segmentLength = Vector2.Distance(start, end);
                int numSamples = Mathf.Max(1, Mathf.FloorToInt(segmentLength / samplingResolution));

                for (int j = 0; j <= numSamples; j++)
                {
                    float t = (float)j / numSamples;
                    Vector2 samplePoint = Vector2.Lerp(start, end, t);
                    sampledPoints.Add((samplePoint, i));
                }
            }

            return sampledPoints;
        }

        /// <summary>
        /// Selects random points from the pool while respecting spacing constraints.
        /// </summary>
        private List<EnemySpawnData> SelectSpacedPoints(List<(Vector2 position, int segmentIndex)> availablePoints)
        {
            List<EnemySpawnData> selected = new List<EnemySpawnData>();

            // Shuffle the available points for randomness
            List<(Vector2 position, int segmentIndex)> shuffled = new List<(Vector2, int)>(availablePoints);
            ShuffleList(shuffled);

            foreach (var point in shuffled)
            {
                if (selected.Count >= maxEnemyCount)
                    break;

                // Check spacing against already selected points
                bool canPlace = true;
                foreach (var existing in selected)
                {
                    if (Vector2.Distance(point.position, existing.Position) < minEnemySpacing)
                    {
                        canPlace = false;
                        break;
                    }
                }

                if (canPlace)
                {
                    selected.Add(new EnemySpawnData(point.position, point.segmentIndex));
                }
            }

            return selected;
        }

        /// <summary>
        /// Fisher-Yates shuffle for randomizing point selection.
        /// </summary>
        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        // Public properties for external configuration
        public float SafeZoneRadius { get => safeZoneRadius; set => safeZoneRadius = value; }
        public float MinEnemySpacing { get => minEnemySpacing; set => minEnemySpacing = value; }
        public int MinEnemyCount { get => minEnemyCount; set => minEnemyCount = value; }
        public int MaxEnemyCount { get => maxEnemyCount; set => maxEnemyCount = value; }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos)
                return;

            // Draw all sampled points
            Gizmos.color = invalidPointColor;
            foreach (var point in lastSampledPoints)
            {
                Gizmos.DrawWireSphere(point, 0.1f);
            }

            // Draw valid/selected points
            Gizmos.color = validPointColor;
            foreach (var point in lastValidPoints)
            {
                Gizmos.DrawSphere(point, 0.2f);
            }
        }
#endif
    }
}
