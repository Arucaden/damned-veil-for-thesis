using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using ProjectLightsOut.Riddles;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class MimicBoss : BossBase<MimicBoss>
    {
        [Header("Mimic Settings")]
        [SerializeField] private float bulletSpeed = 10f;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private SimplePool bulletPool;
        [SerializeField] private Transform bulletSpawnPoint;
        
        [Header("Riddle Integration")]
        [SerializeField] private PillarsRiddle pillarsRiddle;
        
        [Header("Phase Transition")]
        [SerializeField] private bool startInPhase2 = false;
        [SerializeField] private string phase2SceneName;
        [SerializeField] private float blackoutDuration = 2f;
        [Tooltip("How long to hold the black screen before loading Phase 2. Adds dramatic pause.")]
        [SerializeField] private float blackoutHoldDuration = 4f;

        [Header("Vulnerability Settings")]
        [Tooltip("Time (in seconds) the boss remains vulnerable after solving the puzzle before the puzzle resets.")]
        [SerializeField] private float vulnerabilityDuration = 10f;

        [Header("Health Regen Settings")]
        [Tooltip("How often (in seconds) the boss passively regenerates health when the puzzle is NOT solved.")]
        [SerializeField] private float passiveRegenInterval = 10f;
        [Tooltip("Percentage of Max Health restored every passive regen tick.")]
        [SerializeField] private float passiveRegenPercentage = 0.1f;

        [Header("Under Asset Settings")]
        [SerializeField] private GameObject underAssetObject;
        [SerializeField] private SpriteRenderer underAssetRenderer;
        [SerializeField] private float invulnerableOpacity = 1f;
        [SerializeField] private float vulnerableOpacity = 0.3f;
        [SerializeField] private float opacityChangeSpeed = 2f;
        [SerializeField] private float invulnerableRotationSpeed = 180f;
        [SerializeField] private float rotationAcceleration = 360f;

        public PillarsRiddle PillarsRiddle => pillarsRiddle;
        public string Phase2SceneName => phase2SceneName;
        public float BlackoutDuration => blackoutDuration;
        public float BlackoutHoldDuration => blackoutHoldDuration;
        public float VulnerabilityDuration => vulnerabilityDuration;
        public bool IsVulnerable { get; set; }

        private PlayerShoot playerShoot;
        private IMimicBossPhase currentMimicPhase;
        private Coroutine passiveRegenCoroutine;
        private Coroutine assetAnimationCoroutine;

        protected override void OnEnable()
        {
            base.OnEnable();
            passiveRegenCoroutine = StartCoroutine(PassiveRegenRoutine());
            assetAnimationCoroutine = StartCoroutine(AssetAnimationRoutine());
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (playerShoot != null)
            {
                playerShoot.OnShoot -= HandlePlayerShoot;
            }
            if (passiveRegenCoroutine != null)
            {
                StopCoroutine(passiveRegenCoroutine);
            }
            if (assetAnimationCoroutine != null)
            {
                StopCoroutine(assetAnimationCoroutine);
            }
        }

        private IEnumerator PassiveRegenRoutine()
        {
            while (true)
            {
                float timer = 0;
                while (timer < passiveRegenInterval)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                if (!IsVulnerable && health < MaxHealth && currentMimicPhase != null && !(currentMimicPhase is MimicBossDeadPhase))
                {
                    int healAmount = Mathf.Max(1, Mathf.RoundToInt(MaxHealth * passiveRegenPercentage));
                    health = Mathf.Min(MaxHealth, health + healAmount);
                    OnBossHealed?.Invoke();
                    Debug.Log($"[MimicBoss] Passive regen! Healed for {healAmount}. Current health: {health}");
                }
            }
        }

        private IEnumerator AssetAnimationRoutine()
        {
            float currentRotationSpeed = 0f;

            while (true)
            {
                yield return null;

                if (underAssetObject == null || !underAssetObject.activeInHierarchy) continue;

                float targetOpacity = IsVulnerable ? vulnerableOpacity : invulnerableOpacity;
                float targetRotationSpeed = IsVulnerable ? 0f : invulnerableRotationSpeed;

                if (underAssetRenderer != null)
                {
                    Color c = underAssetRenderer.color;
                    c.a = Mathf.MoveTowards(c.a, targetOpacity, opacityChangeSpeed * Time.deltaTime);
                    underAssetRenderer.color = c;
                }

                currentRotationSpeed = Mathf.MoveTowards(currentRotationSpeed, targetRotationSpeed, rotationAcceleration * Time.deltaTime);

                underAssetObject.transform.Rotate(0, 0, currentRotationSpeed * Time.deltaTime);
            }
        }

        public override void SetPhase(IBossPhase<MimicBoss> newPhase)
        {
            currentMimicPhase = newPhase as IMimicBossPhase;
            base.SetPhase(newPhase);

            if (currentMimicPhase is MimicBossPhase1 || currentMimicPhase is MimicBossPhase2)
            {
                if (underAssetObject != null && !underAssetObject.activeSelf)
                {
                    if (underAssetRenderer != null)
                    {
                        Color c = underAssetRenderer.color;
                        c.a = 0f;
                        underAssetRenderer.color = c;
                    }
                    underAssetObject.SetActive(true);
                }
            }
            else
            {
                if (underAssetObject != null) underAssetObject.SetActive(false);
            }
        }

        protected override IBossPhase<MimicBoss> CreateEntrancePhase() => new MimicBossEntrancePhase();
        protected override IBossPhase<MimicBoss> CreateDeadPhase() => new MimicBossDeadPhase();

        protected override IEnumerator OnEntranceComplete()
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerShoot = playerObj.GetComponentInChildren<PlayerShoot>();
                if (playerShoot != null)
                {
                    playerShoot.OnShoot += HandlePlayerShoot;
                    Debug.Log("[MimicBoss] Successfully subscribed to PlayerShoot.OnShoot.");
                }
                else
                {
                    Debug.LogWarning("[MimicBoss] Player object found, but PlayerShoot component is missing!");
                }
            }
            else
            {
                Debug.LogWarning("[MimicBoss] Player object not found with tag 'Player'!");
            }

            if (startInPhase2)
            {
                SetPhase(new MimicBossPhase2());
            }
            else
            {
                SetPhase(new MimicBossPhase1());
            }
            yield return null;
        }

        private void HandlePlayerShoot()
        {
            if (currentMimicPhase == null)
            {
                Debug.Log("[MimicBoss] Ignored shoot: currentMimicPhase is null");
                return;
            }
            
            if (currentMimicPhase is MimicBossTransitionPhase)
            {
                Debug.Log("[MimicBoss] Ignored shoot: currently in TransitionPhase");
                return;
            }

            Vector2 playerAimDir = playerShoot.transform.up;
            Vector2 mimicAimDir = -playerAimDir;
            
            FireBullet(mimicAimDir);
        }

        private void FireBullet(Vector2 direction)
        {
            if (bulletPrefab == null)
            {
                Debug.LogWarning("[MimicBoss] FireBullet failed: bulletPrefab is null!");
                return;
            }
            if (bulletSpawnPoint == null)
            {
                Debug.LogWarning("[MimicBoss] FireBullet failed: bulletSpawnPoint is null!");
                return;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            GameObject bulletObj = bulletPool != null
                ? bulletPool.Get(bulletSpawnPoint.position, rotation)
                : Instantiate(bulletPrefab, bulletSpawnPoint.position, rotation);

            Projectile proj = bulletObj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.ResetProjectile();
                proj.ParentPool = bulletPool;
                proj.SetDirection(direction.normalized * bulletSpeed);
                proj.IsEnemyProjectile = true;
            }

            EventManager.Broadcast(new OnProjectileShoot(0));
            EventManager.Broadcast(new OnPlaySFX("EnemyShoot"));
        }

        public void HealToFull()
        {
            health = MaxHealth;
            OnBossHealed?.Invoke();
        }
    }

    public interface IMimicBossPhase
    {
    }
}
