using System;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class MimicBossPhase2 : IBossPhase<MimicBoss>, IMimicBossPhase
    {
        private float vulnerabilityTimer = 0f;

        public void Enter(MimicBoss boss)
        {
            boss.IsHittable = true;
            boss.IsVulnerable = false;
        }

        public void UpdatePhase(MimicBoss boss)
        {
            if (boss.IsVulnerable)
            {
                vulnerabilityTimer -= Time.deltaTime;
                if (vulnerabilityTimer <= 0)
                {
                    boss.IsVulnerable = false;
                    if (boss.PillarsRiddle != null) boss.PillarsRiddle.ForceReset();
                }
            }
            else
            {
                if (boss.PillarsRiddle != null && boss.PillarsRiddle.IsSolved)
                {
                    boss.IsVulnerable = true;
                    vulnerabilityTimer = boss.VulnerabilityDuration;
                }
            }
        }

        public void OnHit(MimicBoss boss, int multiplier, Action OnTargetHit)
        {
            if (boss.IsVulnerable)
            {
                Debug.Log($"[MimicBossPhase2] Boss hit while vulnerable. Health before: {boss.Health}");
                boss.ApplyDamage(multiplier, OnTargetHit);
                Debug.Log($"[MimicBossPhase2] Health after damage: {boss.Health}");
                if (boss.Health <= 0)
                {
                    Debug.Log("[MimicBossPhase2] Health <= 0! Entering MimicBossDeadPhase.");
                    boss.SetPhase(new MimicBossDeadPhase());
                }
            }
            else
            {
                Debug.Log("[MimicBossPhase2] Boss hit while INVULNERABLE. Ignoring damage.");
                OnTargetHit?.Invoke();
            }
        }

        public void Exit(MimicBoss boss) { }
    }
}
