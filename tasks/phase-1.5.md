TASK: Phase 1.5 — Implement Escalating Stage Pressure

PROJECT:
Faith Fight
Godot 4 + C#

GOAL:

Build the next part of the Core Survivors Loop:

Run Start
→ Timed Stage Progression
→ Increasing Enemy Pressure
→ Combat
→ XP / Level Up / Upgrade
→ Stage Completion
→ Run Completion State

Phase 1.1–1.4 are already complete.
Preserve all existing working behavior.

---

## CURRENT BASELINE

The project already has:

- PlayerRuntimeState
- EnemyRuntimeState
- PlayerProgressionState
- Enemy death lifecycle
- Automatic projectile combat
- XP pickups
- XP accumulation
- Level progression
- Temporary upgrade selection
- Generic development projectile weapon
- Generic temporary development upgrades

Do not redo these systems.

Build on the current repository state.

Before changing anything:

1. Inspect the current run, spawning, enemy, XP, and progression systems.
2. Confirm the existing Phase 1.1–1.4 implementation.
3. Identify the smallest reusable seam for stage timing and enemy pressure.
4. Do not perform a broad refactor.

---

# REQUIREMENTS

## 1. Run Timer

Introduce a reusable run/stage timer.

The timer must:

- start when a run begins
- advance while the run is actively playing
- stop when the run ends
- not advance while gameplay is paused for level-up selection
- stop after player death
- expose elapsed run time

Keep the timer independent from Bible-specific content.

If a pure runtime state is appropriate, prefer that over putting all timer logic directly into GameManager.

---

## 2. Wave / Stage Progression

Implement generic timed pressure stages.

IMPORTANT:

A "wave" in this phase means a time-based stage of increasing pressure.

Do NOT require the player to kill all enemies before progressing to the next stage.

Example:

0–2 min  → Stage 1
2–5 min  → Stage 2
5–10 min → Stage 3
10–15 min → Stage 4

The exact values should be configurable.

Use neutral development data.

Do not create Biblical stages.

---

## 3. Enemy Spawn Rate Scaling

Enemy spawning must become progressively more aggressive as the run progresses.

At minimum support:

- base spawn interval/rate
- stage/time-based multiplier
- configurable scaling

For example:

Stage 1:
low spawn pressure

Stage 2:
higher spawn pressure

Stage 3:
higher spawn pressure

Stage 4:
high spawn pressure

Do not focus on final balancing.

Avoid hardcoding individual enemy types into the stage system.

---

## 4. Enemy Population Scaling

Enemy pressure should increase not only through spawn rate.

Implement a generic population constraint or scaling mechanism.

The system should support concepts such as:

- maximum active enemies
- stage-based population multiplier
- configurable population limits

Do not introduce enemy variety, elites, bosses, or wave-specific enemy types.

The same existing generic enemy is sufficient.

---

## 5. Data-Driven Configuration

Scaling should be data-driven where practical.

Prefer reusable configuration/resource data for:

- stage duration
- spawn rate
- population limit
- scaling multipliers
- total stage/run duration

Do not hard-code Biblical stage names or content.

A neutral development stage configuration is sufficient.

Do not build the final content pipeline yet.

---

## 6. Stage Completion

The run must have a deterministic completion condition.

For this phase, use a simple configurable total stage/run duration.

Example:

15-minute development run.

When the timer reaches the configured completion duration:

`Run Completed`

must become a clear runtime state/event.

Do NOT implement:

- Biblical story completion
- boss encounters
- narrative events
- rewards
- shop
- achievements

Only establish the generic completion state.

---

## 7. Integration With Existing Systems

Preserve:

- Player movement
- Enemy pursuit
- Enemy contact damage
- Projectile combat
- Enemy death
- XP pickup
- XP progression
- Level-up pause
- Temporary upgrade selection
- Upgrade application

The new stage-pressure system must coexist with the existing gameplay loop.

During level-up selection:

- gameplay may be paused
- run timer must not incorrectly advance
- enemy spawning must not continue as if real time were passing

After the upgrade is selected:

- gameplay resumes
- timer continues
- enemy pressure continues from the correct stage/time

After player death:

- timer stops
- spawning stops
- no further stage progression occurs

After stage completion:

- spawning stops
- run enters a completed state
- normal combat progression stops

---

# ARCHITECTURE CONSTRAINTS

Prefer small, reusable runtime components.

A reasonable conceptual separation is:

Run / Stage State
    ↓
Run Timer
    ↓
Stage Configuration
    ↓
Spawn Pressure
    ↓
Enemy Manager

Do NOT create a large centralized StageManager if the existing architecture can support a smaller abstraction.

Do not perform a large GameManager refactor.

Do not replace existing spawning unless necessary.

Extract only the minimum seam required for timed scaling.

---

# IMPORTANT SCOPE LIMITS

DO NOT IMPLEMENT:

- Biblical locations
- Biblical stages
- Bible story
- narrative encounters
- dialogue
- bosses
- elites
- mini-bosses
- enemy types
- enemy waves requiring full clear
- shop
- achievements
- meta progression
- persistence
- save system
- final balancing
- final UI
- Bible content

Do not add systems merely because they may be useful later.

---

# TESTING

Add pure .NET unit tests for any new runtime state.

At minimum test:

## Timer

- initial time
- time advancement
- paused state
- stopped state
- deterministic elapsed time

## Stage Progression

- initial stage
- stage transition at exact threshold
- stage transition after threshold
- multiple stage transitions
- final completion threshold
- no progression after completion

## Scaling

- base spawn rate
- stage-based spawn scaling
- population limit scaling
- deterministic configuration

## Run Lifecycle

Test that:

- run starts
- timer advances during active gameplay
- timer does not advance while paused
- timer stops on death
- timer stops on completion
- spawning is disabled after death
- spawning is disabled after completion

Preserve all existing tests.

---

# VERIFICATION

Before declaring Phase 1.5 complete, run:

`dotnet test Tests/BibleSurvivors.Core.Tests.csproj --no-restore`

`dotnet build Survivors Starter Kit.sln --no-restore`

`git diff --check`

Report:

- tests passed/failed
- build result
- warnings
- files changed
- scenes changed
- resources changed
- whether Bible content was added
- whether bosses/elites/encounters were added
- whether any later-phase system was introduced
- limitations caused by the lack of a Godot executable

If Godot is unavailable, inspect scene/resource wiring statically and clearly state that manual in-engine verification could not be performed.

---

# ACCEPTANCE CRITERIA

Phase 1.5 is complete only if the following generic development run can exist:

Start Run
→ Timer starts
→ Enemies spawn
→ Player fights
→ XP / Level Up / Upgrade continue working
→ Time progresses
→ Enemy spawn pressure increases
→ Active enemy population can increase
→ Stage progresses
→ Timer reaches configured completion duration
→ Spawning stops
→ Run enters Completed state

The system must be reusable and independent of Biblical content.

Neutral development configuration is sufficient.

---

# STOP CONDITION

After completing and verifying Phase 1.5:

STOP.

Do not continue into:

- enemy variety
- bosses
- Biblical stages
- narrative
- weapon evolution
- weapon fusion
- meta progression
- shop
- achievements

Report implementation and verification results only.