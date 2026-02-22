using System;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Effects;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    /// <summary>
    /// First combat phase. Boss spawns waves from firstPhaseWaves,
    /// is hittable, and accepts shield/health buffs.
    /// Transitions to Stun at half health, or Dead at 0.
    /// </summary>
    public class BossPhase1 : IBossPhase
    {
        public void Enter(Boss boss)
        {
            boss.IsHittable = true;
        }

        public void UpdatePhase(Boss boss)
        {
            boss.TrySpawnWave(boss.FirstPhaseWaves);
            boss.TickSpawnCooldown();
        }

        public void OnHit(Boss boss, int multiplier, Action OnTargetHit)
        {
            boss.ApplyDamage(multiplier, OnTargetHit);

            if (boss.Health <= boss.MaxHealth / 2)
            {
                boss.SetPhase(new BossStunPhase());
                return;
            }

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
