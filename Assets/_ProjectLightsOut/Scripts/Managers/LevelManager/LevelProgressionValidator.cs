using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Gameplay;

namespace ProjectLightsOut.Managers
{
    /// <summary>
    /// Acts as the gatekeeper for Level progression.
    /// Handles deciding when the level actually starts and when it is legally allowed to end.
    /// </summary>
    public class LevelProgressionValidator : MonoBehaviour
    {
        [SerializeField] private WaveManager waveManager;
        
        private BaseRiddle[] activeRiddles;
        private bool isLevelCompleteTriggered = false;
        private Enemy cachedLastEnemy;

        private void Awake()
        {
            // Auto-detect all optional riddles in the level
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
            // 1. Validator checks start: The player hit the waypoint, safe to start spawning waves.
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

        /// <summary>
        /// Continually checks the two main validation gates.
        /// </summary>
        private void CheckForLevelCompletion()
        {
            if (isLevelCompleteTriggered) return;

            // GATE 1: Are all waves exhausted and is the cinematic finished?
            if (waveManager == null || !waveManager.AllWavesDefeated || !isCombatCinematicFinished)
            {
                return;
            }

            // GATE 2: Are all interactive riddles solved?
            if (HasUnsolvedRiddles())
            {
                return; // Abort, wait for riddles
            }

            // PASS: Both gates cleared. Wait for cinematic to end, then trigger true level completion.
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
