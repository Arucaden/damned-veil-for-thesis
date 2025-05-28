using System;
using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
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

    public class Boss : Enemy
    {
        [SerializeField] private List<WaveDataSO> firstPhaseWaves;
        [SerializeField] private List<WaveDataSO> secondPhaseWaves;
        [SerializeField] private ShieldEffect shieldEffect;
        [SerializeField] private List<Transform> teleportPoints;
        private List<ActiveWaveData> activeWaves = new List<ActiveWaveData>();
        public Action OnBossDamaged;
        [HideInInspector] public int MaxHealth;
        private List<Enemy> activeEnemies = new List<Enemy>();
        private float spawnCooldown = 4f;
        [SerializeField] private float maxSpawnCooldown = 4f;
        private bool isBossReady = false;
        private bool isSpawnNeeded = false;
        public Action OnBossHealed;
        private bool isSecondPhase = false;
        private float teleportCooldown = 5f;
        private bool isShieldDisabled;
        [SerializeField] private float teleportCooldownMax = 5f;
        
        protected override void Start()
        {
            EventManager.Broadcast(new OnBossRegister(this));
            MaxHealth = health;
        }

        private void Update()
        {
            if (!IsHittable) return;

            TrySpawnWave();

            if (spawnCooldown > 0 && isSpawnNeeded)
            {
                spawnCooldown -= Time.deltaTime;
            }

            if (teleportCooldown > 0 && isSecondPhase)
            {
                teleportCooldown -= Time.deltaTime;
            }

            else if (teleportCooldown <= 0 && isSecondPhase)
            {
                StartCoroutine(Teleport(1f));
            }
        }

        private void OnEnable()
        {
            EventManager.AddListener<OnReadyBoss>(OnReadyBoss);
            EventManager.AddListener<OnEnemyRegister>(OnEnemyRegister);
            EventManager.AddListener<OnEnemyDead>(OnEnemyDead);
            EventManager.AddListener<OnBossBuff>(OnBossBuff);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnReadyBoss>(OnReadyBoss);
            EventManager.RemoveListener<OnEnemyRegister>(OnEnemyRegister);
            EventManager.RemoveListener<OnEnemyDead>(OnEnemyDead);
            EventManager.RemoveListener<OnBossBuff>(OnBossBuff);
        }

        private void OnBossBuff(OnBossBuff e)
        {
            if (e.buffType == BuffType.Health)
            {
                // if (health < MaxHealth)
                // {
                //     health += 1;
                // }

                OnBossHealed?.Invoke();
            }

            else if (e.buffType == BuffType.Shield)
            {
                if (isShieldDisabled) return;

                shieldEffect.ChargeShield();
            }
        }

        private void OnEnemyRegister(OnEnemyRegister e)
        {
            activeEnemies.Add(e.Enemy);
        }

        private void OnEnemyDead(OnEnemyDead e)
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

        private void OnReadyBoss(OnReadyBoss e)
        {
            StartCoroutine(ReadyBoss());
        }

        public override void OnHit(int multiplier, Action OnTargetHit)
        {
            if (!IsHittable) return;

            health--;
            OnDamaged?.Invoke(multiplier);
            OnTargetHit?.Invoke();
            OnBossDamaged?.Invoke();

            if (health <= MaxHealth / 2 && !isSecondPhase)
            {
                StartSecondPhase();
            }

            if (health <= 0)
            {
                IsHittable = false;
                EventManager.Broadcast(new OnTriggerLevelComplete());
                EventManager.Broadcast(new OnBossDead());

                EventManager.Broadcast(new OnPlaySFX("Bell"));
                StartCoroutine(LastZoom());
                EventManager.Broadcast(new OnSlowTime(0.1f, 1.2f));
                animator.SetTrigger("stun");
            }
        }

        private void StartSecondPhase()
        {
            isSecondPhase = true;
            StartCoroutine(Stun(6f));
        }

        private IEnumerator Stun(float duration)
        {
            isShieldDisabled = true;
            animator.SetTrigger("stun");
            EventManager.Broadcast(new OnPlaySFX("Stun"));
            shieldEffect.DeactivateShield();
            EventManager.Broadcast(new OnSpotting(transform, 0.2f));
            EventManager.Broadcast(new OnZoom(-0.7f, 0.2f));
            EventManager.Broadcast(new OnSlowTime(0.1f, 1.2f));
            yield return new WaitForSeconds(0.5f);
            EventManager.Broadcast(new OnSpottingEnd(0.4f));
            EventManager.Broadcast(new OnZoomEnd(0.4f));
            yield return new WaitForSeconds(duration);
            animator.SetTrigger("wake");
            yield return new WaitForSeconds(0.6f);
            StartCoroutine(Teleport(1f));
            isShieldDisabled = false;
        }

        private IEnumerator Teleport(float delay)
        {
            isShieldDisabled = true;
            IsHittable = false;
            animator.SetTrigger("teleport");
            shieldEffect.DeactivateShield();
            int random = UnityEngine.Random.Range(0, teleportPoints.Count - 1);
            Instantiate(SpawnEffect, teleportPoints[random].position, Quaternion.identity);
            yield return new WaitForSeconds(delay);
            transform.position = teleportPoints[random].position;
            isShieldDisabled = false;
            teleportCooldown = teleportCooldownMax + UnityEngine.Random.Range(0, 3);
            IsHittable = true;
        }

        private IEnumerator LastZoom()
        {
            EventManager.Broadcast(new OnSpotting(transform, 0.2f));
            EventManager.Broadcast(new OnZoom(-0.5f, 0.2f));

            yield return new WaitForSecondsRealtime(1.2f);

            EventManager.Broadcast(new OnSpottingEnd(0.4f));
            EventManager.Broadcast(new OnZoomEnd(0.4f));
        }

        private ActiveWaveData FindActiveWaveByEnemy(Enemy enemy)
        {
            foreach (ActiveWaveData activeWaveData in activeWaves)
            {
                if (activeWaveData.waveData == enemy.WaveData)
                {
                    return activeWaveData;
                }
            }

            return null;
        }

        private void TrySpawnWave()
        {
            if (!isBossReady) return;
            
            if (activeWaves.Count <= 1)
            {
                isSpawnNeeded = true;
            }

            if (spawnCooldown <= 0)
            {
                List<WaveDataSO> waveCache = new List<WaveDataSO>();
                waveCache.RemoveAll(x => activeWaves.Exists(y => y.waveData == x));

                if (waveCache.Count == 0)
                {
                    waveCache = firstPhaseWaves;
                }

                int random = UnityEngine.Random.Range(0, waveCache.Count - 1);
                LevelManager.SpawnEnemyWave(waveCache[random]);
                ActiveWaveData activeWaveData = new ActiveWaveData();
                activeWaveData.waveData = waveCache[random];
                activeWaveData.enemyCount = waveCache[random].Enemies.Count;
                activeWaves.Add(activeWaveData);
                isSpawnNeeded = false;
                spawnCooldown = maxSpawnCooldown;
            }

        }

        private IEnumerator ReadyBoss()
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

            isBossReady = true;
        }
    }
}