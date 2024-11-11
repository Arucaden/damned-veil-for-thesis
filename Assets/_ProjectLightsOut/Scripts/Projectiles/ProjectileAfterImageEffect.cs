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

        private void CreateTrailEffect()
        {
            GameObject trailEffect = Instantiate(new GameObject(), transform.position, Quaternion.identity);
            GameObject trailEffectSprite = Instantiate(new GameObject(), trailEffect.transform.position, Quaternion.identity);
            trailEffectSprite.transform.SetParent(trailEffect.transform);
            trailEffect.transform.localScale = projectilePrefab.transform.localScale;
            trailEffectSprite.transform.localScale = projectileSpriteRenderer.transform.localScale;

            SpriteRenderer trailEffectSpriteRenderer = trailEffectSprite.AddComponent<SpriteRenderer>();
            trailEffectSpriteRenderer.sortingLayerName = projectileSpriteRenderer.sortingLayerName;
            trailEffectSpriteRenderer.sprite = projectileSpriteRenderer.sprite;
            trailEffectSpriteRenderer.color = new Color(trailEffectSpriteRenderer.color.r, trailEffectSpriteRenderer.color.g, trailEffectSpriteRenderer.color.b, 0.25f);
        }
    }
}
