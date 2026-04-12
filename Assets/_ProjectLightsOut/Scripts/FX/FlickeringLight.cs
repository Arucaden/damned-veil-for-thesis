using UnityEngine;

#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine.Rendering.Universal;
#endif

namespace ProjectLightsOut.Gameplay.FX
{
    [RequireComponent(typeof(Light2D))]
    public class FlickeringLight : MonoBehaviour
    {
        [Header("Flicker Settings")]
        public float minIntensity = 0.8f;
        public float maxIntensity = 1.2f;
        
        [Tooltip("How fast the flicker noise updates")]
        public float flickerSpeed = 5f;

        private Light2D attachedLight;
        private float randomOffset;

        private void Awake()
        {
            attachedLight = GetComponent<Light2D>();
            // Add a random offset so all lights don't flicker in perfect sync
            randomOffset = Random.Range(0f, 100f);
        }

        private void Update()
        {
            if (attachedLight == null) return;

            // Simple perlin noise for smooth, natural flickering
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed + randomOffset, 0f);

            // Interpolate intensity
            attachedLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

        }
    }
}
