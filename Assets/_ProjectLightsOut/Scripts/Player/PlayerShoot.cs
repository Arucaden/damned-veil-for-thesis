using UnityEngine;
using System;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;

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
                
        // ========================

        private void Awake()
        {
            bulletCollider = bulletPrefab.GetComponent<CircleCollider2D>();
        }

        private void OnEnable()
        {
            EventManager.AddListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
        }

        private void OnPlayerEnableShooting(OnPlayerEnableShooting evt)
        {
            IsFiringEnabled = evt.IsEnabled;
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

        private void DrawLaser()
        {
            if (!isFiringEnabled)
            {
                lineRenderer.enabled = false;
            }

            else
            {
                lineRenderer.enabled = true;
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
        }

        private void Aim()
        {
            if (!isFiringEnabled) return;
            
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

            if (!isFiringEnabled) return;

            OnShoot?.Invoke();

            EventManager.Broadcast(new OnPlaySFX("Cast"));

            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.GetComponent<Projectile>().SetDirection(bulletSpawnPoint.up * bulletSpeed);

            Bullets--;
            EventManager.Broadcast(new OnProjectileShoot(bullets));
        }
    }
}