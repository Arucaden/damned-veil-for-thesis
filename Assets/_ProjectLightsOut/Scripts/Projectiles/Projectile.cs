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
        public int MaxRicochetCount => maxRicochetCount;
        [SerializeField] private SimplePool impactPool;
        [SerializeField] private SimplePool hitPool;
        private Action OnTargetHit;
        private int targetHit;

        [Header("Combat Settings")]
        [SerializeField] private int damage = 1;
        public int Damage => damage;

        [HideInInspector] public SimplePool ParentPool;

        // Freeze state (used by Kronos time field)
        private bool isFrozen;
        private Vector2 frozenDirection;
        public bool IsFrozen => isFrozen;

        private float bulletRadius = 0.1f;
        private LayerMask collisionMask;

        private void Awake()
        {
            if (rb == null)
            {
                Debug.LogError($"{name}: Rigidbody2D is not assigned!");
            }
            
            // Switch to Kinematic to bypass black-box physics resolving that causes stuttering/sticking
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col != null) bulletRadius = col.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);

            collisionMask = ~(1 << LayerMask.NameToLayer("Ignore Laser") | 1 << LayerMask.NameToLayer("Projectile"));

            OnTargetHit = () => { maxRicochetCount++; };
        }

        private void Update()
        {
            if (!isFrozen)
                SelfDestruct();
        }

        private void FixedUpdate()
        {
            if (isFrozen || direction.sqrMagnitude == 0) return;

            float distanceToMove = direction.magnitude * Time.fixedDeltaTime;
            Vector2 currentPos = rb.position;
            Vector2 moveDir = direction.normalized;

            // Execute the exact same mathematical sweep the Laser Aimer uses
            RaycastHit2D hit = Physics2D.CircleCast(currentPos, bulletRadius, moveDir, distanceToMove, collisionMask);

            if (hit.collider != null && hit.distance > 0)
            {
                if (hit.collider.CompareTag("Ricochet"))
                {
                    destroyTimer = 10f;
                    EventManager.Broadcast(new OnPlaySFX("WallHit"));

                    // Manually notify solid destructibles (like DestructibleWall) about the hit
                    // because Kinematic bodies no longer trigger OnCollisionEnter2D
                    IHittable hittable = hit.collider.GetComponent<IHittable>();
                    if (hittable != null && hittable.IsHittable) hittable.OnHit(damage, OnTargetHit);
                    
                    if (ricochetCount < maxRicochetCount)
                    {
                        ricochetCount++;
                        
                        // Advance exactly to the mathematical impact centroid
                        Vector2 impactCenter = hit.centroid;
                        
                        direction = Vector2.Reflect(direction, hit.normal);
                        transform.up = direction;

                        // Offset the remainder using the unified 0.05f constant
                        rb.MovePosition(impactCenter + hit.normal * 0.05f);
                    }
                    else
                    {
                        DestroyProjectile();
                        return;
                    }

                    EventManager.Broadcast(new OnCameraShake(0.1f, 0.05f));
                    SpawnEffect(hit.point + hit.normal * 0.05f, hit.normal);
                }
                else if (!hit.collider.isTrigger)
                {
                    // Solid wall without ricochet capabilities
                    IHittable hittable = hit.collider.GetComponent<IHittable>();
                    if (hittable != null && hittable.IsHittable) hittable.OnHit(damage, OnTargetHit);

                    SpawnEffect(hit.point + hit.normal * 0.05f, hit.normal);
                    DestroyProjectile();
                }
                else 
                {
                    // Hit a purely visual/trigger collider (like an enemy), ignore the wall collision and proceed
                    rb.MovePosition(currentPos + moveDir * distanceToMove);
                }
            }
            else
            {
                // No geometric impact, advance normally
                rb.MovePosition(currentPos + moveDir * distanceToMove);
            }
        }

        public void SetDirection(Vector2 direction)
        {
            this.direction = direction;
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

        public void ResetProjectile()
        {
            ricochetCount = 0;
            targetHit = 0;
            destroyTimer = 10f;
            direction = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            isFrozen = false;
            frozenDirection = Vector2.zero;
        }

        // --- Freeze API (for Kronos time field) ---

        public void Freeze()
        {
            if (isFrozen) return;
            isFrozen = true;
            frozenDirection = direction;
            direction = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }

        public void Unfreeze()
        {
            if (!isFrozen) return;
            isFrozen = false;
            direction = frozenDirection;
            frozenDirection = Vector2.zero;
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