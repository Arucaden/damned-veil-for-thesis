using System.Collections.Generic;
using UnityEngine;
using ProjectLightsOut.Hittable;
using ProjectLightsOut.Managers;
using System;

namespace ProjectLightsOut.Riddles
{
    /// <summary>
    /// A puzzle that infinitely loops the enemy spawn waves until all pillars are destroyed IN SEQUENTIAL ORDER!
    /// The player must destroy all pillars exactly 0 to N, AND clean up the residual enemies to finish combat.
    /// </summary>
    public class PillarsRiddle : BaseRiddle
    {
        [Tooltip("Drag the 'Pillar' Destructible Walls that must be destroyed here IN THE EXACT ORDER they must be hit! Make sure to set their 'Destroy On Death' to False!")]
        [SerializeField] private List<DestructibleWall> targetPillars = new List<DestructibleWall>();

        private int currentSequenceIndex = 0;
        private WaveManager waveManager;
        private Action[] pillarDelegates;

        private void Start()
        {
            if (targetPillars.Count == 0)
            {
                // Auto-complete if no pillars are placed in the inspector
                CompleteRiddle();
                return;
            }

            pillarDelegates = new Action[targetPillars.Count];

            for (int i = 0; i < targetPillars.Count; i++)
            {
                if (targetPillars[i] != null)
                {
                    int index = i;
                    pillarDelegates[i] = () => HandlePillarDestroyed(index);
                    targetPillars[i].OnWallDestroyed += pillarDelegates[i];
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

        private void HandlePillarDestroyed(int destroyedIndex)
        {
            if (IsSolved) return;

            if (destroyedIndex == currentSequenceIndex)
            {
                // CORRECT HIT!
                currentSequenceIndex++;

                if (currentSequenceIndex >= targetPillars.Count)
                {
                    //DevUtils.EventManager.Broadcast(new ProjectLightsOut.DevUtils.OnPlaySFX("PuzzleSolved"));
                    CompleteRiddle();
                }
            }
            else
            {
                // INCORRECT HIT! Reset the sequence and revive everything!
                StopAllCoroutines();
                StartCoroutine(FailSequenceCoroutine());
            }
        }

        private System.Collections.IEnumerator FailSequenceCoroutine()
        {
            //DevUtils.EventManager.Broadcast(new ProjectLightsOut.DevUtils.OnPlaySFX("ErrorBuzzer")); // Placeholder, add a buzzer sound to your pools!
            
            // Wait slightly for the bullet impact and death visuals before snapping everything back
            yield return new WaitForSeconds(0.5f);

            currentSequenceIndex = 0;

            foreach (var pillar in targetPillars)
            {
                if (pillar != null)
                {
                    pillar.Respawn();
                }
            }
        }

        private void OnDestroy()
        {
            if (pillarDelegates != null)
            {
                for (int i = 0; i < targetPillars.Count; i++)
                {
                    if (targetPillars[i] != null && pillarDelegates[i] != null)
                    {
                        targetPillars[i].OnWallDestroyed -= pillarDelegates[i];
                    }
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
