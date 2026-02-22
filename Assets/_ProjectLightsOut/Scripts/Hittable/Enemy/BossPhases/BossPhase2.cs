using System;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    /// <summary>
    /// Second combat phase. Boss spawns waves from secondPhaseWaves,
    /// is hittable, teleports periodically, and accepts buffs.
    /// Transitions to Dead at 0 health.
    /// </summary>
    public class BossPhase2 : IBossPhase
    {
        public void Enter(Boss boss)
        {
            boss.IsHittable = true;
        }

        public void UpdatePhase(Boss boss)
        {
            boss.TrySpawnWave(boss.SecondPhaseWaves);
            boss.TickSpawnCooldown();
            boss.TickTeleportCooldown();
        }

        public void OnHit(Boss boss, int multiplier, Action OnTargetHit)
        {
            boss.ApplyDamage(multiplier, OnTargetHit);

            if (boss.Health <= 0)
            {
                boss.SetPhase(new BossDeadPhase());
            }
        }

        public void OnBuff(Boss boss, OnBossBuff e)
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

        public void Exit(Boss boss) { }
    }
}
