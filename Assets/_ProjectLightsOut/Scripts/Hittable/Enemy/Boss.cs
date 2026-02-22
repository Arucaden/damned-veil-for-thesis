using System;
using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Effects;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    [Serializable]
    public class ActiveWaveData
    {
        public WaveDataSO waveData;
        public int enemyCount;
    }

    /// <summary>
    /// Boss enemy — thin coordinator that delegates behavior to the current IBossPhase.
    /// Phases: Idle → Phase1 → Stun → Phase2 → Dead.
    /// </summary>
    public class Boss : Enemy
    {
        // --- Serialized configuration ---
        [SerializeField] private List<WaveDataSO> firstPhaseWaves;
        [SerializeField] private List<WaveDataSO> secondPhaseWaves;
        [SerializeField] private ShieldEffect shieldEffect;
        [SerializeField] private List<Transform> teleportPoints;
        [SerializeField] private float maxSpawnCooldown = 4f;
        [SerializeField] private float teleportCooldownMax = 5f;

        // --- Public accessors for phases ---
        public List<WaveDataSO> FirstPhaseWaves => firstPhaseWaves;
        public List<WaveDataSO> SecondPhaseWaves => secondPhaseWaves;
        public ShieldEffect ShieldEffect => shieldEffect;
        public Animator Animator => animator;
        [HideInInspector] public int MaxHealth;
        public Action OnBossDamaged;
        public Action OnBossHealed;

        // --- Wave tracking (shared across phases) ---
        private List<ActiveWaveData> activeWaves = new List<ActiveWaveData>();
        private List<Enemy> activeEnemies = new List<Enemy>();
        private float spawnCooldown = 4f;
        private bool isSpawnNeeded = false;

        // --- Teleport state ---
        private float teleportCooldown = 5f;

        // --- State machine ---
        private IBossPhase currentPhase;

        public void SetPhase(IBossPhase newPhase)
        {
            currentPhase?.Exit(this);
            currentPhase = newPhase;
            currentPhase.Enter(this);
        }

        // =================================================================
        // Unity lifecycle
        // =================================================================

        protected override void Start()
        {
            EventManager.Broadcast(new OnBossRegister(this));
            MaxHealth = health;
            SetPhase(new BossIdlePhase());
        }

        private void Update()
        {
            currentPhase?.UpdatePhase(this);
        }

        private void OnEnable()
        {
            EventManager.AddListener<OnReadyBoss>(HandleReadyBoss);
            EventManager.AddListener<OnEnemyRegister>(HandleEnemyRegister);
            EventManager.AddListener<OnEnemyDead>(HandleEnemyDead);
            EventManager.AddListener<OnBossBuff>(HandleBossBuff);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnReadyBoss>(HandleReadyBoss);
            EventManager.RemoveListener<OnEnemyRegister>(HandleEnemyRegister);
            EventManager.RemoveListener<OnEnemyDead>(HandleEnemyDead);
            EventManager.RemoveListener<OnBossBuff>(HandleBossBuff);
        }

        // =================================================================
        // Event handlers (delegate to current phase or manage shared state)
        // =================================================================

        private void HandleReadyBoss(OnReadyBoss e)
        {
            StartCoroutine(ReadyBossSequence());
        }

        private void HandleEnemyRegister(OnEnemyRegister e)
        {
            activeEnemies.Add(e.Enemy);
        }

        private void HandleEnemyDead(OnEnemyDead e)
        {
            activeEnemies.Remove(e.Enemy);
            ActiveWaveData activeWaveData = FindActiveWaveByEnemy(e.Enemy);

            if (activeWaveData == null) return;

            activeWaveData.enemyCount--;

            if (activeWaveData.enemyCount <= 0)
            {
                activeWaves.Remove(activeWaveData);
            }
        }

        private void HandleBossBuff(OnBossBuff e)
        {
            currentPhase?.OnBuff(this, e);
        }

        // =================================================================
        // OnHit override — delegates to current phase
        // =================================================================

        public override void OnHit(int multiplier, Action OnTargetHit)
        {
            currentPhase?.OnHit(this, multiplier, OnTargetHit);
        }

        // =================================================================
        // Helper methods called by phase classes
        // =================================================================

        /// <summary>
        /// Applies damage to the boss. Called by combat phases.
        /// </summary>
        public void ApplyDamage(int multiplier, Action OnTargetHit)
        {
            health--;
            OnDamaged?.Invoke(multiplier);
            OnTargetHit?.Invoke();
            OnBossDamaged?.Invoke();
        }

        /// <summary>
        /// Attempts to spawn a new wave from the given source list.
        /// Called by Phase1 (firstPhaseWaves) and Phase2 (secondPhaseWaves).
        /// </summary>
        public void TrySpawnWave(List<WaveDataSO> sourceWaves)
        {
            if (activeWaves.Count <= 1)
            {
                isSpawnNeeded = true;
            }

            if (spawnCooldown <= 0)
            {
                List<WaveDataSO> waveCache = new List<WaveDataSO>(sourceWaves);
                waveCache.RemoveAll(x => activeWaves.Exists(y => y.waveData == x));

                if (waveCache.Count == 0)
                {
                    waveCache = new List<WaveDataSO>(sourceWaves);
                }

                int random = UnityEngine.Random.Range(0, waveCache.Count);
                LevelManager.SpawnEnemyWave(waveCache[random]);
                ActiveWaveData activeWaveData = new ActiveWaveData();
                activeWaveData.waveData = waveCache[random];
                activeWaveData.enemyCount = waveCache[random].Enemies.Count;
                activeWaves.Add(activeWaveData);
                isSpawnNeeded = false;
                spawnCooldown = maxSpawnCooldown;
            }
        }

        /// <summary>
        /// Ticks the spawn cooldown timer. Called each frame by active combat phases.
        /// </summary>
        public void TickSpawnCooldown()
        {
            if (spawnCooldown > 0 && isSpawnNeeded)
            {
                spawnCooldown -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Ticks the teleport cooldown and triggers teleport when ready.
        /// Called each frame by Phase2.
        /// </summary>
        public void TickTeleportCooldown()
        {
            if (teleportCooldown > 0)
            {
                teleportCooldown -= Time.deltaTime;
            }
            else
            {
                StartCoroutine(Teleport(1f));
            }
        }

        /// <summary>
        /// Teleports the boss to a random teleport point.
        /// </summary>
        public IEnumerator Teleport(float delay)
        {
            IsHittable = false;
            animator.SetTrigger("teleport");
            shieldEffect.DeactivateShield();
            int random = UnityEngine.Random.Range(0, teleportPoints.Count - 1);
            if (spawnEffectPool != null)
            {
                GameObject fx = spawnEffectPool.Get(teleportPoints[random].position, Quaternion.identity);
                spawnEffectPool.Return(fx, 1f);
            }
            else
            {
                Instantiate(SpawnEffect, teleportPoints[random].position, Quaternion.identity);
            }
            yield return new WaitForSeconds(delay);
            transform.position = teleportPoints[random].position;
            teleportCooldown = teleportCooldownMax + UnityEngine.Random.Range(0, 3);
            IsHittable = true;
        }

        // =================================================================
        // Intro sequence (transitions from Idle → Phase1)
        // =================================================================

        private IEnumerator ReadyBossSequence()
        {
            EventManager.Broadcast(new OnSpotting(transform, 2f));

            yield return new WaitForSeconds(3f);

            EventManager.Broadcast(new OnSpottingEnd(1f));
            EventManager.Broadcast(new OnZoomEnd(1f));

            yield return new WaitForSeconds(1f);

            EventManager.Broadcast(new OnBossReady(this));

            LevelManager.SpawnEnemyWave(firstPhaseWaves[0]);
            ActiveWaveData activeWaveData = new ActiveWaveData();
            activeWaveData.waveData = firstPhaseWaves[0];
            activeWaveData.enemyCount = firstPhaseWaves[0].Enemies.Count;
            activeWaves.Add(activeWaveData);

            yield return new WaitForSeconds(4.5f);

            EventManager.Broadcast(new OnPlayerEnableShooting(true));

            SetPhase(new BossPhase1());
        }

        // =================================================================
        // Internal helpers
        // =================================================================

        private ActiveWaveData FindActiveWaveByEnemy(Enemy enemy)
        {
            foreach (ActiveWaveData data in activeWaves)
            {
                if (data.waveData == enemy.WaveData)
                {
                    return data;
                }
            }
            return null;
        }
    }
}