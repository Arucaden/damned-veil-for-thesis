using System;
using System.Collections;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class KronosBoss : BossBase<KronosBoss>
    {
        [Header("Kronos Settings")]
        [SerializeField] private KronosTimeShield timeShield;

        public KronosTimeShield TimeShield => timeShield;


        protected override IBossPhase<KronosBoss> CreateEntrancePhase()
        {
            return new KronosEntrancePhase();
        }

        protected override IBossPhase<KronosBoss> CreateDeadPhase()
        {
            return new KronosDeadPhase();
        }

        protected override IEnumerator OnEntranceComplete()
        {
            // Transition from Entrance → Phase 1
            SetPhase(new KronosPhase1());
            yield return null;
        }
    }
}
