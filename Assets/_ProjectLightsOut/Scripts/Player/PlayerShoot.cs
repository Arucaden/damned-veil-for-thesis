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
        [SerializeField] private SimplePool bulletPool;
        private CircleCollider2D bulletCollider;
        [SerializeField] private Transform bulletSpawnPoint;
        [SerializeField] private Transform laserSpawnPoint;
        [SerializeField] private float bulletSpeed = 10f;
        
        public GameObject BulletPrefab => bulletPrefab;
        public Transform LaserSpawnPoint => laserSpawnPoint;
        private bool isFiringEnabled = false;
        public bool IsFiringEnabled {
            get => isFiringEnabled;
            private set { isFiringEnabled = value; OnFiringEnabled?.Invoke(isFiringEnabled); }
        }
        public Vector2 Direction => transform.up;
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
            // Aiming and laser drawing is now handled by PlayerLaserAimer.cs
            GetInput();
        }

        private void OnGrantReload(OnGrantReload evt)
        {
            reloadCoroutine = StartCoroutine(ReloadCoroutine());
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

            GameObject bullet = bulletPool != null
                ? bulletPool.Get(bulletSpawnPoint.position, bulletSpawnPoint.rotation)
                : Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

            Projectile proj = bullet.GetComponent<Projectile>();
            proj.ResetProjectile();
            proj.ParentPool = bulletPool;
            proj.SetDirection(bulletSpawnPoint.up * bulletSpeed);

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