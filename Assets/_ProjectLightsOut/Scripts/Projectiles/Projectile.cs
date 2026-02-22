using System;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        private Vector2 direction;
        private int ricochetCount;
        private float destroyTimer = 10f;
        [SerializeField] private int maxRicochetCount = 3;
        [SerializeField] private SimplePool impactPool;
        [SerializeField] private SimplePool hitPool;
        private Action OnTargetHit;
        private int targetHit;

        /// <summary>
        /// Set by PlayerShoot so the projectile can return itself to the pool.
        /// </summary>
        [HideInInspector] public SimplePool ParentPool;

        private void Awake()
        {
            if (rb == null)
            {
                Debug.LogError($"{name}: Rigidbody2D is not assigned!");
            }

            OnTargetHit = () => { maxRicochetCount++; };
        }

        private void Update()
        {
            SelfDestruct();
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = direction;
        }

        public void SetDirection(Vector2 direction)
        {
            this.direction = direction;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            CheckTargetCollision(collision);
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            CheckTargetTrigger(collider);
        }

        private void CheckTargetTrigger(Collider2D collider)
        {
            IHittable hittable = collider.gameObject.GetComponent<IHittable>();

            if (hittable != null && hittable.IsHittable)
            {
                targetHit++;
                EventManager.Broadcast(new OnPlaySFX("EnemyHit"));
                EventManager.Broadcast(new OnCameraShake(0.1f, 0.05f));
                EventManager.Broadcast(new OnSlowTime(0.1f, 0.2f));
                hittable.OnHit(targetHit, OnTargetHit);

                SpawnHitEffect(collider.transform.position, collider.transform.up);
            }
        }

        private void CheckTargetCollision(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ricochet"))
            {
                destroyTimer = 10f;
                EventManager.Broadcast(new OnPlaySFX("WallHit"));
                
                if (ricochetCount < maxRicochetCount)
                {
                    ricochetCount++;
                    direction = Vector2.Reflect(direction, collision.GetContact(0).normal);
                    transform.up = direction;
                }

                else
                {
                    DestroyProjectile();
                }

                EventManager.Broadcast(new OnCameraShake(0.1f, 0.05f));
            }

            SpawnEffect(collision.GetContact(0).point + collision.GetContact(0).normal * 0.05f, collision.GetContact(0).normal);
        }

        private void SelfDestruct()
        {
            destroyTimer -= Time.deltaTime;

            if (destroyTimer <= 0)
            {
                DestroyProjectile();
            }
        }

        private void DestroyProjectile()
        {
            EventManager.Broadcast(new OnProjectileDestroy());
            ResetProjectile();

            if (ParentPool != null)
            {
                ParentPool.Return(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Resets all runtime state so the pooled object can be reused cleanly.
        /// </summary>
        public void ResetProjectile()
        {
            ricochetCount = 0;
            targetHit = 0;
            destroyTimer = 10f;
            direction = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }

        private void SpawnEffect(Vector2 position, Vector2 normal)
        {
            if (impactPool == null) return;

            GameObject impactFx = impactPool.Get(position, Quaternion.identity);
            impactFx.transform.right = normal;
            impactPool.Return(impactFx, 1f);
        }

        private void SpawnHitEffect(Vector2 position, Vector2 normal)
        {
            if (hitPool == null) return;

            GameObject hitFx = hitPool.Get(position, Quaternion.identity);
            hitFx.transform.right = normal;
            hitPool.Return(hitFx, 1f);
        }
    }
}