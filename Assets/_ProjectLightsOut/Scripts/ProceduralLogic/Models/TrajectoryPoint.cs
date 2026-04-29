using UnityEngine;

namespace DamnedVeil.ProceduralLogic.Models
{
    [System.Serializable]
    public struct TrajectoryPoint
    {
        public Vector2 Position;
        public Vector2 Normal;
        public int BounceIndex;

        public TrajectoryPoint(Vector2 position, Vector2 normal, int bounceIndex)
        {
            Position = position;
            Normal = normal;
            BounceIndex = bounceIndex;
        }
    }
}
