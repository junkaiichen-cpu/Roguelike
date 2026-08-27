# PROJECT_STATUS.md

# Faith Fight / Bible Survivors
# Current Project Status

Last Updated:
2026-08-27

---

# 1. Current Phase

Current Phase:
Phase 1.6

Phase Name:
Complete and Stabilize the First Playable Survivors Loop

Status:
IN PROGRESS

---

# 2. Completed Phases

## Phase 0 — Documentation Foundation

Status:
COMPLETE

Implemented:

- Architecture documentation
- Game design documentation
- Narrative design documentation
- Content pipeline documentation
- Development roadmap

No gameplay systems were changed.

---

## Phase 1.1 — Player Health and Death

Status:
COMPLETE

Implemented:

- PlayerRuntimeState
- deterministic health
- damage/healing clamping
- terminal death state
- Player death event
- GameManager death handling
- gameplay pause after death

Tests:
PASS

---

## Phase 1.2 — Enemy Runtime and Death

Status:
COMPLETE

Implemented:

- EnemyRuntimeState
- terminal enemy death
- generic enemy damage/death flow
- XP reward triggered by actual death
- no XP reward from tree removal

Tests:
PASS

---

## Phase 1.3 — Automatic Projectile Combat

Status:
COMPLETE

Implemented:

- ProjectileWeaponDefinition
- automatic targeting
- nearest living enemy targeting
- projectile spawning
- projectile damage
- fire rate
- projectile speed
- stop firing after player death

Tests:
PASS

---

## Phase 1.4 — XP and Level Progression

Status:
COMPLETE

Implemented:

- PlayerProgressionState
- XP accumulation
- XP thresholds
- XP overflow
- multiple level progression
- XP pickup
- XP collection
- level-up event
- temporary upgrade selection
- neutral development upgrades
- gameplay pause during upgrade selection
- gameplay resume after selection

Tests:
26 passed

Bible content:
NONE

Later systems:
NOT IMPLEMENTED

---

## Phase 1.5 — Escalating Stage Pressure

Status:
COMPLETE

Goal:

- run timer
- escalating enemy pressure
- spawn-rate progression
- population scaling
- stage completion

Implemented:

- `RunPressureState`, a Godot-independent lifecycle model with deterministic
  elapsed time, pause/resume, stopped, and completed states
- data-driven neutral stage-pressure Resources: 2, 3, 5, and 5 minute stages
  (a 15-minute development run)
- generic spawn-interval and active-population-cap scaling per stage
- GameManager integration that starts with the player, pauses during temporary
  upgrade selection, and stops normal pressure after player death or the final
  duration
- spawning and stage advancement disabled after death or run completion
- pure runtime tests for timing, lifecycle, exact/after/multiple thresholds,
  completion, scaling, and invalid values

Verification:

- `dotnet test Tests/BibleSurvivors.Core.Tests.csproj --no-restore`: PASS —
  35 tests passed
- `dotnet build Survivors Starter Kit.sln --no-restore`: PASS — 0 errors,
  0 warnings
- `git diff --check`: PASS
- Stage Resources and their GameManager load path: statically inspected
- Godot manual verification: unavailable because no Godot executable is
  available in this environment

Scenes changed:

- None for Phase 1.5

Resources added:

- `Stages/development_run_pressure.tres`
- four neutral `StagePressureDefinition` Resources under `Stages/`

Bible content and later-phase systems:

- None added. Elites, narrative, encounters, achievements, shop, persistence,
  and final end-of-run UI remain out of scope.

---

## Phase 1.6 — Complete the First Playable Survivors Loop

Status:
IN PROGRESS

Implemented:

- `RunResultState`, a pure state model for Running, Victory, and Defeat with
  terminal transition protection and reset behavior
- `BossDefinition` Resource and neutral `development_boss.tres`, referenced by
  the existing stage-pressure configuration as its completion boss
- deterministic boss spawn when the configured timed pressure completes;
  normal enemy spawning and pressure timing stop at that point
- boss reuse of the existing Enemy health, damage, terminal death, pursuit,
  and XP-drop behavior
- victory from the configured boss's one-time death, and defeat from the
  existing player death path; either terminal state pauses normal gameplay
- minimal HUD result panel showing Victory or Defeat, level, elapsed time, and
  a Restart button
