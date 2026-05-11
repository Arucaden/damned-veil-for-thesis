using System;
using UnityEngine;
using ProjectLightsOut.Managers;

namespace ProjectLightsOut.Gameplay
{
    public class MimicBossPhase1 : IBossPhase<MimicBoss>, IMimicBossPhase
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
                    boss.PillarsRiddle.ForceReset();
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
                boss.ApplyDamage(multiplier, OnTargetHit);
                if (boss.Health <= 0)
                {
                    if (!string.IsNullOrEmpty(boss.Phase2SceneName))
                    {
                        boss.SetPhase(new MimicBossTransitionPhase());
                    }
                    else
                    {
                        boss.SetPhase(new MimicBossDeadPhase());
                    }
                }
            }
            else
            {
                OnTargetHit?.Invoke();
            }
        }

        public void Exit(MimicBoss boss)
        {
        }
    }
}
