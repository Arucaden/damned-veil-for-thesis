using System;

namespace ProjectLightsOut.Gameplay
{
    public class KronosEntrancePhase : IBossPhase<KronosBoss>
    {
        public void Enter(KronosBoss boss) { boss.IsHittable = false; }
        public void UpdatePhase(KronosBoss boss) { }
        public void OnHit(KronosBoss boss, int multiplier, Action OnTargetHit) { }
        public void Exit(KronosBoss boss) { }
    }
}
