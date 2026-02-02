using System.Collections.Generic;
using UnityEngine;
using DamnedVeil.ProceduralLogic.Models;

namespace DamnedVeil.ProceduralLogic.PathGeneration
{
    /// <summary>
    /// Generates specular (ricochet) bullet paths using the Image Source Method.
    /// Simulates bullet trajectory bouncing off walls.
    /// </summary>
    public class SpecularPathGenerator : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private int maxBounces = 6;
        [SerializeField] private float maxRayDistance = 100f;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color pathColor = Color.cyan;

        private SpecularPathData lastGeneratedPath;

        /// <summary>
        /// Generates a specular path from the given origin in a specific direction.
        /// Uses Physics2D.Raycast to simulate bullet bounces off walls.
        /// </summary>
        /// <param name="origin">Starting position (typically player position)</param>
        /// <param name="direction">Initial bullet direction (normalized)</param>
        /// <returns>SpecularPathData containing all trajectory points</returns>
        public SpecularPathData GeneratePath(Vector2 origin, Vector2 direction)
        {
            SpecularPathData pathData = new SpecularPathData();

            // Add origin point
            pathData.PathPoints.Add(new TrajectoryPoint(origin, Vector2.zero, 0));

            Vector2 currentPosition = origin;
            Vector2 currentDirection = direction.normalized;
            float totalLength = 0f;

            for (int bounce = 0; bounce < maxBounces; bounce++)
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    currentPosition,
                    currentDirection,
                    maxRayDistance,
                    wallLayer
                );

                if (hit.collider != null)
                {
                    // Calculate segment length
                    float segmentLength = Vector2.Distance(currentPosition, hit.point);
                    totalLength += segmentLength;

                    // Add bounce point
                    pathData.PathPoints.Add(new TrajectoryPoint(
                        hit.point,
                        hit.normal,
                        bounce + 1
                    ));

                    // Calculate reflected direction
                    currentDirection = Vector2.Reflect(currentDirection, hit.normal);
                    currentPosition = hit.point + (currentDirection * 0.01f); // Small offset to avoid self-collision
                }
                else
                {
                    // Ray didn't hit anything, extend to max distance
                    Vector2 endPoint = currentPosition + currentDirection * maxRayDistance;
                    totalLength += maxRayDistance;

                    pathData.PathPoints.Add(new TrajectoryPoint(
                        endPoint,
                        Vector2.zero,
                        bounce + 1
                    ));
                    break;
                }
            }

            pathData.TotalLength = totalLength;
            lastGeneratedPath = pathData;

            return pathData;
        }

        /// <summary>
        /// Generates a path in a random direction from the origin.
        /// </summary>
        public SpecularPathData GenerateRandomPath(Vector2 origin)
        {
            float randomAngle = Random.Range(0f, 360f);
            Vector2 direction = new Vector2(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                Mathf.Sin(randomAngle * Mathf.Deg2Rad)
            );
            return GeneratePath(origin, direction);
        }

        /// <summary>
        /// Generates a path at a specific angle (in degrees).
        /// </summary>
        public SpecularPathData GeneratePathAtAngle(Vector2 origin, float angleDegrees)
        {
            Vector2 direction = new Vector2(
                Mathf.Cos(angleDegrees * Mathf.Deg2Rad),
                Mathf.Sin(angleDegrees * Mathf.Deg2Rad)
            );
            return GeneratePath(origin, direction);
        }

        public int MaxBounces => maxBounces;
        public LayerMask WallLayer => wallLayer;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || lastGeneratedPath == null || lastGeneratedPath.PathPoints.Count < 2)
                return;

            Gizmos.color = pathColor;
            for (int i = 0; i < lastGeneratedPath.PathPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(
                    lastGeneratedPath.PathPoints[i].Position,
                    lastGeneratedPath.PathPoints[i + 1].Position
                );
            }

            // Draw bounce points
            Gizmos.color = Color.yellow;
            foreach (var point in lastGeneratedPath.PathPoints)
            {
                if (point.BounceIndex > 0)
                {
                    Gizmos.DrawWireSphere(point.Position, 0.2f);
                }
            }
        }
#endif
    }
}
