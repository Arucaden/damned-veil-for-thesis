using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class AwakenedAzalethTransitionPhase : IBossPhase<AwakenedAzalethBoss>, IAwakenedAzalethPhase
    {
        private bool hitRegistered;

        public void Enter(AwakenedAzalethBoss boss)
        {
            hitRegistered  = false;
            boss.IsHittable = false;
            boss.StartCoroutine(StaggerSequence(boss));
        }

        public void UpdatePhase(AwakenedAzalethBoss boss) { }

        public void OnHit(AwakenedAzalethBoss boss, int multiplier, Action OnTargetHit)
        {
            if (hitRegistered) return;
            hitRegistered    = true;
            boss.IsHittable  = false;

            OnTargetHit?.Invoke();
            boss.StartCoroutine(HitSequence(boss));
        }

        public void OnBuff(AwakenedAzalethBoss boss, OnBossBuff e)
        {
            if (e.buffType == BuffType.Health)
                boss.OnBossHealed?.Invoke();
        }

        public void Exit(AwakenedAzalethBoss boss) { }

        private IEnumerator StaggerSequence(AwakenedAzalethBoss boss)
        {
            boss.WipeAllOrbs();

            boss.BossAnimator.SetTrigger("stun");
            EventManager.Broadcast(new OnPlaySFX("Stun"));
            boss.ShieldEffect.DeactivateShield();
            EventManager.Broadcast(new OnSpotting(boss.transform, 0.2f));
            EventManager.Broadcast(new OnZoom(-0.7f, 0.2f));
            EventManager.Broadcast(new OnSlowTime(0.1f, 1.2f));

            yield return new WaitForSeconds(0.5f);
            EventManager.Broadcast(new OnSpottingEnd(0.4f));
            EventManager.Broadcast(new OnZoomEnd(0.4f));

            yield return new WaitForSeconds(0.5f);
            boss.IsHittable = true;
        }

        private IEnumerator HitSequence(AwakenedAzalethBoss boss)
        {
            EventManager.Broadcast(new OnPlayerEnableShooting(false));
            EventManager.Broadcast(new OnSlowTime(0.1f, 0.5f));

            yield return new WaitForSeconds(1.5f);

            boss.GivePlayerVoidProjectile();

            yield return new WaitForSeconds(0.3f);
            boss.BossAnimator.SetTrigger("wake");
            yield return new WaitForSeconds(0.6f);

            boss.SetPhase(new AwakenedAzalethPhase2());
            boss.StartCoroutine(boss.Teleport(1f));
        }
    }
}
