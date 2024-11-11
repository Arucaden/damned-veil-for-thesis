using System;

namespace ProjectLightsOut.Gameplay
{
    public interface IHittable
    {
        public bool IsHittable { get; }
        void OnHit(int multiplier, Action onDeadAction = null);
    }
}
