using System;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    /// <summary>
    /// Boss is inactive, waiting for OnReadyBoss event to start the intro sequence.
    /// Not hittable, no wave spawning.
    /// </summary>
    public class BossIdlePhase : IBossPhase
    {
        public void Enter(Boss boss)
        {
            boss.IsHittable = false;
        }

        public void UpdatePhase(Boss boss) { }

        public void OnHit(Boss boss, int multiplier, Action OnTargetHit) { }

        public void OnBuff(Boss boss, OnBossBuff e) { }

        public void Exit(Boss boss) { }
    }
}
