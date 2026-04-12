using System.Collections.Generic;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    [RequireComponent(typeof(LineRenderer))]
    public class PlayerLaserAimer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerShoot playerShoot; // To read state and stats
        
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
            // Rotate the root PlayerShoot object so everything (including sprites) follows the mouse
            Vector2 direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - playerShoot.transform.position;
            Direction = direction;
            
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
            playerShoot.transform.rotation = targetRotation;
            
            // If this script is attached to a disconnected object, explicitly rotate it too
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
            points.Add(transform.position);

            Vector2 currentPosition = transform.position;
            Vector2 currentDirection = transform.up; // We face the mouse, so UP is forward
            int remainingBounces = playerShoot.Ricochets; // Read directly from player stats
            float distanceRemaining = maxLaserDistance;

            LayerMask effectiveMask = collisionLayerMask;
            // Fallback if inspector isn't set, match original logic:
            if (effectiveMask.value == 0)
            {
                effectiveMask = ~(1 << LayerMask.NameToLayer("Ignore Laser") | 1 << LayerMask.NameToLayer("Projectile"));
            }

            // Loop to calculate all bounces
            for (int i = 0; i <= remainingBounces; i++)
            {
                // To simulate the bullet's width, we cast two rays from edges and pick the closest hit
                Vector2 rightOffset = new Vector2(-currentDirection.y, currentDirection.x) * bulletRadiusInWorldSpace;
                Vector3 leftRayOrigin = (Vector3)currentPosition - (Vector3)rightOffset;
                Vector3 rightRayOrigin = (Vector3)currentPosition + (Vector3)rightOffset;

                RaycastHit2D hitLeft = Physics2D.Raycast(leftRayOrigin, currentDirection, distanceRemaining, effectiveMask);
                RaycastHit2D hitRight = Physics2D.Raycast(rightRayOrigin, currentDirection, distanceRemaining, effectiveMask);

                // Determine which side hit first
                RaycastHit2D closestHit = hitLeft;
                bool hitOccurred = hitLeft.collider != null || hitRight.collider != null;

                if (hitLeft.collider != null && hitRight.collider != null)
                {
                    closestHit = hitLeft.distance < hitRight.distance ? hitLeft : hitRight;
                }
                else if (hitRight.collider != null)
                {
                    closestHit = hitRight;
                }

                if (hitOccurred && closestHit.distance > 0)
                {
                    // Calculate the TRUE center point of the bounce
                    // Since we casted from the edges, the hit.point is on the edge of the bullet's width.
                    // We must shift it back to the true center to prevent the next origin from being inside the wall.
                    Vector2 trueCenterBouncePoint = closestHit == hitLeft 
                        ? closestHit.point + rightOffset 
                        : closestHit.point - rightOffset;

                    // Add the true center hit point to our renderer
                    points.Add((Vector3)trueCenterBouncePoint);
                    
                    // Deduct distance
                    distanceRemaining -= closestHit.distance;

                    // Update direction and position for the next bounce iteration
                    currentDirection = Vector2.Reflect(currentDirection, closestHit.normal);
                    
                    // Push the origin slightly forward along the reflected path AND the normal to ensure we completely escape the wall's collider surface.
                    currentPosition = trueCenterBouncePoint + currentDirection * 0.05f + closestHit.normal * 0.01f;

                    // If we're out of distance, break entirely
                    if (distanceRemaining <= 0) break;
                }
                else
                {
                    // No hit, ray just goes out to max distance
                    points.Add((Vector3)currentPosition + (Vector3)currentDirection * distanceRemaining);
                    break;
                }
            }

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }
    }
}
