using System;
using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Effects;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class AwakenedAzalethBoss : BossBase<AwakenedAzalethBoss>
    {
        [Header("Waves")]
        [SerializeField] private List<WaveDataSO> firstPhaseWaves;
        [SerializeField] private List<WaveDataSO> secondPhaseWaves;

        [Header("Shield")]
        [SerializeField] private ShieldEffect shieldEffect;

        [Header("Teleport")]
        [SerializeField] private List<Transform> teleportPoints;
        [SerializeField] private float teleportCooldownMax = 5f;

        [Header("Wave Spawn")]
        [SerializeField] private float maxSpawnCooldown = 4f;

        [Header("Void Orb")]
        [Tooltip("Prefab that has a VoidOrb component")]
        [SerializeField] private GameObject voidOrbPrefab;
        [SerializeField] private SimplePool voidOrbPool;

        [Tooltip("Possible spawn positions for void orbs around the map edges")]
        [SerializeField] private List<Transform> voidOrbSpawnPoints;

        [Tooltip("Phase 1: seconds between each orb spawn")]
        [SerializeField] private float phase1OrbInterval = 3f;

        [Tooltip("Phase 2: seconds between each orb spawn (faster)")]
        [SerializeField] private float phase2OrbInterval = 1.5f;

        [Tooltip("Maximum orbs alive at one time before oldest is wiped")]
        [SerializeField] private int maxActiveOrbs = 5;

        [Header("Blackout")]
        [Tooltip("How long the fade-to-dark takes when Phase 2 begins")]
        [SerializeField] private float blackoutFadeTime = 2f;

        public List<WaveDataSO> FirstPhaseWaves  => firstPhaseWaves;
        public List<WaveDataSO> SecondPhaseWaves => secondPhaseWaves;
        public ShieldEffect     ShieldEffect     => shieldEffect;
        public float            BlackoutFadeTime => blackoutFadeTime;
        public float            Phase1OrbInterval => phase1OrbInterval;
        public float            Phase2OrbInterval => phase2OrbInterval;

        private List<ActiveWaveData> activeWaves   = new List<ActiveWaveData>();
        private List<Enemy>          activeEnemies = new List<Enemy>();
        private float spawnCooldown;
        private bool  isSpawnNeeded;
        private float teleportCooldown;

        private readonly List<VoidOrb> activeOrbs = new List<VoidOrb>();
        private Coroutine orbSpawnCoroutine;

        private Transform playerTransform;
        private PlayerShoot playerShoot;

        private IAwakenedAzalethPhase currentAwakenedPhase;

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.AddListener<OnBossBuff>(HandleBossBuff);
            EventManager.AddListener<OnVoidOrbHitPlayer>(HandleVoidOrbHitPlayer);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.RemoveListener<OnBossBuff>(HandleBossBuff);
            EventManager.RemoveListener<OnVoidOrbHitPlayer>(HandleVoidOrbHitPlayer);
        }


        public override void SetPhase(IBossPhase<AwakenedAzalethBoss> newPhase)
        {
            currentAwakenedPhase = newPhase as IAwakenedAzalethPhase;
            base.SetPhase(newPhase);
        }

        protected override IBossPhase<AwakenedAzalethBoss> CreateEntrancePhase() =>
            new AwakenedAzalethEntrancePhase();

        protected override IBossPhase<AwakenedAzalethBoss> CreateDeadPhase() =>
            new AwakenedAzalethDeadPhase();

        protected override IEnumerator OnEntranceComplete()
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerShoot     = playerObj.GetComponentInChildren<PlayerShoot>();
            }

            LevelManager.SpawnEnemyWave(firstPhaseWaves[0]);
            var waveData = new ActiveWaveData
            {
                waveData   = firstPhaseWaves[0],
                enemyCount = firstPhaseWaves[0].Enemies.Count
            };
            activeWaves.Add(waveData);

            spawnCooldown  = maxSpawnCooldown;
            teleportCooldown = teleportCooldownMax;

            SetPhase(new AwakenedAzalethPhase1());
            yield return null;
        }

        protected override void HandleEnemyRegister(OnEnemyRegister e)
        {
            activeEnemies.Add(e.Enemy);
        }

        protected override void HandleEnemyDead(OnEnemyDead e)
        {
            activeEnemies.Remove(e.Enemy);
            ActiveWaveData data = FindActiveWaveByEnemy(e.Enemy);
            if (data == null) return;

            data.enemyCount--;
            if (data.enemyCount <= 0)
                activeWaves.Remove(data);
        }

        private ActiveWaveData FindActiveWaveByEnemy(Enemy enemy)
        {
            foreach (var data in activeWaves)
                if (data.waveData == enemy.WaveData)
                    return data;
            return null;
        }

        public void TrySpawnWave(List<WaveDataSO> sourceWaves)
        {
            if (activeWaves.Count == 0)
                isSpawnNeeded = true;

            if (spawnCooldown <= 0)
            {
                List<WaveDataSO> cache = new List<WaveDataSO>(sourceWaves);
                cache.RemoveAll(x => activeWaves.Exists(y => y.waveData == x));

                if (cache.Count == 0)
                    cache = new List<WaveDataSO>(sourceWaves);

                int idx = UnityEngine.Random.Range(0, cache.Count);
                LevelManager.SpawnEnemyWave(cache[idx]);

                var waveEntry = new ActiveWaveData
                {
                    waveData   = cache[idx],
                    enemyCount = cache[idx].Enemies.Count
                };
                activeWaves.Add(waveEntry);
                isSpawnNeeded = false;
                spawnCooldown = maxSpawnCooldown;
            }
        }

        public void TickSpawnCooldown()
        {
            if (spawnCooldown > 0 && isSpawnNeeded)
                spawnCooldown -= Time.deltaTime;
        }

        public void TickTeleportCooldown()
        {
            if (teleportCooldown > 0)
            {
                teleportCooldown -= Time.deltaTime;
            }
            else
            {
                teleportCooldown = 9999f;
                StartCoroutine(Teleport(1f));
            }
        }

        public IEnumerator Teleport(float delay)
        {
            IsHittable = false;
            BossAnimator.SetTrigger("teleport");
            shieldEffect.DeactivateShield();

            int idx = UnityEngine.Random.Range(0, teleportPoints.Count - 1);
            if (spawnEffectPool != null)
            {
                GameObject fx = spawnEffectPool.Get(teleportPoints[idx].position, Quaternion.identity);
                spawnEffectPool.Return(fx, 1f);
            }
            else
            {
                Instantiate(SpawnEffect, teleportPoints[idx].position, Quaternion.identity);
            }

            yield return new WaitForSeconds(delay);
            transform.position = teleportPoints[idx].position;
            teleportCooldown   = teleportCooldownMax + UnityEngine.Random.Range(0, 3);
            IsHittable         = true;
        }

        public void StartOrbSpawnLoop(float interval)
        {
            StopOrbSpawnLoop();
            orbSpawnCoroutine = StartCoroutine(OrbSpawnLoop(interval));
        }

        public void StopOrbSpawnLoop()
        {
            if (orbSpawnCoroutine != null)
            {
                StopCoroutine(orbSpawnCoroutine);
                orbSpawnCoroutine = null;
            }
        }

        public void WipeAllOrbs()
        {
            StopOrbSpawnLoop();

            foreach (VoidOrb orb in activeOrbs)
            {
                if (orb != null) orb.Deactivate();
            }
            activeOrbs.Clear();
        }

        private IEnumerator OrbSpawnLoop(float interval)
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);

                if (activeOrbs.Count >= maxActiveOrbs && activeOrbs.Count > 0)
                {
                    VoidOrb oldest = activeOrbs[0];
                    activeOrbs.RemoveAt(0);
                    if (oldest != null) oldest.Deactivate();
                }

                SpawnOrb();
            }
        }

        private void SpawnOrb()
        {
            if (voidOrbSpawnPoints == null || voidOrbSpawnPoints.Count == 0) return;
            if (playerTransform == null) return;

            int spawnIdx = UnityEngine.Random.Range(0, voidOrbSpawnPoints.Count);
            Vector3 spawnPos = voidOrbSpawnPoints[spawnIdx].position;

            if (playerShoot != null)
                playerShoot.Ricochets = Mathf.Max(0, playerShoot.Ricochets - 1);

            GameObject orbObj = voidOrbPool != null
                ? voidOrbPool.Get(spawnPos, Quaternion.identity)
                : Instantiate(voidOrbPrefab, spawnPos, Quaternion.identity);

            VoidOrb orb = orbObj.GetComponent<VoidOrb>();
            if (orb == null)
            {
                Debug.LogError("[AwakenedAzalethBoss] VoidOrb prefab is missing VoidOrb component!");
                return;
            }

            orb.ParentPool = voidOrbPool;
            orb.Activate(playerTransform);
            activeOrbs.Add(orb);
        }

        public void GivePlayerVoidProjectile()
        {
            if (playerShoot != null)
                playerShoot.GiveVoidProjectile();
        }


        private void HandleBossBuff(OnBossBuff e)
        {
            currentAwakenedPhase?.OnBuff(this, e);
        }

        private void HandleVoidOrbHitPlayer(OnVoidOrbHitPlayer e)
        {
            StartCoroutine(InstantGameOver());
        }

        private System.Collections.IEnumerator InstantGameOver()
        {
            // Disable shooting immediately
            EventManager.Broadcast(new OnPlayerEnableShooting(false));
            EventManager.Broadcast(new OnPlayBGM("GameOver", fadeIn: 1f));
            yield return new WaitForSeconds(1f);
            EventManager.Broadcast(new OnGameOver());
        }
    }


    public interface IAwakenedAzalethPhase
    {
        void OnBuff(AwakenedAzalethBoss boss, OnBossBuff e);
    }
}
