using System;
using ProjectLightsOut.Managers;

namespace ProjectLightsOut.Gameplay
{
    public class AzalethEntrancePhase : IBossPhase<AzalethBoss>, IAzalethPhase
    {
        public void Enter(AzalethBoss boss)
        {
            boss.IsHittable = false;
        }

        public void UpdatePhase(AzalethBoss boss) { }

        public void OnHit(AzalethBoss boss, int multiplier, Action OnTargetHit) { }

        public void OnBuff(AzalethBoss boss, OnBossBuff e) { }

        public void Exit(AzalethBoss boss) { }
    }
}
