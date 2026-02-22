using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    /// <summary>
    /// Stun transition phase. Boss is stunned with camera effects,
    /// shield disabled. After duration, wakes up and transitions to Phase2.
    /// </summary>
    public class BossStunPhase : IBossPhase
    {
        public void Enter(Boss boss)
        {
            boss.IsHittable = false;
            boss.StartCoroutine(StunSequence(boss));
        }

        public void UpdatePhase(Boss boss) { }

        public void OnHit(Boss boss, int multiplier, Action OnTargetHit) { }

        public void OnBuff(Boss boss, OnBossBuff e)
        {
            // Shield buffs blocked during stun
            if (e.buffType == BuffType.Health)
            {
                boss.OnBossHealed?.Invoke();
            }
        }

        public void Exit(Boss boss) { }

        private IEnumerator StunSequence(Boss boss)
        {
            boss.Animator.SetTrigger("stun");
            EventManager.Broadcast(new OnPlaySFX("Stun"));
            boss.ShieldEffect.DeactivateShield();
            EventManager.Broadcast(new OnSpotting(boss.transform, 0.2f));
            EventManager.Broadcast(new OnZoom(-0.7f, 0.2f));
            EventManager.Broadcast(new OnSlowTime(0.1f, 1.2f));

            yield return new WaitForSeconds(0.5f);
            EventManager.Broadcast(new OnSpottingEnd(0.4f));
            EventManager.Broadcast(new OnZoomEnd(0.4f));

            yield return new WaitForSeconds(6f);
            boss.Animator.SetTrigger("wake");

            yield return new WaitForSeconds(0.6f);

            boss.SetPhase(new BossPhase2());

            // Teleport immediately after entering Phase2
            boss.StartCoroutine(boss.Teleport(1f));
        }
    }
}
