using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class MimicBossDeadPhase : IBossPhase<MimicBoss>, IMimicBossPhase
    {
        public void Enter(MimicBoss boss)
        {
            Debug.Log($"[MimicBossDeadPhase] Enter called. boss.Health={boss.Health}");
            boss.IsHittable = false;

            EventManager.Broadcast(new OnBossDead());
            EventManager.Broadcast(new OnPlaySFX("Bell"));
            EventManager.Broadcast(new OnSlowTime(0.1f, 1.2f));

            boss.BossAnimator.SetTrigger("stun");
            boss.StartCoroutine(LastZoom(boss));
        }

        public void UpdatePhase(MimicBoss boss) { }
        public void OnHit(MimicBoss boss, int multiplier, Action OnTargetHit) { }
        public void Exit(MimicBoss boss) { }

        private IEnumerator LastZoom(MimicBoss boss)
        {
            Debug.Log("[MimicBossDeadPhase] LastZoom started.");
            EventManager.Broadcast(new OnSpotting(boss.transform, 0.2f));
            EventManager.Broadcast(new OnZoom(-0.5f, 0.2f));

            yield return new WaitForSecondsRealtime(1.2f);

            Debug.Log("[MimicBossDeadPhase] Cinematic wait done. Firing SpottingEnd, ZoomEnd, then OnTriggerLevelComplete.");
            EventManager.Broadcast(new OnSpottingEnd(0.4f));
            EventManager.Broadcast(new OnZoomEnd(0.4f));

            // Fire AFTER slow-motion ends so LevelComplete() runs at normal timescale
            EventManager.Broadcast(new OnTriggerLevelComplete());
        }
    }
}
