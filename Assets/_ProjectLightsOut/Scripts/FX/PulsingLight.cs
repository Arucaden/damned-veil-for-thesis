using UnityEngine;

#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine.Rendering.Universal;
#endif

namespace ProjectLightsOut.Gameplay.FX
{
    public class PulsingLight : MonoBehaviour
    {
        [Header("Light Pulse Settings")]
        [Tooltip("Optional: A Light2D component to gently pulse. If empty, finds one on this object.")]
        [SerializeField] private Light2D targetLight;
        [SerializeField] private float minIntensity = 0.5f;
        [SerializeField] private float maxIntensity = 1.5f;
        [SerializeField] private float pulseSpeed = 2f;

        private void Awake()
        {
            if (targetLight == null)
            {
                targetLight = GetComponentInChildren<Light2D>();
            }
        }

        private void Update()
        {
            if (targetLight != null)
            {
                float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; 
                targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
            }
        }
    }
}
