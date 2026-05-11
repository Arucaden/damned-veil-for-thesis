using System;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using ProjectLightsOut.Hittable;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        private Vector2 direction;
        public Vector2 Direction => direction;
        private int ricochetCount;
        public int RicochetCount => ricochetCount;
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
        
        [Tooltip("Multiplies the initial speed fired by PlayerShoot. Set to 1 for generic bullet, 3 for a high-speed 'Void Piercer' variant!")]
        [SerializeField] private float customSpeedMultiplier = 1f;

        [HideInInspector] public SimplePool ParentPool;
        public bool IsEnemyProjectile { get; set; } = false;

        private float bulletRadius = 0.1f;
        private LayerMask collisionMask;

        private void Awake()
        {
            if (rb == null)
            {
                Debug.LogError($"{name}: Rigidbody2D is not assigned!");
            }
            
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col != null) bulletRadius = col.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);

            collisionMask = ~(1 << LayerMask.NameToLayer("Ignore Laser"));

            OnTargetHit = () => { maxRicochetCount++; };
        }

        private RaycastHit2D[] hitResults = new RaycastHit2D[16];

        private void Update()
        {
            SelfDestruct();
        }

        private void FixedUpdate()
        {
            if (direction.sqrMagnitude == 0) return;

            float distanceToMove = direction.magnitude * Time.fixedDeltaTime;
            Vector2 currentPos = rb.position;
            Vector2 moveDir = direction.normalized;

            int hitCount = Physics2D.CircleCastNonAlloc(currentPos, bulletRadius, moveDir, hitResults, distanceToMove, collisionMask);
            
            RaycastHit2D hit = default;
            float minDistance = float.MaxValue;
            bool hitFound = false;

            for (int i = 0; i < hitCount; i++)
            {
                var h = hitResults[i];
                if (h.collider != null && h.collider.gameObject != this.gameObject)
                {
                    if (h.distance < minDistance)
                    {
                        minDistance = h.distance;
                        hit = h;
                        hitFound = true;
                    }
                }
            }

            if (hitFound)
            {
                PortalBase portal = hit.collider.GetComponent<PortalBase>();

                if (portal != null)
                {
                    if (portal.TryEnterPortal(this, hit.centroid, hit.normal, out Vector2 exitPos, out Vector2 exitDir))
                    {
                        rb.MovePosition(exitPos);
                        direction = exitDir.normalized * direction.magnitude;
                        transform.up = direction.normalized;
                    }
                    else
                    {
                        DoRicochet(hit);
                    }
                }
                else if (hit.collider.TryGetComponent<Projectile>(out var otherProjectile) && otherProjectile.IsEnemyProjectile != this.IsEnemyProjectile)
                {
                    Vector2 impactNormal = hit.normal;
                    if (impactNormal == Vector2.zero) 
                    {
                        impactNormal = (currentPos - (Vector2)otherProjectile.transform.position).normalized;
                        if (impactNormal == Vector2.zero) impactNormal = Vector2.up;
                    }

                    otherProjectile.ForceClash(-impactNormal, hit.point);
                    hit.normal = impactNormal;
                    ForceClash(impactNormal, hit.point);
                }
                else if (hit.collider.CompareTag("Ricochet"))
                {
                    DoRicochet(hit);
                }
                else if (!hit.collider.isTrigger)
                {
                    IHittable hittable = hit.collider.GetComponent<IHittable>();
                    if (hittable != null && hittable.IsHittable)
                    {
                        if (hittable is DestructibleWall wall) wall.TakeDamageFromProjectile(damage, IsEnemyProjectile);
                        else hittable.OnHit(damage, OnTargetHit);
                    }

                    SpawnEffect(hit.point + hit.normal * 0.05f, hit.normal);
                    DestroyProjectile();
                }
                else 
                {
                    rb.MovePosition(currentPos + moveDir * distanceToMove);
                }
            }
            else
            {
                rb.MovePosition(currentPos + moveDir * distanceToMove);
            }
        }

        private void DoRicochet(RaycastHit2D hit)
        {
            destroyTimer = 10f;
            EventManager.Broadcast(new OnPlaySFX("WallHit"));

            IHittable hittable = hit.collider.GetComponent<IHittable>();
            if (hittable != null && hittable.IsHittable)
            {
                if (hittable is DestructibleWall wall) wall.TakeDamageFromProjectile(damage, IsEnemyProjectile);
                else hittable.OnHit(damage, OnTargetHit);
            }
            
            if (ricochetCount < maxRicochetCount)
            {
                ricochetCount++;
                
                Vector2 impactCenter = hit.centroid;
                
                direction = Vector2.Reflect(direction, hit.normal);
                transform.up = direction;

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

        public void SetDirection(Vector2 direction)
        {
            this.direction = direction * customSpeedMultiplier;
        }

        public void ForceRicochet(Vector2 normal, Vector2 impactPoint)
        {
            if (ricochetCount >= maxRicochetCount)
            {
                DestroyProjectile();
                return;
            }

            destroyTimer = 10f;
            EventManager.Broadcast(new OnPlaySFX("WallHit"));
            
            ricochetCount++;
            
            direction = Vector2.Reflect(direction, normal);
            if (direction != Vector2.zero)
            {
                transform.up = direction.normalized;
            }

            rb.MovePosition(rb.position + normal * 0.05f);

            EventManager.Broadcast(new OnCameraShake(0.1f, 0.05f));
            SpawnEffect(impactPoint + normal * 0.05f, normal);
        }

        public void ForceClash(Vector2 normal, Vector2 impactPoint)
        {
            destroyTimer = 10f;
            EventManager.Broadcast(new OnPlaySFX("WallHit"));

            direction = Vector2.Reflect(direction, normal);
            if (direction != Vector2.zero)
            {
                transform.up = direction.normalized;
            }

            rb.MovePosition(rb.position + normal * 0.05f);

            EventManager.Broadcast(new OnCameraShake(0.1f, 0.05f));
            SpawnEffect(impactPoint + normal * 0.05f, normal);
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
                if (hittable is DestructibleWall wall)
                {
                    wall.TakeDamageFromProjectile(damage, IsEnemyProjectile);
                }
                else
                {
                    if (IsEnemyProjectile) return;

                    targetHit++;
                    EventManager.Broadcast(new OnPlaySFX("EnemyHit"));
                    EventManager.Broadcast(new OnCameraShake(0.1f, 0.05f));
                    EventManager.Broadcast(new OnSlowTime(0.1f, 0.2f));
                    hittable.OnHit(targetHit, OnTargetHit);

                    SpawnHitEffect(collider.transform.position, collider.transform.up);
                }
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
            IsEnemyProjectile = false;
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