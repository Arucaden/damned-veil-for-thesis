using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
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

        private Transform playerTarget;
        private float currentSpeed;
        private bool isActive;
        private bool hasHit;

        public void Activate(Transform player)
        {
            playerTarget   = player;
            currentSpeed   = startSpeed;
            isActive       = true;
            hasHit         = false;
        }

        public void Deactivate()
        {
            isActive = false;
            ReturnToPool();
        }

        private void Update()
        {
            if (!isActive || hasHit || playerTarget == null) return;

            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);

            Vector3 dir = (playerTarget.position - transform.position).normalized;
            transform.position += dir * currentSpeed * Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActive || hasHit) return;

            if (other.CompareTag("Player"))
            {
                hasHit = true;
                isActive = false;

                EventManager.Broadcast(new OnVoidOrbHitPlayer());
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            if (ParentPool != null)
                ParentPool.Return(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
