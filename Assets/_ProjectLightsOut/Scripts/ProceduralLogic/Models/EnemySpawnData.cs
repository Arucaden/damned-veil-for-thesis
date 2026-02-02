using UnityEngine;

namespace DamnedVeil.ProceduralLogic.Models
{
    /// <summary>
    /// Contains spawn data for a single enemy on the ricochet path.
    /// </summary>
    [System.Serializable]
    public struct EnemySpawnData
    {
        /// <summary>
        /// World position where the enemy should spawn.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// Which path segment this enemy is on (0-indexed).
        /// </summary>
        public int OnSegmentIndex;

        public EnemySpawnData(Vector2 position, int onSegmentIndex)
        {
            Position = position;
            OnSegmentIndex = onSegmentIndex;
        }
    }
}
