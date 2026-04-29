using UnityEngine;

namespace DamnedVeil.ProceduralLogic.Models
{
    [System.Serializable]
    public struct EnemySpawnData
    {
        public Vector2 Position;
        public int OnSegmentIndex;

        public EnemySpawnData(Vector2 position, int onSegmentIndex)
        {
            Position = position;
            OnSegmentIndex = onSegmentIndex;
        }
    }
}
