# Procedural Logic Integration Plan

## Overview
This document outlines the plan to integrate the `ProceduralLogic` system (ProceduralEnemySpawner, SpecularPathGenerator, CSPValidator) into the existing `LevelManager` architecture using `LevelDataSO` and `WaveDataSO`.

## 1. Data Structure Modifications

### `WaveDataSO.cs`
We will extend `WaveDataSO` to support procedural generation settings. This allows a Level Designer to mix manually placed waves with procedural waves.

**Additions:**
- `bool IsProcedural`: Toggle for procedural generation.
- `ProceduralWaveSettings Settings`: Configuration for the procedural generation.

```csharp
[Serializable]
public struct ProceduralWaveSettings
{
    public int EnemyCount;
    public List<GameObject> EnemyPool; // List of possible enemies to spawn
    // Optional overrides for PCG
    public float MinPathLength; 
    public int MaxBounces;
}
```

## 2. Component Updates

### `ProceduralEnemySpawner.cs` (Refactor)
- **Singleton Pattern**: Implement `Singleton<ProceduralEnemySpawner>` (or standard static instance) to be easily called by `LevelManager`.
- **New Method**: `SpawnWave(ProceduralWaveSettings settings)`
    - Instead of using inspector-referenced `enemyPrefab`, it will pick random prefabs from `settings.EnemyPool`.
    - Instead of inspector-defined settings, it can accept settings from the WaveData.
- **Integration**:
    - The `Instantiate` call will still trigger `Enemy.Start()` -> `LevelManager.OnEnemyRegister`, so no manual registration tracking is needed.

### `LevelManager.cs` (Refactor)
- Update `SpawnWave(WaveDataSO waveData)`:
    - Add a branch condition:
      ```csharp
      if (waveData.IsProcedural)
      {
          ProceduralEnemySpawner.Instance.SpawnWave(waveData.ProceduralSettings);
          yield break; 
      }
      ```
    - Ensure `ProceduralEnemySpawner` is present in the scene (or instantiated if missing, though likely it should be placed in the scene or `Managers` prefab).

## 3. Execution Flow

1. `LevelManager` starts a wave.
2. Checks `WaveData.IsProcedural`.
3. Calls `ProceduralEnemySpawner`.
4. `ProceduralEnemySpawner`:
   - Runs `SpecularPathGenerator` to get a path.
   - Runs `CSPValidator` to get valid positions.
   - Instantiates enemies from the `EnemyPool` at valid positions.
5. `Enemy` instances register themselves to `LevelManager` via `EventManager`.
6. `LevelManager` tracks `enemies.Count` as usual for wave completion.

## 4. Work Checklist
- [ ] Modify `WaveDataSO.cs` to include `ProceduralWaveSettings`.
- [ ] Refactor `ProceduralEnemySpawner.cs` to accept settings and work as a service.
- [ ] Update `LevelManager.cs` to delegate procedural waves to the spawner.
