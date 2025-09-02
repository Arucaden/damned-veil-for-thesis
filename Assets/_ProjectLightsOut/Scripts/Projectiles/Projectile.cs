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
        [SerializeField] private GameObject impactEffect;
        [SerializeField] private GameObject hitEffect;
        private Action OnTargetHit;
        private int targetHit;

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
            Destroy(gameObject);
        }

        private void SpawnEffect(Vector2 position, Vector2 normal)
        {
            if (impactEffect == null) return;

            Transform impactFx = Instantiate(impactEffect, position, Quaternion.identity).transform;
            impactFx.right = normal;
        }

        private void SpawnHitEffect(Vector2 position, Vector2 normal)
        {
            if (hitEffect == null) return;

            Transform hitFx = Instantiate(hitEffect, position, Quaternion.identity).transform;
            hitFx.right = normal;
        }
    }
}