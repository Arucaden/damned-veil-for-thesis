using UnityEngine;
using System;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using System.Collections;

namespace ProjectLightsOut.Gameplay
{
    public class PlayerShoot : MonoBehaviour
    {
        [SerializeField] private int bullets = 6;

        public int Bullets { 
            get => bullets;
            set { bullets = value; OnBulletChange?.Invoke(bullets); }
        }
        public Action<int> OnBulletChange;

        [SerializeField] private int ricochets = 4;
        public int Ricochets {
            get => ricochets;
            set { ricochets = value; }
        }

        [SerializeField] private GameObject bulletPrefab;
        private CircleCollider2D bulletCollider;
        [SerializeField] private Transform bulletSpawnPoint;
        [SerializeField] private Transform laserSpawnPoint;
        [SerializeField] private float bulletSpeed = 10f;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private LineRenderer lineRendererRicochet;
        [SerializeField] private Gradient reloadGradient;
        private Gradient lineRendererOriginalColor;
        private bool isFiringEnabled = false;
        public bool IsFiringEnabled {
            get => isFiringEnabled;
            private set { isFiringEnabled = value; OnFiringEnabled?.Invoke(isFiringEnabled); }
        }
        private Vector2 direction;
        public Vector2 Direction {
            get => direction;
            private set { direction = value; }
        }

        public Action<bool> OnFiringEnabled;
        private bool reloading;
        public Action<bool> OnReloading;
        public bool Reloading {
            get => reloading;
            private set { reloading = value; OnReloading?.Invoke(reloading); }
        }
        public Action OnShoot;
        private Coroutine reloadCoroutine;
        private bool wasReloadingWhenPaused = false;
                
        //========================

        private void Awake()
        {
            bulletCollider = bulletPrefab.GetComponent<CircleCollider2D>();
            lineRendererOriginalColor = lineRenderer.colorGradient;
        }

        private void OnEnable()
        {
            EventManager.AddListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
            EventManager.AddListener<OnGrantReload>(OnGrantReload);
            EventManager.AddListener<OnTriggerLevelComplete>(OnTriggerLevelComplete);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
            EventManager.RemoveListener<OnGrantReload>(OnGrantReload);
            EventManager.RemoveListener<OnTriggerLevelComplete>(OnTriggerLevelComplete);
        }

        private void OnTriggerLevelComplete(OnTriggerLevelComplete evt)
        {
            if (reloadCoroutine != null)
            {
                StopCoroutine(reloadCoroutine);
            }
        }

        private void OnPlayerEnableShooting(OnPlayerEnableShooting evt)
        {
            IsFiringEnabled = evt.IsEnabled;

            if (reloadCoroutine != null && !IsFiringEnabled)
            {
                wasReloadingWhenPaused = reloading; // Remember if we were reloading
                StopCoroutine(reloadCoroutine);
            }
            else if (IsFiringEnabled && wasReloadingWhenPaused)
            {
                // If we were reloading when paused, resume the reload
                wasReloadingWhenPaused = false;
                reloadCoroutine = StartCoroutine(ReloadCoroutine(bullets));
            }
        }

        private void Start()
        {
            try
            {
                bullets = LevelManager.LevelData.Bullets;
                EventManager.Broadcast(new OnBulletReload(LevelManager.LevelData.Bullets));
            }

            catch (NullReferenceException)
            {
                bullets = 6;
                EventManager.Broadcast(new OnBulletReload(bullets));
            }
        }

        private void Update()
        {
            Aim();
            DrawLaser();
            GetInput();
        }

        private void OnGrantReload(OnGrantReload evt)
        {
            reloadCoroutine = StartCoroutine(ReloadCoroutine());

            lineRenderer.colorGradient = reloadGradient;
            lineRendererRicochet.colorGradient = reloadGradient;
        }

