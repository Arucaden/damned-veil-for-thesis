using UnityEngine;

namespace DamnedVeil.ProceduralLogic.Models
{
    /// <summary>
    /// Represents a single point on the specular (ricochet) trajectory.
    /// </summary>
    [System.Serializable]
    public struct TrajectoryPoint
    {
        /// <summary>
        /// World position of this trajectory point.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Wall normal at this point (if this is a bounce point).
        /// </summary>
        public Vector2 Normal;

        /// <summary>
        /// Which bounce this is (0 = source/origin point).
        /// </summary>
        public int BounceIndex;

        public TrajectoryPoint(Vector2 position, Vector2 normal, int bounceIndex)
        {
            Position = position;
            Normal = normal;
            BounceIndex = bounceIndex;
        }
    }
}
