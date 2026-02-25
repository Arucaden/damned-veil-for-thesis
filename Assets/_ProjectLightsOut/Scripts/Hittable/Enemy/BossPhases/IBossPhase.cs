using System;

namespace ProjectLightsOut.Gameplay
{
    /// <summary>
    /// Generic state interface for all boss state machines.
    /// T is the specific boss type, enabling type-safe phase implementations.
    /// </summary>
    public interface IBossPhase<T> where T : BossBase<T>
    {
        void Enter(T boss);
        void UpdatePhase(T boss);
        void OnHit(T boss, int multiplier, Action OnTargetHit);
        void Exit(T boss);
    }
}