        private IEnumerator ReloadCoroutine(int bulletsToReload = 6)
        {
            reloading = true;
            float duration = 2f;
            int startingBulletCount = Bullets;
            int targetBulletCount = Mathf.Min(startingBulletCount + bulletsToReload, LevelManager.LevelData.Bullets);
            float interval = duration / bulletsToReload;

            for (int i = startingBulletCount; i < targetBulletCount; i++)
            {
                yield return new WaitForSecondsRealtime(interval); // Use realtime to work during pause
                Bullets++;
                EventManager.Broadcast(new OnBulletReload(1));
            }

            reloading = false;

            lineRenderer.colorGradient = lineRendererOriginalColor;
            lineRendererRicochet.colorGradient = lineRendererOriginalColor;
        }

        private void DrawLaser()
        {
            if (!isFiringEnabled)
            {
                lineRenderer.enabled = false;
                lineRendererRicochet.enabled = false;
            }

            else
            {
                lineRenderer.enabled = true;
                lineRendererRicochet.enabled = true;
            }

            float radiusInWorldSpace = bulletCollider.radius * Mathf.Max(bulletPrefab.transform.lossyScale.x, bulletPrefab.transform.lossyScale.y);

            Vector3 leftRayOrigin = laserSpawnPoint.position + laserSpawnPoint.right * radiusInWorldSpace;
            Vector3 rightRayOrigin = laserSpawnPoint.position - laserSpawnPoint.right * radiusInWorldSpace;

            Debug.DrawRay(leftRayOrigin, laserSpawnPoint.up * 100, Color.green);
            Debug.DrawRay(rightRayOrigin, laserSpawnPoint.up * 100, Color.green);

            LayerMask layerMask = 1 << LayerMask.NameToLayer("Ignore Laser") | 1 << LayerMask.NameToLayer("Projectile");
            layerMask = ~layerMask;
            RaycastHit2D hitLeft = Physics2D.Raycast(leftRayOrigin, laserSpawnPoint.up, Mathf.Infinity, layerMask);
            RaycastHit2D hitRight = Physics2D.Raycast(rightRayOrigin, laserSpawnPoint.up, Mathf.Infinity, layerMask);

            if (hitLeft.distance < hitRight.distance)
            {
                lineRenderer.SetPosition(0, laserSpawnPoint.position);
                lineRenderer.SetPosition(1, hitLeft.point);
            }

            else
            {
                lineRenderer.SetPosition(0, laserSpawnPoint.position);
                lineRenderer.SetPosition(1, hitRight.point);
            }

            DrawRicochet(direction, hitLeft, hitRight);
        }

        private void DrawRicochet(Vector3 direction, RaycastHit2D hitLeft, RaycastHit2D hitRight)
        {
            if (hitLeft.distance < hitRight.distance)
            {
                Vector2 ricochetDirection = Vector2.Reflect(direction, hitLeft.normal);
                Debug.DrawRay(hitLeft.point, ricochetDirection * 100, Color.green);
                
                lineRendererRicochet.SetPosition(0, hitLeft.point);
                lineRendererRicochet.SetPosition(1, hitLeft.point + (Vector2)ricochetDirection * 0.1f);
            }

            else
            {
                Vector2 ricochetDirection = Vector2.Reflect(direction, hitRight.normal);
                Debug.DrawRay(hitRight.point, ricochetDirection * 100, Color.green);

                lineRendererRicochet.SetPosition(0, hitRight.point);
                lineRendererRicochet.SetPosition(1, hitRight.point + (Vector2)ricochetDirection * 0.1f);
            }
        }

        private void Aim()
        {
            direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;

            transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        }
        
        private void GetInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            if (bullets <= 0)
            {
                return;
            }

            if (reloading) return;

            if (!isFiringEnabled) return;

            OnShoot?.Invoke();

            EventManager.Broadcast(new OnPlaySFX("Cast"));

            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.GetComponent<Projectile>().SetDirection(bulletSpawnPoint.up * bulletSpeed);

            Bullets--;
            EventManager.Broadcast(new OnProjectileShoot(bullets));

            if (bullets == 0)
            {
                if (LevelManager.LevelData.IsBossLevel)
                {
                    EventManager.Broadcast(new OnGrantReload(LevelManager.LevelData.Bullets));
                }
            }
        }
    }
}