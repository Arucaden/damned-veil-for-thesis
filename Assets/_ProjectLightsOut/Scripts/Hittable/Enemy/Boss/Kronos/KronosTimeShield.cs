using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    /// <summary>
    /// Kronos's time shield — manages the freeze zone and orbiting orbs.
    /// The shield itself (this GameObject + CircleCollider2D trigger) is FIXED.
    /// The orbs orbit independently around the boss.
    /// 
    /// Setup:
    /// 1. Create child GameObject under Kronos
    /// 2. Add CircleCollider2D (isTrigger = true) — freeze zone
    /// 3. Add KronosOrb children at desired orbit radius
    /// </summary>
    public class KronosTimeShield : MonoBehaviour
    {
        [Header("Freeze Settings")]
        [Tooltip("Seconds before a frozen bullet is erased")]
        [SerializeField] private float bulletEraseTime = 3f;

        [Header("Orbit Settings")]
        [Tooltip("Orbit speed in degrees per second")]
        [SerializeField] private float orbOrbitSpeed = 90f;
        [Tooltip("Orbit radius from boss center")]
        [SerializeField] private float orbOrbitRadius = 2f;

        [Header("Shield Cooldown")]
        [Tooltip("Seconds before shield reactivates and orbs respawn")]
        [SerializeField] private float shieldRespawnCooldown = 5f;

        [Header("Orb References")]
        [SerializeField] private List<KronosOrb> orbs = new List<KronosOrb>();

        private List<Projectile> frozenBullets = new List<Projectile>();
        private List<Coroutine> eraseTimers = new List<Coroutine>();
        private int orbsRemaining;
        private bool isShieldActive;
        private float orbitAngle;
        private Collider2D freezeZoneCollider;

        public Action OnShieldDown;

        public Action OnShieldUp;

        public float BulletEraseTime
        {
            get => bulletEraseTime;
            set => bulletEraseTime = value;
        }

        public float ShieldRespawnCooldown
        {
            get => shieldRespawnCooldown;
            set => shieldRespawnCooldown = value;
        }

        public bool IsShieldActive => isShieldActive;
        public int FrozenBulletCount => frozenBullets.Count;
        public int OrbsRemaining => orbsRemaining;

        private void Awake()
        {
            freezeZoneCollider = GetComponent<Collider2D>();
        }

        public void Activate()
        {
            isShieldActive = true;
            if (freezeZoneCollider != null) freezeZoneCollider.enabled = true;
            orbsRemaining = orbs.Count;

            // Distribute orbs evenly around the orbit
            for (int i = 0; i < orbs.Count; i++)
            {
                orbs[i].Respawn();
                orbs[i].OnOrbDestroyed = HandleOrbDestroyed;
            }

            PositionOrbs();
        }

        public void Deactivate()
        {
            isShieldActive = false;
            if (freezeZoneCollider != null) freezeZoneCollider.enabled = false;
        }

        private void Update()
        {
            if (!isShieldActive) return;

            // Orbit the orbs around the boss center
            orbitAngle += orbOrbitSpeed * Time.deltaTime;
            PositionOrbs();
        }

        private void PositionOrbs()
        {
            float angleStep = 360f / orbs.Count;

            for (int i = 0; i < orbs.Count; i++)
            {
                float angle = (orbitAngle + i * angleStep) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * orbOrbitRadius,
                    Mathf.Sin(angle) * orbOrbitRadius,
                    0f
                );
                orbs[i].transform.position = transform.position + offset;
            }
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isShieldActive) return;

            Projectile bullet = other.GetComponent<Projectile>();
            if (bullet != null && !bullet.IsFrozen)
            {
                FreezeBullet(bullet);
            }
        }

        private void FreezeBullet(Projectile bullet)
        {
            bullet.Freeze();
            frozenBullets.Add(bullet);
            Coroutine timer = StartCoroutine(EraseBulletAfterDelay(bullet, bulletEraseTime));
            eraseTimers.Add(timer);
        }

        private IEnumerator EraseBulletAfterDelay(Projectile bullet, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (bullet != null && bullet.IsFrozen)
            {
                frozenBullets.Remove(bullet);

                if (bullet.ParentPool != null)
                {
                    bullet.ResetProjectile();
                    bullet.ParentPool.Return(bullet.gameObject);
                }
                else
                {
                    Destroy(bullet.gameObject);
                }
            }
        }


        private void HandleOrbDestroyed()
        {
            orbsRemaining--;

            if (orbsRemaining <= 0)
            {
                isShieldActive = false;
                if (freezeZoneCollider != null) freezeZoneCollider.enabled = false;
                ReleaseAllBullets();
                OnShieldDown?.Invoke();

                StartCoroutine(ShieldRespawnSequence());
            }
        }

        private IEnumerator ShieldRespawnSequence()
        {
            yield return new WaitForSeconds(shieldRespawnCooldown);

            // Reactivate shield and respawn orbs
            Activate();
            OnShieldUp?.Invoke();
        }

        // --- Bullet Release / Erase ---

        /// <summary>
        /// Releases all frozen bullets — they continue on their original paths.
        /// </summary>
        public void ReleaseAllBullets()
        {
            foreach (var timer in eraseTimers)
            {
                if (timer != null) StopCoroutine(timer);
            }
            eraseTimers.Clear();

            foreach (var bullet in frozenBullets)
            {
                if (bullet != null && bullet.IsFrozen)
                {
                    bullet.Unfreeze();
                }
            }
            frozenBullets.Clear();
        }

        /// <summary>
        /// Erases all frozen bullets without releasing them.
        /// </summary>
        public void EraseAllBullets()
        {
            foreach (var timer in eraseTimers)
            {
                if (timer != null) StopCoroutine(timer);
            }
            eraseTimers.Clear();

            foreach (var bullet in frozenBullets)
            {
                if (bullet != null)
                {
                    if (bullet.ParentPool != null)
                    {
                        bullet.ResetProjectile();
                        bullet.ParentPool.Return(bullet.gameObject);
                    }
                    else
                    {
                        Destroy(bullet.gameObject);
                    }
                }
            }
            frozenBullets.Clear();
        }
    }
}
