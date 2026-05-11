using System.Collections;
using ProjectLightsOut.DevUtils;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
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
            Debug.Log($"[ProjectileTracker] OnTriggerLevelComplete received. isGameOver={isGameOver}, activeProjectiles={activeProjectiles}");
            isLevelComplete = true;

            if (activeProjectiles == 0)
            {
                Debug.Log("[ProjectileTracker] activeProjectiles=0, starting LevelComplete coroutine.");
                StartCoroutine(LevelComplete());
            }
            else
            {
                Debug.Log($"[ProjectileTracker] Waiting for {activeProjectiles} projectile(s) to be destroyed before LevelComplete.");
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
            Debug.Log($"[ProjectileTracker] Projectile destroyed. activeProjectiles={activeProjectiles}, isLevelComplete={isLevelComplete}");

            if (activeProjectiles == 0 && isLevelComplete)
            {
                Debug.Log("[ProjectileTracker] All projectiles gone + level complete flag set. Starting LevelComplete.");
                StartCoroutine(LevelComplete());
            }

            if (activeProjectiles == 0 && bulletRemaining <= 0 && !isLevelComplete)
            {
                bool definitelyStuck = false;

                if (waveManager != null && waveManager.Enemies.Count > 0)
                {
                    definitelyStuck = true;
                }
                else if (progressionValidator != null && progressionValidator.HasUnsolvedRiddles())
                {
                    definitelyStuck = true;
                }

                if (definitelyStuck && !LevelManager.LevelData.IsBossLevel)
                {
                    StartCoroutine(GameOver());
                }
            }
        }


        private IEnumerator GameOver()
        {
            Debug.Log("[ProjectileTracker] GameOver triggered!");
            isGameOver = true;
            EventManager.Broadcast(new OnPlayBGM("GameOver", fadeIn: 1f));
            yield return new WaitForSeconds(1f);
            EventManager.Broadcast(new OnPlayerEnableShooting(false));
            yield return new WaitForSeconds(1f);
            EventManager.Broadcast(new OnGameOver());
        }

        private IEnumerator LevelComplete()
        {
            Debug.Log($"[ProjectileTracker] LevelComplete started. isGameOver={isGameOver}, flowController={(flowController == null ? "NULL" : flowController.name)}");
            if (isGameOver) { Debug.LogWarning("[ProjectileTracker] isGameOver=true, aborting LevelComplete."); yield break; }
            yield return new WaitForSeconds(1f);

            Debug.Log("[ProjectileTracker] Disabling player shooting.");
            EventManager.Broadcast(new OnPlayerEnableShooting(false));

            yield return new WaitForSeconds(2f);

            if (flowController == null) { Debug.LogError("[ProjectileTracker] flowController is NULL! Cannot move player or fire OnLevelComplete. Assign it in the Inspector."); yield break; }
            Debug.Log("[ProjectileTracker] Moving player to end waypoints and firing OnLevelComplete.");
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
