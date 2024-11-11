using System.Collections.Generic;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    [CreateAssetMenu(fileName = "Level Data", menuName = "ProjectLightsOut/Level Data")]
    public class LevelDataSO : ScriptableObject
    {
        public int LevelScore = 1000;
        public List<WaveDataSO> Waves;
        public int Bullets = 6;
        public float AceTime = 60f;
        public List<string> NextLevelScenes;
    }
}