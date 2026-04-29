using System.Collections.Generic;
using UnityEngine;
using ProjectLightsOut.Hittable;
using ProjectLightsOut.Managers;
using System;

namespace ProjectLightsOut.Riddles
{
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

            waveManager = FindObjectOfType<WaveManager>();
            if (waveManager != null)
            {
                waveManager.ShouldLoopWaves += IsRiddleActive;
            }
        }

        private bool IsRiddleActive()
        {
            return !IsSolved;
        }

        private void HandlePillarDestroyed(int destroyedIndex)
        {
            if (IsSolved) return;

            if (destroyedIndex == currentSequenceIndex)
            {
                currentSequenceIndex++;

                if (currentSequenceIndex >= targetPillars.Count)
                {
                    CompleteRiddle();
                }
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(FailSequenceCoroutine());
            }
        }

        private System.Collections.IEnumerator FailSequenceCoroutine()
        {
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

            if (waveManager != null)
            {
                waveManager.ShouldLoopWaves -= IsRiddleActive;
            }
        }
    }
}
