// =============================================================================
// EventRegistry.cs — Central Event Reference (Documentation Only)
// =============================================================================
//
// This file is NOT compiled. It serves as a single-page reference for all
// GameEvent classes in the project, grouped by domain.
//
// For each event: signature, defined-in file, and typical broadcasters/listeners.
//
// Last updated: 2026-02-23
// =============================================================================

// ─────────────────────────────────────────────────────────────────────────────
// AUDIO  (AudioManagerEvent.cs)
// ─────────────────────────────────────────────────────────────────────────────
//
// OnPlaySFX(string audioName)
//   Broadcasters: Enemy, Boss, WaveManager, ProjectileTracker, LevelFlowController, PlayerShoot
//   Listeners:    AudioManager
//
// OnPlayBGM(string audioName, AudioClip clip = null, float fadeIn = 3f)
//   Broadcasters: LevelFlowController, ProjectileTracker
//   Listeners:    AudioManager
//
// OnStopBGM(string audioName, float fadeOut = 3f)
//   Broadcasters: AppStateManager
//   Listeners:    AudioManager

// ─────────────────────────────────────────────────────────────────────────────
// CAMERA  (CameraManagerEvent.cs)
// ─────────────────────────────────────────────────────────────────────────────
//
// OnCameraShake(float duration, float magnitude)
//   Broadcasters: PlayerShoot
//   Listeners:    CameraManager
//
// OnSpotting(Transform target, float moveTime = 1f)
//   Broadcasters: LevelFlowController, WaveManager, BossStunPhase, BossDeadPhase, Boss
//   Listeners:    CameraManager
//
// OnSpottingEnd(float moveTime = 1f)
//   Broadcasters: LevelFlowController, WaveManager, BossStunPhase, BossDeadPhase, Boss
//   Listeners:    CameraManager
//
// OnZoom(float zoom, float zoomSpeed)
//   Broadcasters: LevelFlowController, WaveManager, BossStunPhase, BossDeadPhase, Boss
//   Listeners:    CameraManager
//
// OnZoomEnd(float zoomSpeed)
//   Broadcasters: LevelFlowController, WaveManager, BossStunPhase, BossDeadPhase, Boss
//   Listeners:    CameraManager

// ─────────────────────────────────────────────────────────────────────────────
// GAME STATE  (GameManagerEvent.cs)
// ─────────────────────────────────────────────────────────────────────────────
//
// OnSlowTime(float timeScale = 0.5f, float duration = 0.5f)
//   Broadcasters: WaveManager, BossStunPhase, BossDeadPhase
//   Listeners:    GameManager
//
// OnChangeGameState(GameState gameState)
//   Broadcasters: GameManager
//   Listeners:    (internal to GameManager)
//
// OnChangeScene(string sceneName, float delay = 1f)
//   Broadcasters: AppStateManager
//   Listeners:    GameManager
//
// OnFadeBlack()
//   Broadcasters: AppStateManager
//   Listeners:    FadeBlackUI
//
// OnGameOver()
//   Broadcasters: ProjectileTracker
//   Listeners:    GameManager, GameOverUI, HUDScoreUI
//
// OnPause(bool isPaused)
//   Broadcasters: GameManager
//   Listeners:    PauseMenu, HUDScoreUI

// ─────────────────────────────────────────────────────────────────────────────
// LEVEL / GAMEPLAY  (LevelManagerEvent.cs)
// ─────────────────────────────────────────────────────────────────────────────
//
// OnEnemyRegister(Enemy enemy)
//   Broadcasters: Enemy.Start()
//   Listeners:    WaveManager, Boss
//
// OnBossRegister(Boss boss)
//   Broadcasters: Boss.Start()
//   Listeners:    LevelManager
//
// OnEnemyDead(Enemy enemy)
//   Broadcasters: Enemy.OnHit()
//   Listeners:    WaveManager, Boss
//
// OnProjectileShoot(int bulletLeft)
//   Broadcasters: PlayerShoot
//   Listeners:    ProjectileTracker
//
// OnProjectileDestroy()
//   Broadcasters: Projectile
//   Listeners:    ProjectileTracker
//
// OnTriggerGameOver()
//   Broadcasters: (currently unused — game over is triggered internally)
//   Listeners:    (none found)
//
// OnVoidOrbHitPlayer()
//   Broadcasters: VoidOrb (on contact with Player-tagged collider)
//   Listeners:    AwakenedAzalethBoss (triggers instant game-over sequence)
//
// OnTriggerLevelComplete()
//   Broadcasters: WaveManager (all enemies dead), BossDeadPhase
//   Listeners:    ProjectileTracker
//
// OnBossDead()
//   Broadcasters: BossDeadPhase
//   Listeners:    HUDBossHealthBar, HUDBossWinBar
//
// OnLevelComplete(int levelBonus, int bulletRemaining, float levelTimeRemaining)
//   Broadcasters: ProjectileTracker
//   Listeners:    HUDScoreUI, ScoreManager
//
// OnBulletReload(int bullets)
//   Broadcasters: PlayerShoot
//   Listeners:    HUDBulletUI
//
// OnGrantReload(int bullets)
//   Broadcasters: PlayerShoot
//   Listeners:    PlayerShoot (self-reload on boss level)
//
// OnPlayerMove(bool isMoving, List<Transform> waypoints)
//   Broadcasters: LevelFlowController, ProjectileTracker
//   Listeners:    PlayerMove
//
// OnPlayerFinishMove()
//   Broadcasters: PlayerMove
//   Listeners:    LevelFlowController
//
// OnPlayerEnableShooting(bool isEnabled)
//   Broadcasters: LevelFlowController, ProjectileTracker, Boss
//   Listeners:    LevelManager, PlayerShoot, HUDBossWinBar, EnemyHealer, EnemyChanter
//
// OnBossLevel()
//   Broadcasters: (currently unused)
//   Listeners:    (none found)
//
// OnBossReady(Boss boss)
//   Broadcasters: Boss.ReadyBossSequence()
//   Listeners:    HUDBossHealthBar, HUDBossWinBar
//
// OnReadyBoss()
//   Broadcasters: LevelFlowController
//   Listeners:    Boss

// ─────────────────────────────────────────────────────────────────────────────
// SCORE  (ScoreEvents.cs)
// ─────────────────────────────────────────────────────────────────────────────
//
// OnScoreChange(int score)
//   Broadcasters: ScoreManager
//   Listeners:    HUDScoreUI
//
// OnAddScore(int score)
//   Broadcasters: Enemy.OnHit()
//   Listeners:    ScoreManager
//
// OnPostScore(int score, int timeBonus = 0, int levelBonus = 0, int bulletBonus = 0)
//   Broadcasters: ScoreManager
//   Listeners:    TotalScoreCounterUI
//
// OnCompleteCountingScore()
//   Broadcasters: TotalScoreCounterUI
//   Listeners:    LevelFlowController
//
// OnResetScore()
//   Broadcasters: AppStateManager
//   Listeners:    ScoreManager
//
// OnRollbackScore()
//   Broadcasters: GameOverUI
//   Listeners:    ScoreManager

// ─────────────────────────────────────────────────────────────────────────────
// ENEMY ABILITIES  (EnemyChanter.cs, BossBuffThread.cs)
// ─────────────────────────────────────────────────────────────────────────────
//
// OnEnemyChant(Transform chanter)
//   Broadcasters: EnemyChanter
//   Listeners:    (visual effect subscribers)
//
// OnBossBuff(BuffType buffType)
//   Broadcasters: BossBuffThread
//   Listeners:    Boss (delegated to current IBossPhase)
//   BuffType enum: Health, Shield
