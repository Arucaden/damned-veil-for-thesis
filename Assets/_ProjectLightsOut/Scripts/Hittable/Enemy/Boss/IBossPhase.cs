using System;

namespace ProjectLightsOut.Gameplay
{
    public interface IBossPhase<T> where T : BossBase<T>
    {
        void Enter(T boss);
        void UpdatePhase(T boss);
        void OnHit(T boss, int multiplier, Action OnTargetHit);
        void Exit(T boss);
    }
}
