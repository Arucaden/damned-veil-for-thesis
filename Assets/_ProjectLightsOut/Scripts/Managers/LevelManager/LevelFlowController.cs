using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    public class LevelFlowController : MonoBehaviour
    {
        [SerializeField] private List<Transform> startWaypoints = new List<Transform>();
        [SerializeField] private List<Transform> endWaypoints = new List<Transform>();
        [SerializeField] private bool instantlyZoomAtStart = false;
        [SerializeField] private float zoomLevel = 0.3f;

        public List<Transform> EndWaypoints => endWaypoints;

        [SerializeField] private WaveManager waveManager;

        private float timeElapsed = 0f;
        public float TimeElapsed => timeElapsed;

        private void OnEnable()
        {
            EventManager.AddListener<OnPlayerFinishMove>(OnPlayerFinishMove);
            EventManager.AddListener<OnCompleteCountingScore>(OnCompleteCountingScore);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnPlayerFinishMove>(OnPlayerFinishMove);
            EventManager.RemoveListener<OnCompleteCountingScore>(OnCompleteCountingScore);
        }

        private void Start()
        {
            Time.timeScale = 1f;
            Cursor.visible = false;

            if (!AudioManager.IsBGMPlaying)
            {
                EventManager.Broadcast(new OnPlayBGM("Gameplay", fadeIn: 1f));
            }

            if (LevelManager.LevelData.IsBossLevel)
            {
                EventManager.Broadcast(new OnPlayBGM("Boss", fadeIn: 0f));
            }

            if (startWaypoints.Count > 0)
            {
                StartCoroutine(StartLevel());
            }
            else
            {
                OnPlayerFinishMove(new OnPlayerFinishMove());
            }
        }

        private void Update()
        {
            timeElapsed += Time.deltaTime;
        }

        private IEnumerator StartLevel()
        {
            EventManager.Broadcast(new OnPlayerEnableShooting(false));

            yield return new WaitForSeconds(1f);

            if (instantlyZoomAtStart)
            {
                EventManager.Broadcast(new OnZoom(zoomLevel, 0f));
                EventManager.Broadcast(new OnSpotting(startWaypoints[startWaypoints.Count - 1], 0f));
            }
            else
            {
                EventManager.Broadcast(new OnZoom(zoomLevel, 1.7f));
                EventManager.Broadcast(new OnSpotting(startWaypoints[startWaypoints.Count - 1], 1.7f));
            }

            EventManager.Broadcast(new OnPlayerMove(true, startWaypoints));
        }

        private bool isStartMoveComplete = false;

        private void OnPlayerFinishMove(OnPlayerFinishMove evt)
        {
            if (isStartMoveComplete) return;
            isStartMoveComplete = true;
            StartCoroutine(FinishStartMove());
        }

        private IEnumerator FinishStartMove()
        {
            if (LevelManager.LevelData.IsBossLevel)
            {
                EventManager.Broadcast(new OnReadyBoss());
                yield break;
            }

            if (LevelManager.LevelData.TutorialPages != null && LevelManager.LevelData.TutorialPages.Count > 0)
            {
                EventManager.Broadcast(new OnChangeGameState(GameState.Tutorial));
                EventManager.Broadcast(new OnShowTutorial(LevelManager.LevelData.TutorialPages));
                
                while (GameManager.Instance.CurrentGameState == GameState.Tutorial)
                {
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.5f);
            EventManager.Broadcast(new OnSpottingEnd());
            EventManager.Broadcast(new OnZoomEnd(1f));
            yield return new WaitForSeconds(1.5f);

            EventManager.Broadcast(new OnPlayerEnableShooting(true));
        }

        private void OnCompleteCountingScore(OnCompleteCountingScore evt)
        {
            string nextLevel = LevelManager.LevelData.NextLevelScenes[Random.Range(0, LevelManager.LevelData.NextLevelScenes.Count)];
            AppStateManager.Instance.GoToNextLevel(nextLevel);
        }
    }
}
