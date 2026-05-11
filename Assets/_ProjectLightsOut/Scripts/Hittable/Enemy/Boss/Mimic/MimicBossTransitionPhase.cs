using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectLightsOut.Gameplay
{
    public class MimicBossTransitionPhase : IBossPhase<MimicBoss>, IMimicBossPhase
    {
        public void Enter(MimicBoss boss)
        {
            boss.IsHittable = false;
            boss.StartCoroutine(TransitionSequence(boss));
        }

        public void UpdatePhase(MimicBoss boss) { }

        public void OnHit(MimicBoss boss, int multiplier, Action OnTargetHit) { }

        public void Exit(MimicBoss boss) { }

        private IEnumerator TransitionSequence(MimicBoss boss)
        {
            boss.BossAnimator.SetTrigger("stun");
            EventManager.Broadcast(new OnPlaySFX("Bell"));
            
            EventManager.Broadcast(new OnPlayerEnableShooting(false));

            EventManager.Broadcast(new OnSpotting(boss.transform, 0.2f));
            EventManager.Broadcast(new OnZoom(-0.5f, 0.2f));
            EventManager.Broadcast(new OnSlowTime(0.1f, 1.2f));

            yield return new WaitForSecondsRealtime(1.2f);

            EventManager.Broadcast(new OnSpottingEnd(0.4f));
            EventManager.Broadcast(new OnZoomEnd(0.4f));

            Time.timeScale = 0f;
            EventManager.Broadcast(new OnToggleBlackout(true, boss.BlackoutDuration));

            yield return new WaitForSecondsRealtime(boss.BlackoutHoldDuration);

            if (!string.IsNullOrEmpty(boss.Phase2SceneName))
            {
                AppStateManager.Instance.GoToNextLevel(boss.Phase2SceneName);
            }
        }
    }
}
