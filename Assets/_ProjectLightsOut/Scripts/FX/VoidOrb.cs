using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    /// <summary>
    /// A homing void magic orb spawned by Awakened Azaleth.
    ///
    /// Behaviour:
    ///   • Homes toward the player Transform at a speed that ramps up over time.
    ///   • On contact with the player's trigger collider → broadcasts OnVoidOrbHitPlayer
    ///     which triggers an instant game over.
    ///   • Each orb passively drains 1 ricochet from PlayerShoot when it is activated
    ///     (boss controls when to tick this via ActivateOrb()).
    ///   • Can be deactivated/pooled via Deactivate() without triggering the hit effect
    ///     (used during the transition wipe).
    /// </summary>
    public class VoidOrb : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Starting homing speed (units per second)")]
        [SerializeField] private float startSpeed = 1f;

        [Tooltip("Speed added every second while alive")]
        [SerializeField] private float acceleration = 0.15f;

        [Tooltip("Maximum homing speed cap (units per second)")]
        [SerializeField] private float maxSpeed = 3.5f;

        [Header("Pool")]
        [HideInInspector] public SimplePool ParentPool;

        // ── Runtime state ──────────────────────────────────────────────
        private Transform playerTarget;
        private float currentSpeed;
        private bool isActive;
        private bool hasHit;

        // ── Public API ─────────────────────────────────────────────────

        /// <summary>
        /// Call after getting the orb from the pool to begin homing.
        /// </summary>
        public void Activate(Transform player)
        {
            playerTarget   = player;
            currentSpeed   = startSpeed;
            isActive       = true;
            hasHit         = false;
        }

        /// <summary>
        /// Silently deactivate and return to pool (used during transition wipe).
        /// Does NOT broadcast OnVoidOrbHitPlayer.
        /// </summary>
        public void Deactivate()
        {
            isActive = false;
            ReturnToPool();
        }

        // ── Unity ──────────────────────────────────────────────────────

        private void Update()
        {
            if (!isActive || hasHit || playerTarget == null) return;

            // Ramp speed
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);

            // Move toward player
            Vector3 dir = (playerTarget.position - transform.position).normalized;
            transform.position += dir * currentSpeed * Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActive || hasHit) return;

            // Check if we touched the player (via IHittable or tag)
            if (other.CompareTag("Player"))
            {
                hasHit = true;
                isActive = false;

                EventManager.Broadcast(new OnVoidOrbHitPlayer());
                ReturnToPool();
            }
        }

        // ── Internal ───────────────────────────────────────────────────

        private void ReturnToPool()
        {
            if (ParentPool != null)
                ParentPool.Return(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
