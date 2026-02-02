# Procedural Logic Integration Documentation

## 1. Architecture Overview
The Procedural Logic system is now integrated directly into the core `LevelManager` loop. Instead of manually placing every enemy, "Procedural Waves" allow the game to generate valid, solving-guaranteed enemy layouts at runtime.

### Data Flow
1.  **LevelManager**: Orchestrates the level flow. When it encounters a Wave, it checks `WaveDataSO.IsProcedural`.
2.  **ProceduralEnemySpawner**: Acts as a service (Singleton). If a wave is procedural, it takes control to generate the wave.
3.  **SpecularPathGenerator**: Generates a valid ricochet path (geometry).
4.  **CSPValidator**: Ensures valid enemy positions on that path (constraints).
5.  **WaveDataSO**: Acts as the configuration source for both manual and procedural waves.

## 2. Changes Implemented

### `WaveDataSO.cs`
- Added `bool IsProcedural`: Flag to switch between manual list and procedural generation.
- Added `ProceduralWaveSettings ProceduralSettings`: Configuration struct containing:
    - `EnemyCount`: How many enemies to spawn.
    - `EnemyPool`: List of prefabs to choose from.
    - `MinPathLength`: (Optional) Override for path generation.
    - `MaxBounces`: (Optional) Override for path generation.

### `ProceduralEnemySpawner.cs`
- Converted to **Singleton** pattern (`ProceduralEnemySpawner.Instance`).
- Added `SpawnWave(ProceduralWaveSettings settings)` method.
- Implemented `Respawn()` using cached settings from the last wave.

### `LevelManager.cs`
- Modified `SpawnWave` coroutine to delegate to `ProceduralEnemySpawner` if `IsProcedural` is true.

## 3. How to Use (For Designers)

### Creating a Procedural Wave
1.  **Create/Select a Wave Asset**: Go to `Create -> ProjectLightsOut -> Wave Data` (or select an existing one).
2.  **Enable Procedural Mode**: Check the `Is Procedural` box.
3.  **Configure Settings**:
    -   **Enemy Count**: Set the number of enemies you want (e.g., 5).
    -   **Enemy Pool**: Add the Enemy Prefabs you want to appear in this wave. The spawner will pick randomly from this list.
    -   **Min Path Length**: (Optional) Leave at 0 for default, or set higher for more complex levels.
4.  **Assign to Level**: Drag this `WaveDataSO` into the `Waves` list of your `LevelDataSO` just like a normal wave.

### Runtime Behavior
- When the level reaches this wave, the system will automatically:
    -   Generate a "perfect ricochet path".
    -   Spawn the specified number of enemies at valid positions.
    -   Register them to the LevelManager so the level progresses naturally when they are killed.
