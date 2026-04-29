using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Gameplay;

namespace ProjectLightsOut.Managers
{
    public class LevelProgressionValidator : MonoBehaviour
    {
        [SerializeField] private WaveManager waveManager;
        
        private BaseRiddle[] activeRiddles;
        private bool isLevelCompleteTriggered = false;
        private Enemy cachedLastEnemy;

        private void Awake()
        {
            activeRiddles = FindObjectsOfType<BaseRiddle>();
        }

        private void OnEnable()
        {
            EventManager.AddListener<OnPlayerFinishMove>(OnPlayerFinishMove);
            EventManager.AddListener<OnCombatWavesCompleted>(OnCombatWavesCompleted);
            EventManager.AddListener<OnRiddleSolved>(OnRiddleSolved);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnPlayerFinishMove>(OnPlayerFinishMove);
            EventManager.RemoveListener<OnCombatWavesCompleted>(OnCombatWavesCompleted);
            EventManager.RemoveListener<OnRiddleSolved>(OnRiddleSolved);
        }

        private void OnPlayerFinishMove(OnPlayerFinishMove evt)
        {
            if (waveManager != null)
            {
                waveManager.TriggerInitialWaveCheck();
            }
        }

        private bool isCombatCinematicFinished = false;

        private void OnCombatWavesCompleted(OnCombatWavesCompleted evt)
        {
            StartCoroutine(HandleCombatEndCinematic(evt.LastEnemyDead));
        }

        private IEnumerator HandleCombatEndCinematic(Enemy lastEnemy)
        {
            if (lastEnemy != null)
            {
                EventManager.Broadcast(new OnPlaySFX("Bell"));
                EventManager.Broadcast(new OnSlowTime(0.1f, 1.2f));
                EventManager.Broadcast(new OnSpotting(lastEnemy.transform, 0.2f));
                EventManager.Broadcast(new OnZoom(-0.5f, 0.2f));

                yield return new WaitForSecondsRealtime(1.2f);

                EventManager.Broadcast(new OnSpottingEnd(0.4f));
                EventManager.Broadcast(new OnZoomEnd(0.4f));
            }

            isCombatCinematicFinished = true;
            CheckForLevelCompletion();
        }

        private void OnRiddleSolved(OnRiddleSolved evt)
        {
            CheckForLevelCompletion();
        }

        private void CheckForLevelCompletion()
        {
            if (isLevelCompleteTriggered) return;

            if (waveManager == null || !waveManager.AllWavesDefeated || !isCombatCinematicFinished)
            {
                return;
            }

            if (HasUnsolvedRiddles())
            {
                return;
            }

            isLevelCompleteTriggered = true;
            EventManager.Broadcast(new OnTriggerLevelComplete());
        }

        public bool HasUnsolvedRiddles()
        {
            foreach (var riddle in activeRiddles)
            {
                if (!riddle.IsSolved) return true;
            }
            return false;
        }
    }
}
