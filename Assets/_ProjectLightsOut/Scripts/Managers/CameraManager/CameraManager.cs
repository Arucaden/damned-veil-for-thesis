using System.Collections;
using ProjectLightsOut.DevUtils;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    public class CameraManager : Singleton<CameraManager>
    {
        [SerializeField] private Camera mainCamera;
        private Vector3 originalPosition;
        private float dampingSpeed = 1.0f;
        private Coroutine cameraPanCoroutine;
        private Coroutine cameraShakeCoroutine;
        private Coroutine cameraZoomCoroutine;
        private float originalOrthographicSize;
        private bool isSpotting = false;

        protected override void Awake()
        {
            base.Awake();
            originalPosition = mainCamera.transform.localPosition;
            originalOrthographicSize = mainCamera.orthographicSize;
        }

        private void OnEnable()
        {
            EventManager.AddListener<OnCameraShake>(OnCameraShake);
            EventManager.AddListener<OnSpotting>(OnSpotting);
            EventManager.AddListener<OnSpottingEnd>(OnSpottingEnd);
            EventManager.AddListener<OnZoom>(OnZoom);
            EventManager.AddListener<OnZoomEnd>(OnZoomEnd);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnCameraShake>(OnCameraShake);
            EventManager.RemoveListener<OnSpotting>(OnSpotting);
            EventManager.RemoveListener<OnSpottingEnd>(OnSpottingEnd);
            EventManager.RemoveListener<OnZoom>(OnZoom);
            EventManager.RemoveListener<OnZoomEnd>(OnZoomEnd);
        }

        private void OnCameraShake(OnCameraShake evt)
        {
            if (cameraShakeCoroutine != null)
            {
                StopCoroutine(cameraShakeCoroutine);
            }

            if (cameraPanCoroutine != null || isSpotting)
            {
                return;
            }

            cameraShakeCoroutine = StartCoroutine(ShakeCamera(evt.Duration, evt.Magnitude));
        }

        private void OnZoom(OnZoom evt)
        {
            if (cameraZoomCoroutine != null)
            {
                StopCoroutine(cameraZoomCoroutine);
            }

            cameraZoomCoroutine = StartCoroutine(CameraZoom(evt.Zoom, evt.ZoomSpeed));
        }

        private void OnZoomEnd(OnZoomEnd evt)
        {
            if (cameraZoomCoroutine != null)
            {
                StopCoroutine(cameraZoomCoroutine);
            }

            cameraZoomCoroutine = StartCoroutine(CameraZoom(originalOrthographicSize, evt.ZoomSpeed, false));
        }

        private IEnumerator ShakeCamera(float duration, float shakeMagnitude)
        {
            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                mainCamera.transform.localPosition = originalPosition + Random.insideUnitSphere * shakeMagnitude;
                elapsed += Time.deltaTime * dampingSpeed;

                elapsed += Time.deltaTime;
                yield return null;
            }

            mainCamera.transform.localPosition = originalPosition;
        }

        private IEnumerator CameraPan(Vector3 targetPosition, float moveTime)
        {
            if (cameraShakeCoroutine != null)
            {
                StopCoroutine(cameraShakeCoroutine);
            }

            if (moveTime <= 0.0f)
            {
                mainCamera.transform.position = targetPosition;
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, originalPosition.z);
                yield break;
            }
            
            float timer = 0.0f;
            Vector3 lastPosition = mainCamera.transform.position;

            while (timer < moveTime)
            {
                mainCamera.transform.position = Vector3.Lerp(lastPosition, targetPosition, timer / moveTime);
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, lastPosition.z);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            mainCamera.transform.position = targetPosition;
            mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, lastPosition.z);

            cameraPanCoroutine = null;
        }

        private IEnumerator CameraZoom(float targetOrthographicSize, float zoomTime, bool add = true)
        {
            if (zoomTime <= 0.0f)
            {
                if (add)
                {
                    mainCamera.orthographicSize += targetOrthographicSize;
                }

                else
                {
                    mainCamera.orthographicSize = targetOrthographicSize;
                }
                yield break;
            }

            float timer = 0.0f;
            float lastOrthographicSize = mainCamera.orthographicSize;

            while (timer < zoomTime)
            {
                if (add)
                {
                    mainCamera.orthographicSize = Mathf.Lerp(lastOrthographicSize, lastOrthographicSize + targetOrthographicSize, timer / zoomTime);
                }

                else
                {
                    mainCamera.orthographicSize = Mathf.Lerp(lastOrthographicSize, targetOrthographicSize, timer / zoomTime);
                }
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            if (add)
            {
                mainCamera.orthographicSize = lastOrthographicSize + targetOrthographicSize;
            }

            else
            {
                mainCamera.orthographicSize = targetOrthographicSize;
            }
        }

        private void OnSpotting(OnSpotting evt)
        {
            if (cameraPanCoroutine != null)
            {
                StopCoroutine(cameraPanCoroutine);
            }

            isSpotting = true;

            cameraPanCoroutine = StartCoroutine(CameraPan(evt.Target.position, evt.MoveTime));
        }

        private void OnSpottingEnd(OnSpottingEnd evt)
        {
            if (cameraPanCoroutine != null)
            {
                StopCoroutine(cameraPanCoroutine);
            }

            isSpotting = false;

            cameraPanCoroutine = StartCoroutine(CameraPan(originalPosition, evt.MoveTime));
        }
    }
}