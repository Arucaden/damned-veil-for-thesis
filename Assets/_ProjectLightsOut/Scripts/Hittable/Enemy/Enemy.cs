using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class Enemy : MonoBehaviour, IHittable
    {
        public bool IsHittable { get; set; } = true;
        public string EnemyIdentifier { get; set; }
        public WaveDataSO WaveData { get; set; }
        [SerializeField] protected int health = 1;
        [SerializeField] private bool immortal;
        public int Health { get => health; }
        [SerializeField] protected int score = 1000;
        public Action<int> OnDamaged;
        [SerializeField] protected Animator animator;
        [SerializeField] protected GameObject SpawnEffect;
        [SerializeField] protected Collider2D col2d;
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected SpriteRenderer shadowRenderer;
        [SerializeField] protected GameObject killEffect;
        [SerializeField] protected SimplePool spawnEffectPool;
        [SerializeField] protected SimplePool killEffectPool;
        protected Action OnSpawned;

        protected Color originalColor = Color.white;
        protected bool originalIsHittableState = true;

        protected virtual void Awake()
        {
            if (animator == null)
            {
                Debug.LogError($"{name}: Missing an animator component");
            }

            if (col2d == null)
            {
                Debug.LogError($"{name}: Missing a collider2D component");
            }

            if (spriteRenderer == null)
            {
                Debug.LogError($"{name}: Missing a spriteRenderer component");
            }
        }

        protected virtual void Start()
        {
            if (spriteRenderer != null) originalColor = spriteRenderer.color;
            originalIsHittableState = IsHittable;
            
            EventManager.Broadcast(new OnEnemyRegister(this));
        }

        public void SetWardenShield(bool isShielded, Color? shieldColor = null)
        {
            if (isShielded)
            {
                IsHittable = false;
                if (shieldColor.HasValue && spriteRenderer != null)
                {
                    spriteRenderer.color = shieldColor.Value;
                }
            }
            else
            {
                IsHittable = originalIsHittableState;
                if (spriteRenderer != null) spriteRenderer.color = originalColor;
            }
        }

        public virtual void OnHit(int multiplier, Action OnTargetHit)
        {
            if (!IsHittable) return;

            health--;
            OnDamaged?.Invoke(multiplier);
            OnTargetHit?.Invoke();

            if (immortal) return;
            
            if (health <= 0)
            {
                IsHittable = false;
                EventManager.Broadcast(new OnEnemyDead(this));
                EventManager.Broadcast(new OnAddScore(score * multiplier));
                EventManager.Broadcast(new OnPlaySFX("Kill"));
                if (killEffectPool != null)
                {
                    GameObject fx = killEffectPool.Get(transform.position, Quaternion.identity);
                    killEffectPool.Return(fx, 1f);
                }
                else
                {
                    Instantiate(killEffect, transform.position, Quaternion.identity);
                }
                StartCoroutine(DeadDelay());
            }
        }

        private IEnumerator DeadDelay()
        {
            yield return new WaitForSeconds(0.2f);
            animator.SetTrigger("Dead");
            shadowRenderer.enabled = false;
        }

        public void Spawn()
        {
            col2d.enabled = false;
            spriteRenderer.enabled = false;
            shadowRenderer.enabled = false;

            if (SpawnEffect != null)
            {
                if (spawnEffectPool != null)
                {
                    GameObject fx = spawnEffectPool.Get(transform.position - new Vector3(0, 0.1f, 1), Quaternion.identity);
                    spawnEffectPool.Return(fx, 1f);
                }
                else
                {
                    Instantiate(SpawnEffect, transform.position - new Vector3(0, 0.1f, 1), Quaternion.identity);
                }
            }

            StartCoroutine(SpawnDelay());
        }

        private IEnumerator SpawnDelay()
        {
            yield return new WaitForSeconds(1f);
            col2d.enabled = true;
            spriteRenderer.enabled = true;
            shadowRenderer.enabled = true;

            OnSpawned?.Invoke();
        }
    }
}