# Development Log

This log records completed implementation milestones. `PROJECT_STATUS.md`
remains the authoritative snapshot of the repository's current state.

## 2026-08-26 — Phase 1.5: Escalating Stage Pressure

Status: COMPLETE

Added a minimal, data-driven timed-pressure foundation to the existing
Survivors loop. `RunPressureState` is a pure C# lifecycle and timing model;
neutral Godot Resources define four development stages totaling 15 minutes.
The existing `GameManager` now consumes that state to scale spawn interval and
active-enemy capacity, pause during temporary upgrade choices, stop after
player death, and enter a `RunCompleted` state at the configured duration.

Verification recorded at completion:

- `dotnet test Tests/BibleSurvivors.Core.Tests.csproj --no-restore` passed:
  35 tests.
- `dotnet build Survivors Starter Kit.sln --no-restore` passed with zero
  errors and zero warnings.
- `git diff --check` passed.
- No Godot executable was available, so scene and Resource wiring were
  statically inspected rather than manually played.

No Bible content, bosses, elites, narrative, encounters, persistence, or
other later-phase systems were added.

## 2026-08-26 — Phase 1.6: First Playable Survivors Loop

Status: COMPLETE

Added a neutral completion-boss path to the existing timed-pressure run. The
stage-pressure configuration now references one BossDefinition, which points
to the existing generic Enemy boss scene. At the final timed-pressure
milestone, normal spawning stops and that boss is spawned once. Its existing
terminal Enemy.Died event transitions a pure RunResultState to Victory; player
death transitions it to Defeat. A minimal HUD result panel reports the outcome,
level, and elapsed time, then reloads the current scene for a fresh run.
The unused SpawnBoss input action was removed from `project.godot`; the only
active Phase 1.6 boss path is the configured completion milestone.

Verification recorded at completion:

- `dotnet test Tests/BibleSurvivors.Core.Tests.csproj --no-restore` passed:
  39 tests.
- `dotnet build Survivors Starter Kit.sln --no-restore` passed with zero
  errors and zero warnings.
- `git diff --check` passed after the final documentation update.
- Godot executable unavailable; manual/in-engine verification could not be
  completed. C#, scene, and Resource wiring were statically inspected.

No Bible-specific boss/content, rewards, achievements, persistence, elite
systems, weapon evolution/fusion, or advanced boss behavior was added.

## 2026-08-27 — Phase 1.6 stabilization and gameplay verification

Status: IN PROGRESS

Investigation confirmed that missed development bullets had no terminal path:
they were freed only on collision, so each miss remained a RigidBody3D in the
main scene. The bullet now has an eight-second generic lifetime backed by pure,
tested state. Enemy spawn placement now consumes neutral stage-configuration
values for a 30–36 unit player-safe band, eight bounded attempts, and a
deterministic safe-distance fallback. The same stabilization adds a lightweight
reused enemy flash, reuses the existing per-enemy damage particle node instead
of duplicating one per hit, unregisters the damage-label HUD listener on scene
exit, and prevents the existing SpiritWater overlap collection from retaining
dead enemy references.

The XP code path was traced and preserved: only terminal `Enemy.Died` causes
EnemyManager to spawn the configured pickup before enemy cleanup. No evidence
showed a random active-run XP suppression path.

Verification recorded during stabilization:

- `dotnet test Tests/BibleSurvivors.Core.Tests.csproj --no-restore` passed:
  58 tests.
- `dotnet build Survivors Starter Kit.sln --no-restore` passed with zero
  errors and one existing `Enemy.SetName` hiding warning.
- Godot 4.4.1 opens the project, but windowed/fullscreen gameplay, restart,
  and sustained-combat verification are blocked: the existing KayKit FBX
  `.import` metadata is `valid=false` and lacks its imported `.scn` cache, so
  every enemy scene that references it fails to parse. A Godot 4.4.1 import
  pass did not repair it.

No Bible content, weapons/evolution, meta progression, characters, narrative,
or other later-phase systems were added.
