using System;

namespace ProjectLightsOut.Gameplay
{
    /// <summary>
    /// State interface for the Boss state machine.
    /// Each phase implements its own Update, OnHit, and OnBuff behavior.
    /// </summary>
    public interface IBossPhase
    {
        void Enter(Boss boss);
        void UpdatePhase(Boss boss);
        void OnHit(Boss boss, int multiplier, Action OnTargetHit);
        void OnBuff(Boss boss, OnBossBuff e);
        void Exit(Boss boss);
    }
}
