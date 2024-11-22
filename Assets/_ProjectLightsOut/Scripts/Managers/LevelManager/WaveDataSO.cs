using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    [CreateAssetMenu(fileName = "Wave Data", menuName = "ProjectLightsOut/Wave Data")]
    public class WaveDataSO : ScriptableObject
    {
        public List<EnemyData> Enemies;
    }

    [Serializable] public struct EnemyData
    {
        public string EnemyIdentifier;
        public GameObject EnemyPrefab;
        public Vector3 SpawnPosition;
        public float SpawnDelay;
    }
}
