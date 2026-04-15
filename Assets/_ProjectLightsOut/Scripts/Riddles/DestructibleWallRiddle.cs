using UnityEngine;
using System.Collections.Generic;
using ProjectLightsOut.Hittable;

namespace ProjectLightsOut.Managers
{
    /// <summary>
    /// A riddle that requires a specific set of DestructibleWalls to be broken.
    /// </summary>
    public class DestructibleWallRiddle : BaseRiddle
    {
        [Tooltip("The walls that must be destroyed to solve this riddle.")]
        [SerializeField] private List<DestructibleWall> requiredWalls = new List<DestructibleWall>();

        private int wallsRemaining;

        private void Start()
        {
            wallsRemaining = requiredWalls.Count;

            if (wallsRemaining == 0)
            {
                CompleteRiddle();
                return;
            }

            foreach (var wall in requiredWalls)
            {
                if (wall != null)
                {
                    wall.OnWallDestroyed += HandleWallDestroyed;
                }
                else
                {
                    wallsRemaining--;
                }
            }
        }

        private void HandleWallDestroyed()
        {
            wallsRemaining--;
            
            if (wallsRemaining <= 0)
            {
                CompleteRiddle();
            }
        }
        
        private void OnDestroy()
        {
            foreach (var wall in requiredWalls)
            {
                if (wall != null)
                {
                    wall.OnWallDestroyed -= HandleWallDestroyed;
                }
            }
        }
    }
}
