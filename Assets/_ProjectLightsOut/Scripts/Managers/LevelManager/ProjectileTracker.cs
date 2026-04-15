using System.Collections;
using ProjectLightsOut.DevUtils;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    /// <summary>
    /// Tracks active projectile count and triggers game-over or level-complete
    /// based on projectile/enemy state. Extracted from LevelManager.
    /// </summary>
    public class ProjectileTracker : MonoBehaviour
    {
        private int activeProjectiles = 0;
        private int bulletRemaining = 0;
        private bool isLevelComplete = false;
        private bool isGameOver = false;

        [SerializeField] private WaveManager waveManager;
        [SerializeField] private LevelFlowController flowController;
        private LevelProgressionValidator progressionValidator;

        private void Awake()
        {
            progressionValidator = FindObjectOfType<LevelProgressionValidator>();
        }

        private void OnEnable()
        {
            EventManager.AddListener<OnProjectileShoot>(OnProjectileShoot);
            EventManager.AddListener<OnProjectileDestroy>(OnProjectileDestroy);
            EventManager.AddListener<OnTriggerLevelComplete>(OnTriggerLevelComplete);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnProjectileShoot>(OnProjectileShoot);
            EventManager.RemoveListener<OnProjectileDestroy>(OnProjectileDestroy);
            EventManager.RemoveListener<OnTriggerLevelComplete>(OnTriggerLevelComplete);
        }

        private void OnTriggerLevelComplete(OnTriggerLevelComplete evt)
        {
            isLevelComplete = true;

            if (activeProjectiles == 0)
            {
                StartCoroutine(LevelComplete());
            }
        }

        private void OnProjectileShoot(OnProjectileShoot evt)
        {
            activeProjectiles++;
            bulletRemaining = evt.BulletLeft;
        }

        private void OnProjectileDestroy(OnProjectileDestroy evt)
        {
            activeProjectiles--;

            if (activeProjectiles == 0 && isLevelComplete)
            {
                StartCoroutine(LevelComplete());
            }

            if (activeProjectiles == 0 && bulletRemaining <= 0 && !isLevelComplete)
            {
                bool definitelyStuck = false;

                if (waveManager != null && waveManager.Enemies.Count > 0)
                {
                    definitelyStuck = true; // Still enemies alive
                }
                else if (progressionValidator != null && progressionValidator.HasUnsolvedRiddles())
                {
                    definitelyStuck = true; // Stuck staring at a riddle with no ammo
                }

                if (definitelyStuck && !LevelManager.LevelData.IsBossLevel)
                {
                    StartCoroutine(GameOver());
                }
            }
        }


        private IEnumerator GameOver()
        {
            isGameOver = true;
            EventManager.Broadcast(new OnPlayBGM("GameOver", fadeIn: 1f));
            yield return new WaitForSeconds(1f);
            EventManager.Broadcast(new OnPlayerEnableShooting(false));
            yield return new WaitForSeconds(1f);
            EventManager.Broadcast(new OnGameOver());
        }

        private IEnumerator LevelComplete()
        {
            if (isGameOver) yield break;
            yield return new WaitForSeconds(1f);

            EventManager.Broadcast(new OnPlayerEnableShooting(false));

            yield return new WaitForSeconds(2f);

            if (flowController == null) yield break;
            EventManager.Broadcast(new OnPlayerMove(true, flowController.EndWaypoints));

            float timeElapsed = flowController.TimeElapsed;
            EventManager.Broadcast(new OnLevelComplete(
                LevelManager.LevelData.LevelScore,
                bulletRemaining,
                LevelManager.LevelData.AceTime - timeElapsed
            ));
        }
    }
}
