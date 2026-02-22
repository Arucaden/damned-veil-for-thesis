# ProceduralLogic Setup Guide

The code integration is **already complete** — `LevelManager.SpawnWave()` already delegates to `ProceduralEnemySpawner` when a wave is marked procedural. You just need to set things up in the Unity Editor.

---

## Step 1: Create the ProceduralEnemySpawner GameObject

1. In your **Gameplay scene** (the scene where `LevelManager` lives), create an **empty GameObject**
2. Name it `ProceduralEnemySpawner`
3. Add these 3 components:

| Component | Script |
|---|---|
| `ProceduralEnemySpawner` | The orchestrator (already a Singleton) |
| `SpecularPathGenerator` | Generates ricochet paths via raycast |
| `CSPValidator` | Validates enemy positions along the path |

## Step 2: Configure SpecularPathGenerator

In the Inspector on the `SpecularPathGenerator` component:

| Field | Recommended Value | Notes |
|---|---|---|
| **Wall Layer** | Select your wall/ricochet layer | Must match the layer your walls use — the same layer your `Projectile` bounces off of. Likely `Ricochet` or `Wall` |
| **Max Bounces** | `6` | Matches your projectile's ricochet limit |
| **Max Ray Distance** | `100` | How far each ray segment travels |
| **Show Debug Gizmos** | ✅ (while testing) | Shows path lines in Scene view |

## Step 3: Configure CSPValidator

| Field | Recommended Value | Notes |
|---|---|---|
| **Safe Zone Radius** | `3` | Enemies won't spawn within 3 units of the player |
| **Min Enemy Spacing** | `2` | Enemies won't clump together |
| **Min Enemy Count** | `2` | Minimum enemies per wave (overridden by WaveData) |
| **Max Enemy Count** | `5` | Maximum enemies per wave |
| **Sampling Resolution** | `0.5` | How densely points are sampled along the path |
| **End Path Buffer** | `1` | Don't spawn enemies near the end of the path |
| **Show Debug Gizmos** | ✅ (while testing) | Shows valid/invalid positions in Scene view |

## Step 4: Configure ProceduralEnemySpawner

| Field | Recommended Value | Notes |
|---|---|---|
| **Path Generator** | Drag the same GameObject | Auto-fills if on same GameObject (`OnValidate`) |
| **CSP Validator** | Drag the same GameObject | Auto-fills if on same GameObject (`OnValidate`) |
| **Enemy Prefab** | Your default enemy prefab | Fallback only — procedural waves use `EnemyPool` from WaveData |
| **Player Transform** | Drag your Player GameObject | ⚠️ **Required** — the spawner needs the player's position |
| **Max Attempts** | `100` | Retry count before giving up |
| **Min Path Length** | `5` | Minimum total path length in world units |
| **Path Line Renderer** | *(Optional)* Add a `LineRenderer` component | Shows the generated path visually at runtime |
| **Show Path** | ✅ (while testing) | Toggle path visualization |
| **Log Debug Info** | ✅ (while testing) | Logs attempt count to Console |

## Step 5: Create a Procedural WaveDataSO

1. Right-click in your Project window → **Create → ProjectLightsOut → Wave Data**
2. Name it something like `Wave_Procedural_3Enemies`
3. In the Inspector:
   - ✅ **Is Procedural** — check this box
   - Under **Procedural Settings**:
     - **Enemy Count**: `3` (how many enemies to spawn)
     - **Enemy Pool**: Drag in your enemy prefabs (e.g., the basic `Enemy` prefab). The spawner picks randomly from this list
     - **Min Path Length**: `0` (use default) or set higher for longer paths
     - **Max Bounces**: `0` (use default) or override

> [!IMPORTANT]
> When `Is Procedural` is checked, the `Enemies` list (manual spawn positions) is **ignored**. You don't need to fill it.

## Step 6: Assign to a LevelDataSO

1. Open your `LevelDataSO` asset (e.g., `Level_1_Data`)
2. Add your procedural `WaveDataSO` to the **Waves** list — just like any other wave
3. You can mix manual and procedural waves in the same level:

```
Waves:
  [0] Wave_Manual_Tutorial     (IsProcedural = false)
  [1] Wave_Procedural_3Enemies (IsProcedural = true)   ← procedural!
  [2] Wave_Manual_BossEntrance (IsProcedural = false)
```

---

## How It Works at Runtime

```
LevelManager.CheckAllEnemiesDead()
   → enemies.Count == 0? → SpawnWave(nextWaveData)
      → waveData.IsProcedural?
         YES → ProceduralEnemySpawner.Instance.SpawnWave(settings)
                  → SpecularPathGenerator.GeneratePathAtAngle() (up to 100 random angles)
                  → CSPValidator.Solve() (find valid positions on path)
                  → Instantiate enemies from EnemyPool at positions
                  → Each Enemy.Spawn() → broadcasts OnEnemyRegister
                  → LevelManager tracks them normally
         NO  → Spawn enemies from manual EnemyData list (existing behavior)
```

---

## Verification Checklist

- [ ] `ProceduralEnemySpawner` GameObject exists in gameplay scene with all 3 components
- [ ] `Player Transform` is assigned on the spawner
- [ ] `Wall Layer` is set correctly on `SpecularPathGenerator` (must match your wall colliders' layer)
- [ ] At least one `WaveDataSO` has `Is Procedural = true` with `EnemyPool` populated
- [ ] That `WaveDataSO` is in a `LevelDataSO`'s `Waves` list
- [ ] Play the level → Console should show `[ProceduralEnemySpawner] Success after X attempts!`
- [ ] Enemies appear along the ricochet path and can be killed normally
- [ ] Level progresses to next wave / completes when all procedural enemies die
