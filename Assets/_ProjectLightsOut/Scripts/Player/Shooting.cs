using UnityEngine;
using System;

namespace ProjectLightsOut.Gameplay
{
    public class Shooting : MonoBehaviour
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
        [SerializeField] private float bulletSpeed = 10f;
        [SerializeField] private LineRenderer lineRenderer;
        private bool isFiringEnabled;
        public bool IsFiringEnabled {
            get => isFiringEnabled;
            private set { isFiringEnabled = value; OnFiringEnabled?.Invoke(isFiringEnabled); }
        }

        public Action<bool> OnFiringEnabled;
        private bool reloading;
        public Action<bool> OnReloading;
        public bool Reloading {
            get => reloading;
            private set { reloading = value; OnReloading?.Invoke(reloading); }
        }
                
        // ========================

        private void Awake()
        {
            bulletCollider = bulletPrefab.GetComponent<CircleCollider2D>();
        }

        private void Start()
        {
            IsFiringEnabled = true;
        }

        private void Update()
        {
            if (!isFiringEnabled) return;

            Aim();
            DrawLaser();
            GetInput();
        }

        private void DrawLaser()
        {
            if (reloading)
            {
                lineRenderer.enabled = false;
            }

            else
            {
                lineRenderer.enabled = true;
            }

            float radiusInWorldSpace = bulletCollider.radius * Mathf.Max(bulletPrefab.transform.lossyScale.x, bulletPrefab.transform.lossyScale.y);

            Vector3 leftRayOrigin = bulletSpawnPoint.position + bulletSpawnPoint.up * -radiusInWorldSpace;
            Vector3 rightRayOrigin = bulletSpawnPoint.position + bulletSpawnPoint.up * radiusInWorldSpace;

            LayerMask layerMask = 1 << LayerMask.NameToLayer("Projectile");
            layerMask = ~layerMask;
            RaycastHit2D hit = Physics2D.Raycast(leftRayOrigin, bulletSpawnPoint.up, Mathf.Infinity, layerMask);
            RaycastHit2D hit2 = Physics2D.Raycast(rightRayOrigin, bulletSpawnPoint.up, Mathf.Infinity, layerMask);

            if (hit.distance < hit2.distance)
            {
                lineRenderer.SetPosition(0, bulletSpawnPoint.position);
                lineRenderer.SetPosition(1, hit.point);
            }

            else
            {
                lineRenderer.SetPosition(0, bulletSpawnPoint.position);
                lineRenderer.SetPosition(1, hit2.point);
            }
        }

        private void Aim()
        {
            Vector2 direction = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;

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

            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.GetComponent<Projectile>().SetDirection(bulletSpawnPoint.up * bulletSpeed);

            Bullets--;
        }
    }
}