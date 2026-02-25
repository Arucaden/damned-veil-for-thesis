using System;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    /// <summary>
    /// Phase 2 — Azaleth spawns second phase waves, teleports periodically,
    /// and is more aggressive. Transitions to Dead at 0 health.
    /// </summary>
    public class AzalethPhase2 : IBossPhase<AzalethBoss>, IAzalethPhase
    {
        public void Enter(AzalethBoss boss)
        {
            boss.IsHittable = true;
        }

        public void UpdatePhase(AzalethBoss boss)
        {
            boss.TrySpawnWave(boss.SecondPhaseWaves);
            boss.TickSpawnCooldown();
            boss.TickTeleportCooldown();
        }

        public void OnHit(AzalethBoss boss, int multiplier, Action OnTargetHit)
        {
            boss.ApplyDamage(multiplier, OnTargetHit);

            if (boss.Health <= 0)
            {
                boss.SetPhase(new AzalethDeadPhase());
            }
        }

        public void OnBuff(AzalethBoss boss, OnBossBuff e)
        {
            if (e.buffType == BuffType.Health)
            {
                boss.OnBossHealed?.Invoke();
            }
            else if (e.buffType == BuffType.Shield)
            {
                boss.ShieldEffect.ChargeShield();
            }
        }

        public void Exit(AzalethBoss boss) { }
    }
}
