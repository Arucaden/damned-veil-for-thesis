using UnityEngine;
#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine.Rendering.Universal;
#endif

namespace ProjectLightsOut.Gameplay
{
    public class PortalVisuals : MonoBehaviour
    {
        [Header("Spin Settings")]
        [Tooltip("The Transform holding the portal sprite. Leave empty to spin this object.")]
        [SerializeField] private Transform spriteTransform;
        [SerializeField] private float spinSpeed = 180f;

        [Header("Light Pulse Settings")]
        [Tooltip("Optional: A Light2D component to gently pulse.")]
        [SerializeField] private Light2D portalLight;
        [SerializeField] private float minIntensity = 0.5f;
        [SerializeField] private float maxIntensity = 1.5f;
        [SerializeField] private float pulseSpeed = 2f;

        private void Awake()
        {
            if (spriteTransform == null)
            {
                spriteTransform = transform;
            }

            if (portalLight == null)
            {
                portalLight = GetComponentInChildren<Light2D>();
            }
        }

        private void Update()
        {
            // Spin the sprite along the Z axis
            if (spriteTransform != null)
            {
                spriteTransform.Rotate(0, 0, spinSpeed * Time.deltaTime);
            }

            // Pulse the light using a gentle, smooth sine wave
            if (portalLight != null)
            {
                // Mathf.Sin returns -1 to 1. We +1 and /2 to normalize it to 0 to 1.
                float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; 
                portalLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
            }
        }
    }
}
