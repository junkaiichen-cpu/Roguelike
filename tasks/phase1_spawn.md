# Phase 1 - Spawn and Map Foundation

# Status
- status: in_progress
- date: 2026-08-27

# Goal
Keep normal enemy spawning inside the playable area, preserve a safe 360-degree spawn ring, remove directional fallback bias, and make the player boundary use the same bounds data.

# Changes
- Restored a single `MapBoundary` in `Scenes/main_scene.tscn`; removed duplicate `West2`, `East2`, `North2`, `South2`, and incomplete `MapBoundary2` nodes.
- Reattached `PlayableBoundary.cs` to the single boundary body and set its collision mask to Player only.
- Kept the existing uniform `GD.Randf() * Mathf.Tau` ring sampling.
- Extended `EnemySpawnPlacement.IsWithinSpawnBand` to require both the safe distance band and `PlayableBounds` containment.
- Replaced the fixed eastward fallback with a deterministic 360-degree scan that selects the farthest valid point inside the shared bounds. It throws instead of returning an invalid position when no safe point exists.
- `EnemyManager` passes the same `PlayableBounds` to candidate and fallback validation.
- `StagePressureConfiguration` and `development_run_pressure.tres` provide the shared `-50..50` X/Z playable bounds used by spawning and boundary construction.
- Added focused tests for out-of-bounds candidates and in-bounds fallback behavior.

# Files
- `Scripts/Enemies/EnemySpawnConfiguration.cs`
- `Scripts/Enemies/EnemyManager.cs`
- `Scripts/Run/StagePressureConfiguration.cs`
- `Scripts/Run/PlayableBoundary.cs`
- `Stages/development_run_pressure.tres`
- `Scenes/main_scene.tscn`
- `Tests/Enemies/EnemySpawnPlacementTests.cs`

# Decisions
- The existing ring and uniform angle distribution remain unchanged because they already cover 360 degrees.
- Bounds are configuration data, not duplicated constants in runtime spawn logic.
- Boss spawning remains on its separate path and retains its original distance profile.
- Boundary walls collide with the player only, so they do not form an enemy or boss movement barrier.

# Known Issues
- A normal interactive Godot playtest is still required to confirm player edge blocking and visual four-direction spawn distribution. Headless startup passed; Godot reports existing exit-time RID/resource leak messages.
- The playable bounds currently use a rectangular `-50..50` development area and have no visual edge.

# Next Phase
After interactive Phase 1 verification, proceed to Phase 2 combat feel and projectile upgrade variety. Do not begin Phase 2 until the boundary and spawn distribution are manually confirmed.
