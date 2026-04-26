using System.Collections.Generic;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    [RequireComponent(typeof(LineRenderer))]
    public class PlayerLaserAimer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerShoot playerShoot;
        [Tooltip("Optional: Drop a child GameObject here to determine exactly where the laser starts drawing.")]
        [SerializeField] private Transform laserSpawnPoint;
        
        [Header("Visual Settings")]
        [SerializeField] private Gradient normalGradient;
        [SerializeField] private Gradient reloadGradient;
        [SerializeField] private LayerMask collisionLayerMask;
        [Tooltip("Max total distance the laser can travel across all bounces")]
        [SerializeField] private float maxLaserDistance = 100f;

        private LineRenderer lineRenderer;
        private CircleCollider2D bulletCollider;
        private float bulletRadiusInWorldSpace;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            
            if (playerShoot == null)
                playerShoot = GetComponentInParent<PlayerShoot>();

            if (playerShoot != null && playerShoot.BulletPrefab != null)
            {
                bulletCollider = playerShoot.BulletPrefab.GetComponent<CircleCollider2D>();
                bulletRadiusInWorldSpace = bulletCollider.radius * Mathf.Max(playerShoot.BulletPrefab.transform.lossyScale.x, playerShoot.BulletPrefab.transform.lossyScale.y);
            }
        }

        private void Update()
        {
            if (playerShoot == null) return;

            AimAndRotate();
            DrawDynamicLaser();
        }

        public Vector2 Direction { get; private set; }

        private void AimAndRotate()
        {
            Vector2 direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - playerShoot.transform.position;
            Direction = direction;
            
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
            playerShoot.transform.rotation = targetRotation;
            
            if (transform != playerShoot.transform) 
            {
                transform.rotation = targetRotation;
            }
        }

        private void DrawDynamicLaser()
        {
            // State checks
            if (!playerShoot.IsFiringEnabled)
            {
                lineRenderer.enabled = false;
                return;
            }

            lineRenderer.enabled = true;
            lineRenderer.colorGradient = playerShoot.Reloading ? reloadGradient : normalGradient;

            List<Vector3> points = new List<Vector3>();
            Vector2 startPosition = laserSpawnPoint != null ? (Vector2)laserSpawnPoint.position : (Vector2)transform.position;
            points.Add(startPosition);

            Vector2 currentPosition = startPosition;
            Vector2 currentDirection = transform.up; 
            int remainingBounces = playerShoot.Ricochets;
            float distanceRemaining = maxLaserDistance;

            LayerMask effectiveMask = collisionLayerMask;
            // Fallback if inspector isn't set, match original logic:
            if (effectiveMask.value == 0)
            {
                effectiveMask = ~(1 << LayerMask.NameToLayer("Ignore Laser") | 1 << LayerMask.NameToLayer("Projectile"));
            }

            for (int i = 0; i <= remainingBounces; i++)
            {
                RaycastHit2D hit = Physics2D.CircleCast(currentPosition, bulletRadiusInWorldSpace, currentDirection, distanceRemaining, effectiveMask);

                if (hit.collider != null && hit.distance > 0)
                {
                    Vector2 trueCenterBouncePoint = hit.centroid;

                    points.Add((Vector3)trueCenterBouncePoint);
                    
                    distanceRemaining -= hit.distance;

                    currentDirection = Vector2.Reflect(currentDirection, hit.normal);
                    
                    currentPosition = trueCenterBouncePoint + hit.normal * 0.05f;

                    if (distanceRemaining <= 0) break;
                }
                else
                {

                    points.Add((Vector3)currentPosition + (Vector3)currentDirection * distanceRemaining);
                    break;
                }
            }

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }
    }
}
