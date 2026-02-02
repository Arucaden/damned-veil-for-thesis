using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    [CreateAssetMenu(fileName = "Wave Data", menuName = "ProjectLightsOut/Wave Data")]
    public class WaveDataSO : ScriptableObject
    {
        public List<EnemyData> Enemies;
        
        [Header("Procedural Settings")]
        public bool IsProcedural;
        public ProceduralWaveSettings ProceduralSettings;
    }

    [Serializable]
    public struct ProceduralWaveSettings
    {
        public int EnemyCount;
        public List<GameObject> EnemyPool;
        public float MinPathLength; // Optional override, defaults if 0
        public int MaxBounces; // Optional override, defaults if 0
    }


    [Serializable]
    public struct EnemyData
    {
        public string EnemyIdentifier;
        public GameObject EnemyPrefab;
        public Vector3 SpawnPosition;
        public float SpawnDelay;
    }
}
