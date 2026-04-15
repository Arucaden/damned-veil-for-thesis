using System.Collections.Generic;
using UnityEngine;
using ProjectLightsOut.Hittable;
using ProjectLightsOut.Managers;

namespace ProjectLightsOut.Riddles
{
    /// <summary>
    /// A puzzle that infinitely loops the enemy spawn waves until all pillars are destroyed!
    /// The player must destroy all pillars, AND clean up the residual enemies to finish combat.
    /// </summary>
    public class PillarsRiddle : BaseRiddle
    {
        [Tooltip("Drag the 'Pillar' Destructible Walls that must be destroyed into this list.")]
        [SerializeField] private List<DestructibleWall> targetPillars = new List<DestructibleWall>();

        private int pillarsRemaining;
        private WaveManager waveManager;

        private void Start()
        {
            pillarsRemaining = targetPillars.Count;

            if (pillarsRemaining == 0)
            {
                // Auto-complete if no pillars are placed in the inspector
                CompleteRiddle();
                return;
            }

            foreach (var pillar in targetPillars)
            {
                if (pillar != null)
                {
                    pillar.OnWallDestroyed += HandlePillarDestroyed;
                }
            }

            // Hook into the WaveManager to force infinite enemy respawns!
            waveManager = FindObjectOfType<WaveManager>();
            if (waveManager != null)
            {
                waveManager.ShouldLoopWaves += IsRiddleActive;
            }
        }

        private bool IsRiddleActive()
        {
            // If it's NOT solved, we legally demand the WaveManager to loop!
            return !IsSolved;
        }

        private void HandlePillarDestroyed()
        {
            if (IsSolved) return;

            pillarsRemaining--;

            if (pillarsRemaining <= 0)
            {
                CompleteRiddle();
            }
        }

        private void OnDestroy()
        {
            foreach (var pillar in targetPillars)
            {
                if (pillar != null)
                {
                    pillar.OnWallDestroyed -= HandlePillarDestroyed;
                }
            }

            // Always gracefully unbind from the WaveManager
            if (waveManager != null)
            {
                waveManager.ShouldLoopWaves -= IsRiddleActive;
            }
        }
    }
}
