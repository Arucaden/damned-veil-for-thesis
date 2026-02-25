using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class AzalethTransitionPhase : IBossPhase<AzalethBoss>, IAzalethPhase
    {
        public void Enter(AzalethBoss boss)
        {
            boss.IsHittable = false;
            boss.StartCoroutine(StunSequence(boss));
        }

        public void UpdatePhase(AzalethBoss boss) { }

        public void OnHit(AzalethBoss boss, int multiplier, Action OnTargetHit) { }

        public void OnBuff(AzalethBoss boss, OnBossBuff e)
        {
            // Shield buffs blocked during stun
            if (e.buffType == BuffType.Health)
            {
                boss.OnBossHealed?.Invoke();
            }
        }

        public void Exit(AzalethBoss boss) { }

        private IEnumerator StunSequence(AzalethBoss boss)
        {
            boss.BossAnimator.SetTrigger("stun");
            EventManager.Broadcast(new OnPlaySFX("Stun"));
            boss.ShieldEffect.DeactivateShield();
            EventManager.Broadcast(new OnSpotting(boss.transform, 0.2f));
            EventManager.Broadcast(new OnZoom(-0.7f, 0.2f));
            EventManager.Broadcast(new OnSlowTime(0.1f, 1.2f));

            yield return new WaitForSeconds(0.5f);
            EventManager.Broadcast(new OnSpottingEnd(0.4f));
            EventManager.Broadcast(new OnZoomEnd(0.4f));

            yield return new WaitForSeconds(6f);
            boss.BossAnimator.SetTrigger("wake");

            yield return new WaitForSeconds(0.6f);

            boss.SetPhase(new AzalethPhase2());

            // Teleport immediately after entering Phase2
            boss.StartCoroutine(boss.Teleport(1f));
        }
    }
}
