using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    /// <summary>
    /// Terminal state. Boss is dead — broadcasts death events,
    /// plays death animation, camera effects. No further actions.
    /// </summary>
    public class BossDeadPhase : IBossPhase
    {
        public void Enter(Boss boss)
        {
            boss.IsHittable = false;

            EventManager.Broadcast(new OnTriggerLevelComplete());
            EventManager.Broadcast(new OnBossDead());
            EventManager.Broadcast(new OnPlaySFX("Bell"));
            EventManager.Broadcast(new OnSlowTime(0.1f, 1.2f));

            boss.Animator.SetTrigger("stun");
            boss.StartCoroutine(LastZoom(boss));
        }

        public void UpdatePhase(Boss boss) { }

        public void OnHit(Boss boss, int multiplier, Action OnTargetHit) { }

        public void OnBuff(Boss boss, OnBossBuff e) { }

        public void Exit(Boss boss) { }

        private IEnumerator LastZoom(Boss boss)
        {
            EventManager.Broadcast(new OnSpotting(boss.transform, 0.2f));
            EventManager.Broadcast(new OnZoom(-0.5f, 0.2f));

            yield return new WaitForSecondsRealtime(1.2f);

            EventManager.Broadcast(new OnSpottingEnd(0.4f));
            EventManager.Broadcast(new OnZoomEnd(0.4f));
        }
    }
}
