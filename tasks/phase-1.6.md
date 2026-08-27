# TASK: Phase 1.6 — Complete the First Playable Survivors Loop

PROJECT:

Faith Fight

ENGINE:

Godot 4

LANGUAGE:

C#

---

# GOAL

Complete the first fully playable Survivors-style MVP loop:

Run
→ normal enemies
→ automatic combat
→ XP
→ level up
→ temporary upgrades
→ escalating enemy pressure
→ boss
→ victory OR defeat
→ results
→ restart

This phase is the first complete gameplay milestone.

The objective is NOT to introduce Bible content.

The objective is to prove that the core gameplay loop is playable from
start to finish.

---

# CURRENT FOUNDATION

Phase 1.1:
Player health and terminal death.

Phase 1.2:
Enemy runtime death behavior.

Phase 1.3:
Generic automatic projectile combat.

Phase 1.4:
XP pickup, progression, level-up, temporary upgrades.

Phase 1.5:
Run timer, escalating pressure, stage progression, spawn-rate scaling,
population scaling, and deterministic run completion.

Preserve all completed functionality.

Do not reimplement completed systems.

---

# REQUIREMENTS

## 1. Run Start

A run must have a clear start state.

When the player starts the run:

- player is alive
- run progression starts
- enemy spawning begins
- run timer progresses
- combat is active

Reuse the existing Phase 1.5 run lifecycle.

Do not create a second independent run lifecycle.

---

# 2. Normal Enemy Combat

The existing enemy combat loop must remain functional:

Player
→ automatic projectile
→ enemy damage
→ enemy death
→ XP drop

Do not redesign the existing combat system.

Do not introduce new enemy types unless strictly necessary to implement
the boss foundation.

---

# 3. Escalating Pressure

Reuse Phase 1.5.

Enemy pressure must continue increasing during the run.

Do not create a second wave/scaling system.

The existing RunPressureState and stage configuration should remain the
source of truth for normal enemy pressure.

---

# 4. Boss

Introduce a generic reusable boss runtime architecture.

The boss must:

- use the existing enemy/combat architecture where practical
- have health
- receive damage
- die through the same terminal enemy death model
- be distinguishable from normal enemies through data/configuration
- appear according to the run progression condition

The boss must NOT contain Bible-specific content.

Do not create:

- David boss
- Goliath boss
- Pharaoh boss
- Biblical enemy names
- Biblical weapons
- story encounters

Use neutral development/test boss data.

Example:

Development Boss

---

# 5. Boss Spawn Condition

Boss appearance must be deterministic.

The boss should appear when the configured run milestone is reached.

Do not rely on random chance for the MVP boss spawn.

The boss spawn condition should be represented through reusable run/stage
data where practical.

Avoid hard-coding a specific future Biblical stage.

---

# 6. Boss Death

The boss must use the existing terminal enemy death behavior.

Boss death must:

- happen only once
- stop the boss from normal gameplay processing
- prevent duplicate death rewards
- produce the appropriate run-level victory transition

Do not create a separate incompatible boss combat system.

---

# 7. Victory State

Introduce a generic run victory state.

When the configured boss is defeated:

Run
→ Victory

Victory must:

- stop normal enemy spawning
- stop run progression
- stop normal combat progression
- prevent additional boss spawning
- transition to results

Do not implement:

- Bible rewards
- achievements
- narrative
- story completion
- permanent progression

---

# 8. Defeat State

Player death must produce:

Run
→ Defeat

Reuse Phase 1.1 and Phase 1.5 lifecycle behavior.

When the player dies:

- run progression stops
- enemy spawning stops
- gameplay stops
- victory must not trigger afterward
- transition to results

Defeat must be deterministic.

---

# 9. Results Screen

Create a minimal results interface.

The results screen only needs to prove the run ended.

It should distinguish:

- Victory
- Defeat

It may display minimal run information such as:

- result
- level reached
- elapsed run time

Do NOT implement:

- currency rewards
- achievements
- unlocks
- permanent upgrades
- narrative rewards
- shop

Those belong to later phases.

---

# 10. Restart

The player must be able to restart after:

- Victory
- Defeat

Restart should create a fresh run state.

The following temporary run state must reset:

- player HP
- player XP
- player level
- temporary upgrades
- run timer
- stage progression
- active enemies
- boss state
- victory/defeat state

Do not implement persistent save data.

Do not implement meta progression.

---

# 11. Generic Run Result State

If appropriate, introduce a small pure runtime state representing:

- Running
- Victory
- Defeat

The state should be deterministic and testable.

Do not create a large state-machine framework.

