using System.Collections.Generic;
using UnityEngine;
using DamnedVeil.ProceduralLogic.Models;
using UnityEngine.Tilemaps;
namespace DamnedVeil.ProceduralLogic.CSP
{
    public class CSPValidator : MonoBehaviour
    {
        [Header("Sampling Settings")]
        [Tooltip("Tag used to identify bounceable wall colliders")]
        [SerializeField] private string wallTag = "Ricochet";

        [Header("Area Settings")]
        [Tooltip("Optional: Tilemap defining the allowed spawn area. Tiles must be painted where spawning is permitted.")]
        [SerializeField] private Tilemap spawnAreaTilemap;

        [Tooltip("Resolution for sampling points along the path (in world units)")]
        [SerializeField] private float samplingResolution = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color validPointColor = Color.green;
        [SerializeField] private Color invalidPointColor = Color.red;

        private List<Vector2> lastSampledPoints = new List<Vector2>();
        private List<Vector2> lastValidPoints = new List<Vector2>();

        public List<EnemySpawnData> Solve(
            SpecularPathData pathData,
            Vector2 playerPosition,
            int enemyCount,
            float safeZoneRadius,
            float minEnemySpacing,
            float endPathBuffer,
            float wallBufferRadius,
            int maxEnemiesPerSegment = 0)
        {

            if (pathData == null || pathData.PathPoints.Count < 2)
            {
                Debug.LogWarning("[CSPValidator] Invalid path data provided.");
                return null;
            }

            List<(Vector2 position, int segmentIndex)> sampledPoints = SamplePointsAlongPath(pathData);
            lastSampledPoints.Clear();
            foreach (var p in sampledPoints)
            {
                lastSampledPoints.Add(p.position);
            }

            List<(Vector2 position, int segmentIndex)> areaFiltered = new List<(Vector2, int)>();
            foreach (var point in sampledPoints)
            {
                if (spawnAreaTilemap != null)
                {
                    Vector3Int cellPos = spawnAreaTilemap.WorldToCell(point.position);
                    if (spawnAreaTilemap.HasTile(cellPos))
                    {
                        areaFiltered.Add(point);
                    }
                }
                else
                {
                    areaFiltered.Add(point);
                }
            }

            if (areaFiltered.Count < enemyCount)
            {
                Debug.LogWarning($"[CSPValidator] Not enough points after tilemap area filter: {areaFiltered.Count}/{enemyCount}");
                return null;
            }

            List<(Vector2 position, int segmentIndex)> safeZoneFiltered = new List<(Vector2, int)>();
            foreach (var point in areaFiltered)
            {
                float distanceToPlayer = Vector2.Distance(point.position, playerPosition);
                if (distanceToPlayer > safeZoneRadius)
                {
                    safeZoneFiltered.Add(point);
                }
            }

            if (safeZoneFiltered.Count < enemyCount)
            {
                Debug.LogWarning($"[CSPValidator] Not enough points after safe zone filter: {safeZoneFiltered.Count}/{enemyCount}");
                return null;
            }

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

            if (bufferedPoints.Count < enemyCount)
            {
                Debug.LogWarning($"[CSPValidator] Not enough points after end buffer filter: {bufferedPoints.Count}/{enemyCount}");
                return null;
            }

            List<(Vector2 position, int segmentIndex)> wallFiltered = new List<(Vector2, int)>();
            foreach (var point in bufferedPoints)
            {
                if (!IsNearWall(point.position, wallBufferRadius))
                {
                    wallFiltered.Add(point);
                }
            }

            if (wallFiltered.Count < enemyCount)
            {
                Debug.LogWarning($"[CSPValidator] Not enough points after wall buffer filter: {wallFiltered.Count}/{enemyCount}");
                return null;
            }

            List<EnemySpawnData> selectedEnemies = SelectSpacedPoints(wallFiltered, minEnemySpacing, enemyCount, maxEnemiesPerSegment);

            lastValidPoints.Clear();
            foreach (var enemy in selectedEnemies)
            {
                lastValidPoints.Add(enemy.Position);
            }

            if (selectedEnemies.Count < enemyCount)
            {
                Debug.LogWarning($"[CSPValidator] Could not place enough enemies: {selectedEnemies.Count}/{enemyCount}");
                return null;
            }

            Debug.Log($"[CSPValidator] Successfully placed {selectedEnemies.Count} enemies.");
            return selectedEnemies;
        }

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

        private List<EnemySpawnData> SelectSpacedPoints(List<(Vector2 position, int segmentIndex)> availablePoints, float spacing, int maxCount, int maxPerSegment)
        {
            List<EnemySpawnData> selected = new List<EnemySpawnData>();
            Dictionary<int, int> countsPerSegment = new Dictionary<int, int>();

            List<(Vector2 position, int segmentIndex)> shuffled = new List<(Vector2, int)>(availablePoints);
            ShuffleList(shuffled);

            foreach (var point in shuffled)
            {
                if (selected.Count >= maxCount)
                    break;

                if (maxPerSegment > 0)
                {
                    countsPerSegment.TryGetValue(point.segmentIndex, out int currentCount);
                    if (currentCount >= maxPerSegment) continue;
                }

                bool canPlace = true;
                foreach (var existing in selected)
                {
                    if (Vector2.Distance(point.position, existing.Position) < spacing)
                    {
                        canPlace = false;
                        break;
                    }
                }

                if (canPlace)
                {
                    selected.Add(new EnemySpawnData(point.position, point.segmentIndex));
                    
                    if (countsPerSegment.ContainsKey(point.segmentIndex))
                        countsPerSegment[point.segmentIndex]++;
                    else
                        countsPerSegment[point.segmentIndex] = 1;
                }
            }

            return selected;
        }

        private bool IsNearWall(Vector2 position, float bufferRadius)
        {
            if (bufferRadius <= 0f) return false;

            Collider2D[] hits = Physics2D.OverlapCircleAll(position, bufferRadius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag(wallTag))
                {
                    return true;
                }
            }
            return false;
        }

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



#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos)
                return;

            Gizmos.color = invalidPointColor;
            foreach (var point in lastSampledPoints)
            {
                Gizmos.DrawWireSphere(point, 0.1f);
            }

            Gizmos.color = validPointColor;
            foreach (var point in lastValidPoints)
            {
                Gizmos.DrawSphere(point, 0.2f);
            }
        }
#endif
    }
}
