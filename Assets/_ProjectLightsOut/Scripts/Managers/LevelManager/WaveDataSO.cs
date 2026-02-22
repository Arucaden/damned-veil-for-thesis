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
        [Header("Enemy Configuration")]
        public int EnemyCount;
        public List<GameObject> EnemyPool;

        [Header("Path Settings (0 = use component default)")]
        public float MinPathLength;
        public int MaxBounces;

        [Header("Constraint Settings (0 = use component default)")]
        [Tooltip("Minimum distance from player. Enemies won't spawn inside this radius.")]
        public float SafeZoneRadius;
        [Tooltip("Minimum distance between enemies. Prevents clumping.")]
        public float MinEnemySpacing;
        [Tooltip("Don't spawn enemies near the end of the path.")]
        public float EndPathBuffer;
        [Tooltip("Minimum distance from walls. Enemies won't spawn within this radius of a wall.")]
        public float WallBufferRadius;
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
