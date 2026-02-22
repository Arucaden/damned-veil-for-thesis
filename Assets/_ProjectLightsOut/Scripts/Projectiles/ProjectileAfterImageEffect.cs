using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectLightsOut.Effects
{
    public class ProjectileAfterImageEffect : MonoBehaviour
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private SpriteRenderer projectileSpriteRenderer;
        [SerializeField] private float trailEffectRate = 0.1f;

        private void Start()
        {
            StartCoroutine(TrailEffect());
        }

        private IEnumerator TrailEffect()
        {
            while (true)
            {
                yield return new WaitForSeconds(trailEffectRate);
                CreateTrailEffect();
            }
        }

        [SerializeField] private float trailLifetime = 0.3f;

        private void CreateTrailEffect()
        {
            GameObject trailEffect = new GameObject("TrailAfterImage");
            trailEffect.transform.position = transform.position;
            trailEffect.transform.localScale = projectilePrefab.transform.localScale;

            GameObject trailEffectSprite = new GameObject("Sprite");
            trailEffectSprite.transform.SetParent(trailEffect.transform);
            trailEffectSprite.transform.localPosition = Vector3.zero;
            trailEffectSprite.transform.localScale = projectileSpriteRenderer.transform.localScale;

            SpriteRenderer trailEffectSpriteRenderer = trailEffectSprite.AddComponent<SpriteRenderer>();
            trailEffectSpriteRenderer.sortingLayerName = projectileSpriteRenderer.sortingLayerName;
            trailEffectSpriteRenderer.sprite = projectileSpriteRenderer.sprite;
            trailEffectSpriteRenderer.color = new Color(trailEffectSpriteRenderer.color.r, trailEffectSpriteRenderer.color.g, trailEffectSpriteRenderer.color.b, 0.25f);

            Destroy(trailEffect, trailLifetime);
        }
    }
}
