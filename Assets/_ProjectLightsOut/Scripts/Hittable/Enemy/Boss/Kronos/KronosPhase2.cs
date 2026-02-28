using System;

namespace ProjectLightsOut.Gameplay
{
    public class KronosPhase2 : IBossPhase<KronosBoss>
    {
        public void Enter(KronosBoss boss)
        {
            boss.IsHittable = true;
            // TODO: Phase 2 mechanics
        }

        public void UpdatePhase(KronosBoss boss) { }

        public void OnHit(KronosBoss boss, int multiplier, Action OnTargetHit)
        {
            boss.ApplyDamage(multiplier, OnTargetHit);

            if (boss.Health <= 0)
            {
                boss.SetPhase(new KronosDeadPhase());
            }
        }

        public void Exit(KronosBoss boss) { }
    }
}