Prefer a small enum/state model if sufficient.

---

# 12. Architecture Constraints

Preserve:

- PlayerRuntimeState
- EnemyRuntimeState
- PlayerProgressionState
- RunPressureState
- existing projectile system
- existing enemy system
- existing GameManager coordination
- Resource-based architecture
- existing scene composition
- Jolt configuration

Do NOT perform a large GameManager rewrite.

Only extract a small runtime seam if required by this phase.

---

# 13. Boss Data

Boss configuration should be data-driven where practical.

Prefer a reusable Resource definition.

For example:

BossDefinition

or an appropriate extension of the existing enemy definition.

The exact architecture should follow the existing repository rather than
introducing an unnecessary framework.

The development boss must contain no Bible-specific content.

---

# 14. Results UI

Reuse existing HUD/UI primitives where possible.

Do not redesign the entire HUD.

The UI only needs to prove:

Victory
or
Defeat

and provide:

Restart

---

# 15. Testing

Add pure .NET tests for new deterministic runtime logic.

At minimum test:

## Run Result

- initial state
- running state
- victory transition
- defeat transition
- terminal state
- victory cannot become defeat
- defeat cannot become victory

## Boss

If boss runtime state is introduced:

- initial health
- damage
- death
- terminal death
- duplicate death prevention

## Restart

If restart state reset logic is pure:

- HP reset
- XP reset
- level reset
- timer reset
- stage reset
- result reset

Preserve all existing tests.

---

# 16. Verification

Run:

dotnet test Tests/BibleSurvivors.Core.Tests.csproj --no-restore

dotnet build Survivors Starter Kit.sln --no-restore

git diff --check

Inspect:

- affected C# files
- affected scenes
- affected Resources
- project settings if modified
- scene wiring
- signals
- resource references
- run lifecycle
- boss spawn lifecycle
- victory lifecycle
- defeat lifecycle
- restart lifecycle

If Godot is available:

Perform a manual end-to-end run.

Verify:

1. Start run.
2. Normal enemies spawn.
3. Player automatically attacks.
4. Enemies die.
5. XP drops.
6. XP can be collected.
7. Player levels up.
8. Upgrade selection appears.
9. Upgrade changes gameplay.
10. Enemy pressure increases.
11. Boss appears.
12. Boss can be damaged.
13. Boss can die.
14. Victory appears.
15. Results screen appears.
16. Restart starts a fresh run.

Also manually test defeat:

1. Start run.
2. Allow player to die.
3. Defeat appears.
4. Results screen appears.
5. Restart works.
6. New run starts from clean temporary state.

If Godot is unavailable:

State clearly:

"Godot manual verification was unavailable."

Do not claim that the complete gameplay loop was manually verified.

---

# 17. Documentation

After implementation update:

PROJECT_STATUS.md

docs/DEVELOPMENT_LOG.md

docs/DEVELOPMENT_ROADMAP.md

Update:

docs/ARCHITECTURE.md

if the architecture changed significantly.

Update:

docs/GAME_DESIGN.md

if gameplay behavior changed.

Do not document future architecture as if it already exists.

---

# 18. Scope Restrictions

DO NOT IMPLEMENT:

- Biblical boss content
- Biblical enemies
- Biblical weapons
- Bible characters
- story
- narrative
- encounters
- chapters
- scripture
- quizzes
- achievements
- shop
- currency
- meta progression
- save system
- weapon evolution
- weapon fusion
- passive item system
- elite enemy system
- advanced boss AI
- advanced boss phases
- final balancing
- final UI
- stage-specific Bible content

Only implement the minimum reusable foundation required for the first
playable MVP loop.

---

# 19. Completion Criteria

Phase 1.6 is complete only when:

Player starts a run.

↓

Normal enemies spawn.

↓

Player automatically attacks.

↓

Enemies die.

↓

XP drops.

↓

Player collects XP.

↓

Player levels up.

↓

Temporary upgrade is selected.

↓

Player becomes stronger.

↓

Enemy pressure increases.

↓

Boss appears.

↓

Boss can be damaged.

↓

Boss dies.

↓

Victory occurs.

↓

Results appear.

↓

Restart starts a new run.

AND:

Player death

↓

Defeat

↓

Results

↓

Restart

also works.

---

# 20. STOP CONDITION

After completing and verifying Phase 1.6:

STOP.

Do NOT start Phase 1.7.

Do NOT implement Bible content.

Do NOT implement meta progression.

Do NOT implement weapon evolution.

Do NOT implement weapon fusion.

Do NOT add unrelated improvements.

Report implementation and verification results only.