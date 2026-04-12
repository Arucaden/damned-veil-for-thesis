using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Gameplay;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ProjectLightsOut.Managers
{
    [RequireComponent(typeof(Light2D))]
    public class GlobalLightingController : MonoBehaviour
    {
        [Header("Global Light Settings")]
        [Tooltip("The normal bright intensity of the Global Light")]
        public float normalIntensity = 1f;
        
        [Tooltip("The intensity during the blackout phase")]
        public float blackoutIntensity = 0.05f;

        private Light2D globalLight;
        private Coroutine lightTransitionCoroutine;

        private void Awake()
        {
            globalLight = GetComponent<Light2D>();
            // Ensure the initial light state is correct
            if (globalLight != null && globalLight.lightType != Light2D.LightType.Global)
            {
                Debug.LogWarning("[GlobalLightingController] Make sure this component is attached to a Global Light 2D!");
            }
        }

        private void OnEnable()
        {
            EventManager.AddListener<OnToggleBlackout>(HandleToggleBlackout);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnToggleBlackout>(HandleToggleBlackout);
        }

        private void HandleToggleBlackout(OnToggleBlackout evt)
        {
            if (globalLight == null) return;

            float targetIntensity = evt.EnableDarkness ? blackoutIntensity : normalIntensity;

            // Stop any ongoing fade so we can smoothly transition from the current intensity
            if (lightTransitionCoroutine != null)
            {
                StopCoroutine(lightTransitionCoroutine);
            }

            lightTransitionCoroutine = StartCoroutine(TransitionLightIntensity(targetIntensity, evt.TransitionTime));
        }

        private IEnumerator TransitionLightIntensity(float targetIntensity, float duration)
        {
            float startIntensity = globalLight.intensity;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                // Optional: apply smoothstep for a softer transition
                t = t * t * (3f - 2f * t);

                globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
                yield return null;
            }

            globalLight.intensity = targetIntensity;
            lightTransitionCoroutine = null;
        }
    }
}
