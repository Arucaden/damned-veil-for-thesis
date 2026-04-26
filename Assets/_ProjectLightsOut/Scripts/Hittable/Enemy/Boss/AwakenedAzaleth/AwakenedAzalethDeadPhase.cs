using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class AwakenedAzalethDeadPhase : IBossPhase<AwakenedAzalethBoss>, IAwakenedAzalethPhase
    {
        public void Enter(AwakenedAzalethBoss boss)
        {
            boss.IsHittable = false;

            boss.WipeAllOrbs();

            EventManager.Broadcast(new OnToggleBlackout(false, 0.5f));

            EventManager.Broadcast(new OnTriggerLevelComplete());
            EventManager.Broadcast(new OnBossDead());
            EventManager.Broadcast(new OnPlaySFX("Bell"));
            EventManager.Broadcast(new OnSlowTime(0.1f, 1.2f));

            boss.BossAnimator.SetTrigger("stun");
            boss.StartCoroutine(LastZoom(boss));
        }

        public void UpdatePhase(AwakenedAzalethBoss boss) { }

        public void OnHit(AwakenedAzalethBoss boss, int multiplier, Action OnTargetHit) { }

        public void OnBuff(AwakenedAzalethBoss boss, OnBossBuff e) { }

        public void Exit(AwakenedAzalethBoss boss) { }

        private IEnumerator LastZoom(AwakenedAzalethBoss boss)
        {
            EventManager.Broadcast(new OnSpotting(boss.transform, 0.2f));
            EventManager.Broadcast(new OnZoom(-0.5f, 0.2f));

            yield return new WaitForSecondsRealtime(1.2f);

            EventManager.Broadcast(new OnSpottingEnd(0.4f));
            EventManager.Broadcast(new OnZoomEnd(0.4f));
        }
    }
}