- restart that clears active enemies and reloads the main scene, creating new
  player health/XP/level, upgrades, pressure timer/stage, boss, and result
  state for the next run
- removal of the previous debug-key boss spawn from the active run flow
- a configuration-backed enemy spawn safety band: 30–36 units from the player,
  eight bounded attempts, and a deterministic safe-distance fallback
- an eight-second generic lifetime for the development bullet, so a missed
  projectile no longer remains as a live RigidBody3D indefinitely
- a lightweight shared enemy hit flash and restartable existing hit particles;
  no particle node or timer is duplicated per hit
- scene-exit cleanup of the HUD damage-label subscription and removal of stale
  dead-enemy references from the existing SpiritWater attack's overlap set

Lifecycle investigation:

- The ordinary enemy-death path is intact: `Enemy.TakeDamages` reaches its
  terminal `Died` event, `EnemyManager` removes that enemy from its active list
  and calls `GameManager.SpawnExperiencePickup` before `Enemy.QueueFree`.
  XP reward behavior was not changed because no code path randomly suppresses
  an active-run death drop.
- Before this stabilization work, bullets were only freed by `BodyEntered` and
  the bullet scene had no timeout. A miss therefore stayed in `MainScene`
  forever; sustained fire made RigidBody3D count grow without bound.
- Existing damage particles had a finite lifetime but were duplicated per hit.
  They now reuse the enemy scene's existing one-shot particle node instead.

Files changed:

- `Scripts/GameManager.cs`
- `Scripts/Enemies/Enemy.cs`
- `Scripts/Enemies/EnemyManager.cs`
- `Scripts/Enemies/BossDefinition.cs`
- `Scripts/Enemies/EnemySpawnConfiguration.cs`
- `Scripts/Run/RunResultState.cs`
- `Scripts/Run/StagePressureConfiguration.cs`
- `Scripts/Players/ProjectileLifetime.cs`
- `Scripts/Players/ProjectileLifetimeState.cs`
- `Scripts/Players/Shooting.cs`
- `Scripts/Players/SpiritWater.cs`
- `Scripts/UI/DamageLabelManager.cs`
- `Scripts/UI/RunResultsView.cs`
- `Tests/Run/RunResultStateTests.cs`
- `Tests/Enemies/EnemySpawnPlacementTests.cs`
- `Tests/Players/ProjectileLifetimeStateTests.cs`
- `Tests/BibleSurvivors.Core.Tests.csproj`
- `Scenes/HUD.tscn`
- `Stages/development_run_pressure.tres`
- `Bosses/development_boss.tres`
- `project.godot` — removed the obsolete debug Boss input action

Scenes changed:

- `Scenes/HUD.tscn` — minimal result panel only

Prefabs changed:

- `Prefabs/Powerups/bullet.tscn` — lifetime script and neutral eight-second
  timeout

Resources added/changed:

- `Bosses/development_boss.tres`
- `Stages/development_run_pressure.tres` — completion boss plus development
  spawn safety-band values

Verification:

- `dotnet test Tests/BibleSurvivors.Core.Tests.csproj --no-restore`: PASS —
  58 tests passed
- `dotnet build Survivors Starter Kit.sln --no-restore`: PASS — 0 errors,
  1 warning (`Enemy.SetName` hides Godot's base member; this existing unused
  method is outside the stabilization scope)
- Godot 4.4.1 editor and affected scene/resource wiring: statically inspected
- Godot 4.4.1 windowed/fullscreen/restart gameplay tests: NOT VERIFIED. The
  project cannot currently run because the pre-existing KayKit FBX `.import`
  file is `valid=false` and its imported `.scn` cache is absent. All enemy
  scenes that reference it fail to parse. A Godot 4.4.1 re-import attempt did
  not repair the metadata.

Known limitations:

- The manual gameplay acceptance tests are blocked by the invalid third-party
  FBX import metadata described above. The asset and its pre-existing import
  change were not replaced or manually edited in this stabilization task.
- Spawn validation ensures a finite player-distance band only. It does not yet
  perform navigation, obstacle, camera-frustum, or inter-enemy-separation
  validation; no such stage-world contract exists in the current architecture.
- The result UI and scene-reload restart are functional foundations only; no
  final visual polish, transition, reward, or persistence behavior exists.

Bible content and later-phase systems:

- No Bible-specific content, rewards, achievements, persistence, shops,
  weapon evolution/fusion, elite systems, or advanced boss behavior added.

