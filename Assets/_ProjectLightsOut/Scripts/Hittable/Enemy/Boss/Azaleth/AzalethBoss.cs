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

    public class AzalethBoss : BossBase<AzalethBoss>
    {
        [SerializeField] private List<WaveDataSO> firstPhaseWaves;
        [SerializeField] private List<WaveDataSO> secondPhaseWaves;
        [SerializeField] private ShieldEffect shieldEffect;
        [SerializeField] private List<Transform> teleportPoints;
        [SerializeField] private float maxSpawnCooldown = 4f;
        [SerializeField] private float teleportCooldownMax = 5f;

        public List<WaveDataSO> FirstPhaseWaves => firstPhaseWaves;
        public List<WaveDataSO> SecondPhaseWaves => secondPhaseWaves;
        public ShieldEffect ShieldEffect => shieldEffect;

        private List<ActiveWaveData> activeWaves = new List<ActiveWaveData>();
        private List<Enemy> activeEnemies = new List<Enemy>();
        private float spawnCooldown = 4f;
        private bool isSpawnNeeded = false;
        private float teleportCooldown = 5f;


        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.AddListener<OnBossBuff>(HandleBossBuff);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.RemoveListener<OnBossBuff>(HandleBossBuff);
        }

        private void HandleBossBuff(OnBossBuff e)
        {
            if (currentAzalethPhase != null)
            {
                currentAzalethPhase.OnBuff(this, e);
            }
        }

        private IAzalethPhase currentAzalethPhase;

        public override void SetPhase(IBossPhase<AzalethBoss> newPhase)
        {
            currentAzalethPhase = newPhase as IAzalethPhase;
            base.SetPhase(newPhase);
        }

        protected override void HandleEnemyRegister(OnEnemyRegister e)
        {
            activeEnemies.Add(e.Enemy);
        }

        protected override void HandleEnemyDead(OnEnemyDead e)
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

        protected override IBossPhase<AzalethBoss> CreateEntrancePhase()
        {
            return new AzalethEntrancePhase();
        }

        protected override IBossPhase<AzalethBoss> CreateDeadPhase()
        {
            return new AzalethDeadPhase();
        }

        protected override IEnumerator OnEntranceComplete()
        {
            LevelManager.SpawnEnemyWave(firstPhaseWaves[0]);
            ActiveWaveData activeWaveData = new ActiveWaveData();
            activeWaveData.waveData = firstPhaseWaves[0];
            activeWaveData.enemyCount = firstPhaseWaves[0].Enemies.Count;
            activeWaves.Add(activeWaveData);

            // Transition from Entrance → Phase 1
            SetPhase(new AzalethPhase1());
            yield return null;
        }

        public void TrySpawnWave(List<WaveDataSO> sourceWaves)
        {
            // Changed from <= 1 to == 0 to cleanly force waves to fully wipe before respawning, 
            // preventing violent physical grid overlaps!
            if (activeWaves.Count == 0)
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

        public void TickSpawnCooldown()
        {
            if (spawnCooldown > 0 && isSpawnNeeded)
            {
                spawnCooldown -= Time.deltaTime;
            }
        }

        public void TickTeleportCooldown()
        {
            if (teleportCooldown > 0)
            {
                teleportCooldown -= Time.deltaTime;
            }
            else
            {
                // Lock the timer instantly so it doesn't fire 60 coroutines violently during the delay!
                teleportCooldown = 9999f; 
                StartCoroutine(Teleport(1f));
            }
        }

        public IEnumerator Teleport(float delay)
        {
            IsHittable = false;
            BossAnimator.SetTrigger("teleport");
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
    public interface IAzalethPhase
    {
        void OnBuff(AzalethBoss boss, OnBossBuff e);
    }
}
