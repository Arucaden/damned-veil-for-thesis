using System;

namespace ProjectLightsOut.Gameplay
{
    public class MimicBossEntrancePhase : IBossPhase<MimicBoss>, IMimicBossPhase
    {
        public void Enter(MimicBoss boss)
        {
            boss.IsHittable = false;
        }

        public void UpdatePhase(MimicBoss boss) { }
        public void OnHit(MimicBoss boss, int multiplier, Action OnTargetHit) { }
        public void Exit(MimicBoss boss) { }
    }
}
