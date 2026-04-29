using System.Collections.Generic;
using UnityEngine;
using DamnedVeil.ProceduralLogic.Models;

namespace DamnedVeil.ProceduralLogic.PathGeneration
{
    public class SpecularPathGenerator : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [Tooltip("Layers to EXCLUDE from raycast (same as PlayerShoot). Walls/environment are hit automatically.")]
        [SerializeField] private LayerMask excludeLayers;
        [Tooltip("Tag that marks a surface as bounceable (must match Projectile collision tag)")]
        [SerializeField] private string bounceTag = "Ricochet";
        [SerializeField] private int maxBounces = 6;
        [SerializeField] private float maxRayDistance = 100f;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color pathColor = Color.cyan;

        private SpecularPathData lastGeneratedPath;

        public SpecularPathData GeneratePath(Vector2 origin, Vector2 direction, int maxBouncesOverride = -1)
        {
            SpecularPathData pathData = new SpecularPathData();

            // Add origin point
            pathData.PathPoints.Add(new TrajectoryPoint(origin, Vector2.zero, 0));

            Vector2 currentPosition = origin;
            Vector2 currentDirection = direction.normalized;
            float totalLength = 0f;
            int effectiveMaxBounces = maxBouncesOverride > 0 ? maxBouncesOverride : maxBounces;

            LayerMask raycastMask = ~excludeLayers;

            for (int bounce = 0; bounce < effectiveMaxBounces; bounce++)
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    currentPosition,
                    currentDirection,
                    maxRayDistance,
                    raycastMask
                );

                if (hit.collider != null)
                {
                    float segmentLength = Vector2.Distance(currentPosition, hit.point);
                    totalLength += segmentLength;

                    pathData.PathPoints.Add(new TrajectoryPoint(
                        hit.point,
                        hit.normal,
                        bounce + 1
                    ));

                    if (hit.collider.CompareTag(bounceTag))
                    {
                        currentDirection = Vector2.Reflect(currentDirection, hit.normal);
                        currentPosition = hit.point + (currentDirection * 0.01f);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
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

        public SpecularPathData GeneratePathAtAngle(Vector2 origin, float angleDegrees, int maxBouncesOverride = -1)
        {
            Vector2 direction = new Vector2(
                Mathf.Cos(angleDegrees * Mathf.Deg2Rad),
                Mathf.Sin(angleDegrees * Mathf.Deg2Rad)
            );
            return GeneratePath(origin, direction, maxBouncesOverride);
        }

        public int MaxBounces => maxBounces;

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
