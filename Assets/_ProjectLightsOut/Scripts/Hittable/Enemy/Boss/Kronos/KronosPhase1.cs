using System;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class KronosPhase1 : IBossPhase<KronosBoss>
    {
        public void Enter(KronosBoss boss)
        {
            boss.IsHittable = false;
            boss.TimeShield.Activate();

            boss.TimeShield.OnShieldDown = () =>
            {
                boss.IsHittable = true;
            };

            boss.TimeShield.OnShieldUp = () =>
            {
                boss.IsHittable = false;
            };
        }

        public void UpdatePhase(KronosBoss boss) { }

        public void OnHit(KronosBoss boss, int multiplier, Action OnTargetHit)
        {
            boss.ApplyDamage(multiplier, OnTargetHit);

            if (boss.Health <= 0)
            {
                boss.SetPhase(new KronosDeadPhase());
                return;
            }

            // Transition to Phase 2 at threshold (future implementation)
            // if (boss.Health <= boss.MaxHealth / 2)
            // {
            //     boss.SetPhase(new KronosTransitionPhase());
            // }
        }

        public void Exit(KronosBoss boss)
        {
            boss.TimeShield.Deactivate();
            boss.TimeShield.OnShieldDown = null;
            boss.TimeShield.OnShieldUp = null;
        }
    }
}
