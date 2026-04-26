using System;
using ProjectLightsOut.Managers;

namespace ProjectLightsOut.Gameplay
{
    public class AwakenedAzalethPhase1 : IBossPhase<AwakenedAzalethBoss>, IAwakenedAzalethPhase
    {
        public void Enter(AwakenedAzalethBoss boss)
        {
            boss.IsHittable = true;
            boss.StartOrbSpawnLoop(boss.Phase1OrbInterval);
        }

        public void UpdatePhase(AwakenedAzalethBoss boss)
        {
            boss.TrySpawnWave(boss.FirstPhaseWaves);
            boss.TickSpawnCooldown();
        }

        public void OnHit(AwakenedAzalethBoss boss, int multiplier, Action OnTargetHit)
        {
            boss.ApplyDamage(multiplier, OnTargetHit);

            if (boss.Health <= 0)
            {
                boss.SetPhase(new AwakenedAzalethDeadPhase());
                return;
            }

            if (boss.Health <= boss.MaxHealth / 2)
            {
                boss.SetPhase(new AwakenedAzalethTransitionPhase());
            }
        }

        public void OnBuff(AwakenedAzalethBoss boss, OnBossBuff e)
        {
            if (e.buffType == BuffType.Health)
                boss.OnBossHealed?.Invoke();
            else if (e.buffType == BuffType.Shield)
                boss.ShieldEffect.ChargeShield();
        }

        public void Exit(AwakenedAzalethBoss boss)
        {
            boss.StopOrbSpawnLoop();
        }
    }
}
