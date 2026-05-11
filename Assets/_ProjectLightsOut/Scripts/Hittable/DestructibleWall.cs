using System;
using UnityEngine;
using System.Collections;
using ProjectLightsOut.Gameplay;
using UnityEngine.Tilemaps;

namespace ProjectLightsOut.Hittable
{
    public class DestructibleWall : MonoBehaviour, IHittable
    {
        [Tooltip("If this collider is a child object (like a Tilemap Collider), drag the Parent building GameObject here so the whole building gets destroyed. If left empty, it destroys itself.")]
        [SerializeField] private GameObject rootObjectToDestroy;

        [Tooltip("If true, the object is physically destroyed on death. If false, it merely hides and becomes intangible, allowing it to be Respawned later.")]
        [SerializeField] private bool destroyOnDeath = true;

        [Header("Health Settings")]
        [Tooltip("How many ricochet hits this wall can take before being destroyed.")]
        [SerializeField] private int maxHealth = 2;
        private int currentHealth;

        [Header("Damage Visuals")]
        [Tooltip("The color it will briefly flash when damaged. Default is semi-transparent white.")]
        [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private float flashDuration = 0.08f;
        
        [Header("Death Visuals")]
        [Tooltip("How long it takes the wall to fade out after health reaches 0.")]
        [SerializeField] private float fadeOutDuration = 0.3f;

        private SpriteRenderer[] childSpriteRenderers;
        private Tilemap[] childTilemaps;
        private Color[] originalSpriteColors;
        private Color[] originalTilemapColors;

        // --- IHittable implementation ---
        public bool IsHittable { get; private set; } = true;

        public void OnHit(int multiplier, Action onDeadAction = null)
        {
            TakeDamage(multiplier);
            if (currentHealth <= 0) onDeadAction?.Invoke();
        }
        // --------------------------------

        private void Start()
        {
            currentHealth = maxHealth;

            // Auto-gather visual components
            GameObject target = rootObjectToDestroy != null ? rootObjectToDestroy : gameObject;
            
            childSpriteRenderers = target.GetComponentsInChildren<SpriteRenderer>(true);
            originalSpriteColors = new Color[childSpriteRenderers.Length];
            for (int i = 0; i < childSpriteRenderers.Length; i++)
                originalSpriteColors[i] = childSpriteRenderers[i].color;

            childTilemaps = target.GetComponentsInChildren<Tilemap>(true);
            originalTilemapColors = new Color[childTilemaps.Length];
            for (int i = 0; i < childTilemaps.Length; i++)
                originalTilemapColors[i] = childTilemaps[i].color;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Projectile proj = collision.gameObject.GetComponent<Projectile>();
            if (proj != null)
            {
                if (proj.IsEnemyProjectile) return;

                TakeDamageFromProjectile(proj.Damage, proj.IsEnemyProjectile);
            }
        }

        public event Action OnWallDestroyed;
        public event Action<bool> OnWallDestroyedBy;
        public event Action<bool> OnWallHitByEnemy;

        private void TakeDamage(int damage)
        {
            TakeDamageFromProjectile(damage, false);
        }

        public void TakeDamageFromProjectile(int damage, bool isEnemyProjectile)
        {
            if (isEnemyProjectile)
            {
                OnWallHitByEnemy?.Invoke(true);
                return;
            }
            if (currentHealth <= 0) return;

            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                OnWallDestroyedBy?.Invoke(isEnemyProjectile);
                OnWallDestroyed?.Invoke();
                
                Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
                foreach (Collider2D col in allColliders) col.enabled = false;

                IsHittable = false;
                StopAllCoroutines();
                
                StartCoroutine(FadeOutDestroyCoroutine());
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(FlashEffectCoroutine());
            }
        }

        public void Respawn()
        {
            StopAllCoroutines();
            currentHealth = maxHealth;
            IsHittable = true;

            Collider2D[] allColliders = GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D col in allColliders) col.enabled = true;

            for (int i = 0; i < childSpriteRenderers.Length; i++)
            {
                if (childSpriteRenderers[i] != null) childSpriteRenderers[i].color = originalSpriteColors[i];
            }
            for (int i = 0; i < childTilemaps.Length; i++)
            {
                if (childTilemaps[i] != null) childTilemaps[i].color = originalTilemapColors[i];
            }
        }

        private IEnumerator FlashEffectCoroutine()
        {
            foreach (var sr in childSpriteRenderers) { if (sr != null) sr.color = flashColor; }
            foreach (var tm in childTilemaps) { if (tm != null) tm.color = flashColor; }

            yield return new WaitForSeconds(flashDuration);

            for (int i = 0; i < childSpriteRenderers.Length; i++)
            {
                if (childSpriteRenderers[i] != null) childSpriteRenderers[i].color = originalSpriteColors[i];
            }
            for (int i = 0; i < childTilemaps.Length; i++)
            {
                if (childTilemaps[i] != null) childTilemaps[i].color = originalTilemapColors[i];
            }
        }

        private IEnumerator FadeOutDestroyCoroutine()
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeOutDuration;
                
                for (int i = 0; i < childSpriteRenderers.Length; i++)
                {
                    if (childSpriteRenderers[i] != null)
                    {
                        Color c = originalSpriteColors[i];
                        c.a = Mathf.Lerp(originalSpriteColors[i].a, 0f, progress);
                        childSpriteRenderers[i].color = c;
                    }
                }

                for (int i = 0; i < childTilemaps.Length; i++)
                {
                    if (childTilemaps[i] != null)
                    {
                        Color c = originalTilemapColors[i];
                        c.a = Mathf.Lerp(originalTilemapColors[i].a, 0f, progress);
                        childTilemaps[i].color = c;
                    }
                }

                yield return null;
            }

            if (destroyOnDeath)
            {
                GameObject targetToDestroy = rootObjectToDestroy != null ? rootObjectToDestroy : gameObject;
                Destroy(targetToDestroy);
            }
        }
    }
}
