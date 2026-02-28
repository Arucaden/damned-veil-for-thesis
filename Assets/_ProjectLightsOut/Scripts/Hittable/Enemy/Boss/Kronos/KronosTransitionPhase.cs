using System;

namespace ProjectLightsOut.Gameplay
{
    public class KronosTransitionPhase : IBossPhase<KronosBoss>
    {
        public void Enter(KronosBoss boss)
        {
            boss.IsHittable = false;
            boss.SetPhase(new KronosPhase2());
        }

        public void UpdatePhase(KronosBoss boss) { }
        public void OnHit(KronosBoss boss, int multiplier, Action OnTargetHit) { }
        public void Exit(KronosBoss boss) { }
    }
}
