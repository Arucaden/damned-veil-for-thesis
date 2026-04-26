using System;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;

namespace ProjectLightsOut.Gameplay
{
    public class AwakenedAzalethPhase2 : IBossPhase<AwakenedAzalethBoss>, IAwakenedAzalethPhase
    {
        public void Enter(AwakenedAzalethBoss boss)
        {
            boss.IsHittable = true;

            EventManager.Broadcast(new OnPlayerEnableShooting(true));

            EventManager.Broadcast(new OnToggleBlackout(true, boss.BlackoutFadeTime));

            boss.StartOrbSpawnLoop(boss.Phase2OrbInterval);
        }

        public void UpdatePhase(AwakenedAzalethBoss boss)
        {
            boss.TrySpawnWave(boss.SecondPhaseWaves);
            boss.TickSpawnCooldown();
            boss.TickTeleportCooldown();
        }

        public void OnHit(AwakenedAzalethBoss boss, int multiplier, Action OnTargetHit)
        {
            boss.ApplyDamage(multiplier, OnTargetHit);

            if (boss.Health <= 0)
                boss.SetPhase(new AwakenedAzalethDeadPhase());
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