---

# 3. Current Playable Loop

Current expected loop:

Player starts
→ enemies spawn
→ player automatically attacks
→ enemies die
→ XP appears
→ player collects XP
→ player levels up
→ upgrade selection appears
→ upgrade is applied
→ gameplay resumes
→ enemy pressure increases
→ configured pressure duration completes
→ configured development boss appears
→ victory or defeat
→ results
→ restart with a fresh run

Current status:

FIRST PLAYABLE LOOP CODE IMPLEMENTED; PHASE 1.6 STABILIZATION AWAITS GODOT
MANUAL VERIFICATION AFTER THE FBX IMPORT BLOCKER IS RESOLVED

Missing from MVP:

- end-to-end Godot playtest evidence
- final balancing and polished presentation

---

# 4. Next Required Phase

Next:

Resolve the KayKit FBX import metadata/cache with a reviewed Godot 4.4.1-safe
asset recovery, then resume the Phase 1.6 windowed, fullscreen, sustained
combat, boss, and restart acceptance tests. Do not begin a new phase first.

---

# 5. Current Architecture

Core runtime currently includes:

Player
Enemy
Projectile
Projectile Lifetime
XP Pickup
Player Runtime State
Enemy Runtime State
Player Progression State
Run Pressure State
Run Result State
GameManager
EnemyManager
UpgradeView
Run Results View

Architecture principles:

- C# defines behavior
- Godot Resources define content/data
- pure runtime states contain deterministic logic
- Godot Nodes handle engine integration
- Bible content is not hard-coded into gameplay systems

---

# 6. Current Content State

Bible characters:
NOT IMPLEMENTED

Bible weapons:
NOT IMPLEMENTED

Bible enemies:
NOT IMPLEMENTED

Bible stages:
NOT IMPLEMENTED

Narrative:
NOT IMPLEMENTED

Encounters:
NOT IMPLEMENTED

Scripture content:
NOT IMPLEMENTED

Quiz:
NOT IMPLEMENTED

Achievements:
NOT IMPLEMENTED

Meta progression:
NOT IMPLEMENTED

Shop:
NOT IMPLEMENTED

Save system:
NOT IMPLEMENTED

---

# 7. Testing Status

Automated tests:

PASS

Current test coverage includes:

- player health/death
- enemy runtime state
- projectile timing
- projectile lifetime and miss cleanup
- enemy spawn safety-band validation
- player XP progression
- level thresholds
- XP overflow
- multiple level-ups
- temporary upgrade behavior
- run pressure progression
- terminal run-result transitions

Build:

PASS

Godot manual verification:

BLOCKED

Reason:

Godot 4.4.1 is available, but the project cannot enter gameplay while the
pre-existing KayKit FBX import metadata is invalid. Phase 1.6 C#, scene, and
Resource wiring were statically inspected instead.

Scene/resource wiring should therefore be inspected statically until
Godot execution is available.

---

# 8. Known Technical Debt

Current known issues:

- GameManager still owns several run-level responsibilities
- some Starter Kit systems remain tightly coupled
- static scene paths remain in parts of the project
- legacy upgrade resources remain for compatibility
- scene composition has not yet been comprehensively refactored
- third-party KayKit FBX import metadata is invalid and prevents in-engine
  enemy-scene loading
- result presentation is intentionally minimal and has no completed manual
  playtest evidence in this environment

Do not perform a large refactor unless required by the current phase.

---

# 9. Explicitly Forbidden Until Later Phases

Do not implement unless explicitly requested:

- Bible characters
- Bible weapons
- weapon fusion
- weapon evolution
- Biblical stages
- narrative
- encounters
- quizzes
- achievements
- shop
- meta progression
- save system
- advanced enemy types
- elites
- final balancing

---

# 10. Development Rule

The repository is the source of truth.

Before implementation:

1. Read AGENTS.md
2. Read PROJECT_VISION.md
3. Read PROJECT_STATUS.md
4. Read DEVELOPMENT_ROADMAP.md
5. Inspect current implementation
6. Determine what is already complete
7. Implement only the requested phase

After implementation:

1. Run tests
2. Build project
3. Run git diff --check
4. Update PROJECT_STATUS.md
5. Update architecture/design documentation if necessary
6. Report changes
7. Stop at the requested phase
