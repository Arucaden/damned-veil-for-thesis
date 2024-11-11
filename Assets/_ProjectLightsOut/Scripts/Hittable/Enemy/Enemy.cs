using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class Enemy : MonoBehaviour, IHittable
    {
        public bool IsHittable { get; private set; } = true;
        [SerializeField] private int health = 1;
        [SerializeField] private bool immortal;
        public int Health { get => health; }
        [SerializeField] private int score = 1000;
        public Action<int> OnDamaged;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject SpawnEffect;
        [SerializeField] private Collider2D col2d;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteRenderer shadowRenderer;

        private void Awake()
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

        private void Start()
        {
            EventManager.Broadcast(new OnEnemyRegister(this));
        }

        public void OnHit(int multiplier, Action OnTargetHit)
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
                Instantiate(SpawnEffect, transform.position - new Vector3(0, 0.1f, 1), Quaternion.identity);
            }

            StartCoroutine(SpawnDelay());
        }

        private IEnumerator SpawnDelay()
        {
            yield return new WaitForSeconds(1f);
            col2d.enabled = true;
            spriteRenderer.enabled = true;
            shadowRenderer.enabled = true;
        }
    }
}