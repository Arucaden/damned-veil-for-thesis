# 🔍 Project Lights Out — Architecture Review (Part 3/3)
## Best Practices & Refactoring Recommendations

> Continued from [Part 2](file:///C:/Users/Arucaden/.gemini/antigravity/brain/2cd4a969-f687-4517-bf2c-e4ff5b1eab3b/architecture_review_part2.md)

---

## 7. Why These Patterns Matter (and Why Change)

This section explains the **reasoning** behind each recommendation — not just "what" to change, but "why" it's best practice.

---

### 7.1 Break Up `LevelManager` — Single Responsibility Principle (SRP)

**Why:** A class that subscribes to 9+ events and handles wave spawning, projectile tracking, game over logic, level transitions, AND boss sequences will break every time you add a feature. It makes debugging harder because any change could affect unrelated behavior.

**Recommended split:**

| New Class | Responsibility | Extracted From |
|---|---|---|
| `WaveManager` | Spawning waves (both manual and procedural), tracking `currentWave` | `SpawnWave()`, `CheckAllEnemiesDead()`, `OnEnemyRegister()`, `OnEnemyDead()` |
| `LevelFlowController` | Start → Play → Complete → Next orchestration | `StartLevel()`, `FinishStartMove()`, `LevelComplete()`, `GameOver()` |
| `ProjectileTracker` | Count active projectiles, trigger game-over/complete when 0 | `OnProjectileShoot()`, `OnProjectileDestroy()` |
| `LevelManager` (slimmed) | Hold `LevelDataSO`, expose level name, provide static accessors | Everything else |

**Example migration:**
```csharp
// Before: LevelManager.cs (303 lines, does everything)
// After:
public class WaveManager : MonoBehaviour
{
    [SerializeField] private LevelDataSO levelData;
    
    public void SpawnNextWave() { ... }
    private void OnEnemyDead(OnEnemyDead evt) { ... }
}

public class LevelFlowController : MonoBehaviour
{
    // Only orchestrates the high-level flow
    private IEnumerator StartLevel() { ... }
    private IEnumerator LevelComplete() { ... }
}
```

---

### 7.2 Extract Boss State Machine — State Pattern

**Why:** `Boss.cs` uses boolean flags (`isSecondPhase`, `isBossReady`, `isShieldDisabled`, `isSpawnNeeded`) to track state. This creates complex conditionals and is error-prone. A proper state pattern makes each phase's behavior explicit.

```csharp
// Recommended approach
public interface IBossState
{
    void Enter(Boss boss);
    void Update(Boss boss);
    void Exit(Boss boss);
}

public class BossIdleState : IBossState { ... }
public class BossPhase1State : IBossState { ... }
public class BossStunState : IBossState { ... }
public class BossPhase2State : IBossState { ... }
public class BossDeadState : IBossState { ... }
```

No need for a full FSM library — a simple interface with `Enter/Update/Exit` is sufficient for this scope.

---

### 7.3 Fix Anonymous Lambda Event Leaks

**Why:** In C#, `() => { Chant(); }` creates a new delegate instance every time. Two anonymous lambdas with identical body are still **different objects**. `RemoveListener` uses reference equality, so the unsubscribe silently fails, leaking the listener.

```diff
// ❌ Before (broken)
- OnSpawned += () => { Chant(); };
- OnSpawned -= () => { Chant(); };

// ✅ After (works)
+ OnSpawned += Chant;
+ OnSpawned -= Chant;
```

For `GameManager`:
```diff
// ❌ Before
- EventManager.AddListener<OnGameOver>(evt => { gameState = GameState.GameOver; });

// ✅ After
+ private void HandleGameOver(OnGameOver evt) { gameState = GameState.GameOver; }
+ EventManager.AddListener<OnGameOver>(HandleGameOver);
```

---

### 7.4 Normalize Namespaces

**Why:** Having 60% of classes in global scope means any class can accidentally shadow another. IDEs also can't provide proper "Go to Definition" grouping.

**Rule:** Every script file should have a namespace matching its folder path:

```
Scripts/Effects/         → namespace ProjectLightsOut.Effects
Scripts/Hittable/        → namespace ProjectLightsOut.Gameplay
Scripts/Player/          → namespace ProjectLightsOut.Gameplay
Scripts/Projectiles/     → namespace ProjectLightsOut.Gameplay
Scripts/UI/HUD/          → namespace ProjectLightsOut.UI
Scripts/UI/Menu/         → namespace ProjectLightsOut.UI
```

**Quick fix priority:** Start with `BossBuffThread.cs` (defines `BuffType` enum and `OnBossBuff` event in global scope), then `EnemyChanter.cs` (`OnEnemyChant` event in global scope).

---

### 7.5 Create an Event Registry

**Why:** With ~30 event classes spread across 6+ files, a new developer (or future you) has no way to know what events exist, who broadcasts them, and who listens.

**Recommendation:** Create a single reference file (not code, just documentation):

```csharp
// Events/EventRegistry.cs — Documentation only
// Each event class stays in its current file, but this provides a central index

/// AUDIO
/// OnPlaySFX(string) → AudioManager
/// OnPlayBGM(string, clip, fadeIn) → AudioManager
/// OnStopBGM(string, fadeOut) → AudioManager

/// CAMERA
/// OnCameraShake(duration, magnitude) → CameraManager
/// OnSpotting(target, moveTime) → CameraManager
/// OnSpottingEnd(moveTime) → CameraManager
/// OnZoom(zoom, speed) → CameraManager
/// OnZoomEnd(speed) → CameraManager

/// GAMEPLAY
/// OnEnemyRegister(Enemy) → LevelManager, Boss
/// OnEnemyDead(Enemy) → LevelManager, Boss
/// OnProjectileShoot(bulletsLeft) → LevelManager
/// OnProjectileDestroy() → LevelManager
/// ... etc
```

---

### 7.6 Fix `ProjectileAfterImageEffect` Memory Leak

```diff
private void CreateTrailEffect()
{
-   GameObject trailEffect = Instantiate(new GameObject(), ...);
+   GameObject trailEffect = new GameObject("TrailEffect");
+   trailEffect.transform.position = transform.position;
    // ... set up sprite renderer ...
+   Destroy(trailEffect, 0.5f); // Auto-destroy after fade time
}
```

Better yet, use an **object pool** for trail effects since they're created every 0.1s.

---

### 7.7 Consistent `Time.deltaTime` Strategy

**Rule:**
- UI animations that must work during pause → `Time.unscaledDeltaTime`
- Gameplay animations tied to game speed → `Time.deltaTime`

**Apply to:** `HUDScoreUI.Retract()` and `HUDScoreUI.Extend()` should use `Time.unscaledDeltaTime` since they trigger during pause transitions.

---

### 7.8 Split `OnPlayerEnableShooting`

Replace one overloaded event with specific events:

```csharp
// Before: OnPlayerEnableShooting does EVERYTHING

// After: Specific events for specific concerns
public class OnPlayerShootingChanged : GameEvent { public bool IsEnabled; }
public class OnHUDVisibilityChanged : GameEvent { public bool IsVisible; }
public class OnGameplayPaused : GameEvent { public bool IsPaused; }
```

This separates concerns so you can independently control shooting, HUD, and pause state.

---

### 7.9 Object Pooling for Projectiles & Effects

**Why:** `Instantiate` and `Destroy` are expensive. Projectiles, impact effects, trail effects, and spawn effects are created/destroyed frequently. A simple pool would reduce GC pressure.

```csharp
// Simple pool pattern — no dependencies needed
public class SimplePool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialSize = 10;
    private Queue<GameObject> pool = new Queue<GameObject>();
    
    public GameObject Get() { ... }
    public void Return(GameObject obj) { ... }
}
```

---

## 8. Prioritized Refactoring Roadmap

| Priority | Task | Impact | Effort |
|---|---|---|---|
| 🔴 P0 | Fix anonymous lambda event leaks | Prevents listener leaks / ghost behavior | 30 min |
| 🔴 P0 | Fix `ProjectileAfterImageEffect` memory leak | Prevents memory leak during gameplay | 15 min |
| 🟡 P1 | Add namespaces to all global-scope classes | Code organization, prevents collisions | 1-2 hrs |
| 🟡 P1 | Fix `async void` → add try-catch at minimum | Prevents silent failures | 30 min |
| 🟡 P1 | Fix `Boss.TrySpawnWave()` dead code | Boss wave spawning may not work as intended | 15 min |
| 🟢 P2 | Break up `LevelManager` | Architecture health, easier to extend | 3-4 hrs |
| 🟢 P2 | Extract Boss state machine | Cleaner boss behavior, future phases | 2-3 hrs |
| 🟢 P2 | Consistent `Time.deltaTime` strategy | Fixes pause-related UI bugs | 30 min |
| 🔵 P3 | Split `OnPlayerEnableShooting` | Future flexibility | 1-2 hrs |
| 🔵 P3 | Object pooling | Performance optimization | 2-3 hrs |
| 🔵 P3 | Event registry documentation | Maintainability | 1 hr |

---

## 9. Overall Score

| Category | Score | Notes |
|---|---|---|
| **Code Organization** | 7/10 | Great folder structure, but namespace gaps |
| **Architecture** | 6/10 | Good foundations (events, singletons, SO data), but LevelManager/Boss are overloaded |
| **Code Quality** | 5/10 | Several bugs (lambda leaks, memory leaks, dead code), commented-out code, magic numbers |
| **Separation of Concerns** | 6/10 | Event system helps, but mixed coupling and god objects hurt |
| **Extensibility** | 5/10 | Adding new enemies or wave types is easy, but new game flow features will touch LevelManager |
| **ProceduralLogic Module** | 8/10 | Best-engineered part — clean separation, good docs, well-structured |
| **UI Layer** | 6/10 | Functional but lots of animation code duplication (Retract/Extend repeated in 4+ files) |

**Overall: 6/10** — A solid foundation for a thesis project with a strong procedural generation module, held back by some architectural debt and specific bugs that should be addressed before expanding further.

---

## 10. Where This Analysis Left Off

This review covered **all ~37 C# source files** in `Assets/_ProjectLightsOut/Scripts/`. The following areas were **NOT** analyzed (out of scope):
- **Unity Scene files** — hierarchy structure, component wiring, prefab organization
- **ScriptableObject instances** — actual level data values, wave configurations
- **Art/Animation assets** — animator controllers, sprite setup
- **Build settings** — scene order, platform configs
- **Third-party packages** — TextMeshPro setup, URP configuration
- **Performance profiling** — actual runtime GC, draw calls, frame times
- **Existing plan docs** — `ProceduralLogic/plan.md` and `integration_plan.md` were noted but not reviewed against actual implementation

If you'd like me to dive deeper into any of these areas, or to start implementing any of the refactoring recommendations, just let me know!
