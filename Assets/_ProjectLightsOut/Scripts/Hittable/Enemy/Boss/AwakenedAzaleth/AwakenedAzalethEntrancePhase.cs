using System;
using ProjectLightsOut.Managers;

namespace ProjectLightsOut.Gameplay
{
    public class AwakenedAzalethEntrancePhase : IBossPhase<AwakenedAzalethBoss>, IAwakenedAzalethPhase
    {
        public void Enter(AwakenedAzalethBoss boss)
        {
            boss.IsHittable = false;
        }

        public void UpdatePhase(AwakenedAzalethBoss boss) { }

        public void OnHit(AwakenedAzalethBoss boss, int multiplier, Action OnTargetHit) { }

        public void OnBuff(AwakenedAzalethBoss boss, OnBossBuff e) { }

        public void Exit(AwakenedAzalethBoss boss) { }
    }
}
