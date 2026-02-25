using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    /// <summary>
    /// Dead phase — Azaleth is dead. Broadcasts death events,
    /// plays death animation, camera effects. Terminal state.
    /// </summary>
    public class AzalethDeadPhase : IBossPhase<AzalethBoss>, IAzalethPhase
    {
        public void Enter(AzalethBoss boss)
        {
            boss.IsHittable = false;

            EventManager.Broadcast(new OnTriggerLevelComplete());
            EventManager.Broadcast(new OnBossDead());
            EventManager.Broadcast(new OnPlaySFX("Bell"));
            EventManager.Broadcast(new OnSlowTime(0.1f, 1.2f));

            boss.BossAnimator.SetTrigger("stun");
            boss.StartCoroutine(LastZoom(boss));
        }

        public void UpdatePhase(AzalethBoss boss) { }

        public void OnHit(AzalethBoss boss, int multiplier, Action OnTargetHit) { }

        public void OnBuff(AzalethBoss boss, OnBossBuff e) { }

        public void Exit(AzalethBoss boss) { }

        private IEnumerator LastZoom(AzalethBoss boss)
        {
            EventManager.Broadcast(new OnSpotting(boss.transform, 0.2f));
            EventManager.Broadcast(new OnZoom(-0.5f, 0.2f));

            yield return new WaitForSecondsRealtime(1.2f);

            EventManager.Broadcast(new OnSpottingEnd(0.4f));
            EventManager.Broadcast(new OnZoomEnd(0.4f));
        }
    }
}
