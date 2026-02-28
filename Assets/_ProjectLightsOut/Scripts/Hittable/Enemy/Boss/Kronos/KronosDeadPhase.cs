using System;
using System.Collections;
using ProjectLightsOut.Managers;
using UnityEngine;
using ProjectLightsOut.DevUtils;

namespace ProjectLightsOut.Gameplay
{
    public class KronosDeadPhase : IBossPhase<KronosBoss>
    {
        public void Enter(KronosBoss boss)
        {
            boss.IsHittable = false;
            boss.TimeShield.Deactivate();
            boss.TimeShield.EraseAllBullets();

            EventManager.Broadcast(new OnTriggerLevelComplete());
            EventManager.Broadcast(new OnBossDead());
            EventManager.Broadcast(new OnPlaySFX("Bell"));
            EventManager.Broadcast(new OnSlowTime(0.1f, 1.2f));

            boss.BossAnimator.SetTrigger("stun");
            boss.StartCoroutine(DeathSequence(boss));
        }

        public void UpdatePhase(KronosBoss boss) { }
        public void OnHit(KronosBoss boss, int multiplier, Action OnTargetHit) { }
        public void Exit(KronosBoss boss) { }

        private IEnumerator DeathSequence(KronosBoss boss)
        {
            EventManager.Broadcast(new OnSpotting(boss.transform, 0.2f));
            EventManager.Broadcast(new OnZoom(-0.5f, 0.2f));

            yield return new WaitForSecondsRealtime(1.2f);

            EventManager.Broadcast(new OnSpottingEnd(0.4f));
            EventManager.Broadcast(new OnZoomEnd(0.4f));
        }
    }
}
