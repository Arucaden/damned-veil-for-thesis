using System.Collections.Generic;
using UnityEngine;
using ProjectLightsOut.Hittable;
using ProjectLightsOut.Managers;
using System;

namespace ProjectLightsOut.Riddles
{
    public class PillarsRiddle : BaseRiddle
    {
        public event Action OnSequenceFailed;

        [Tooltip("Drag the 'Pillar' Destructible Walls that must be destroyed here IN THE EXACT ORDER they must be hit! Make sure to set their 'Destroy On Death' to False!")]
        [SerializeField] private List<DestructibleWall> targetPillars = new List<DestructibleWall>();

        private int currentSequenceIndex = 0;
        private WaveManager waveManager;
        private Action<bool>[] pillarDelegates;

        private void Start()
        {
            if (targetPillars.Count == 0)
            {
                CompleteRiddle();
                return;
            }

            pillarDelegates = new Action<bool>[targetPillars.Count];

            for (int i = 0; i < targetPillars.Count; i++)
            {
                if (targetPillars[i] != null)
                {
                    int index = i;
                    pillarDelegates[i] = (isEnemy) => HandlePillarDestroyed(index, isEnemy);
                    targetPillars[i].OnWallDestroyedBy += pillarDelegates[i];
                    targetPillars[i].OnWallHitByEnemy += pillarDelegates[i];
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

        private void HandlePillarDestroyed(int destroyedIndex, bool isEnemyProjectile)
        {
            if (IsSolved) return;

            if (!isEnemyProjectile && destroyedIndex == currentSequenceIndex)
            {
                currentSequenceIndex++;

                if (currentSequenceIndex >= targetPillars.Count)
                {
                    CompleteRiddle();
                }
            }
            else
            {
                OnSequenceFailed?.Invoke();
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

        public void ForceReset()
        {
            IsSolved = false;
            StopAllCoroutines();
            StartCoroutine(FailSequenceCoroutine());
        }

        private void OnDestroy()
        {
            if (pillarDelegates != null)
            {
                for (int i = 0; i < targetPillars.Count; i++)
                {
                    if (targetPillars[i] != null && pillarDelegates[i] != null)
                    {
                        targetPillars[i].OnWallDestroyedBy -= pillarDelegates[i];
                        targetPillars[i].OnWallHitByEnemy -= pillarDelegates[i];
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
