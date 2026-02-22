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

            if (activeProjectiles == 0 && GetWaveManager().Enemies.Count > 0 && bulletRemaining <= 0)
            {
                if (LevelManager.LevelData.IsBossLevel)
                {
                    // Boss levels don't game-over on empty bullets
                }
                else
                {
                    StartCoroutine(GameOver());
                }
            }
        }

        private WaveManager GetWaveManager()
        {
            return GetComponent<WaveManager>();
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

            var flowController = GetComponent<LevelFlowController>();
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
