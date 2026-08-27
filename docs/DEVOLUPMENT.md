# Faith Fight Development Log

This document records significant historical development milestones.

It is a historical record, not the current project state.

Current implementation state must be read from:

PROJECT_STATUS.md

The repository itself remains the final source of truth.

---

# Phase 0 — Documentation Foundation

Status:

COMPLETE

Summary:

Established the initial project architecture and long-term development
documentation.

Established:

- architecture documentation
- game design documentation
- narrative design documentation
- content pipeline documentation
- development roadmap

Important Decision:

The project would first establish a reusable Survivors-like gameplay
foundation before introducing Bible-specific content.

Verification:

- documentation validation passed
- dotnet restore passed
- dotnet build passed
- no gameplay systems were changed

---

# Phase 1.1 — Player Health and Death

Status:

COMPLETE

Summary:

Added deterministic player health and terminal death behavior.

Implemented:

- PlayerRuntimeState
- max health
- damage clamping
- healing clamping
- terminal death state
- player death event
- gameplay halt after player death

Important Decision:

Player health and death rules were separated into a pure runtime state
independent of Godot Node APIs.

Verification:

- automated state tests passed
- project build passed

Known Limitations:

- no defeat UI
- no restart flow
- no stage boundary system

---

# Phase 1.2 — Enemy Runtime and Death

Status:

COMPLETE

Summary:

Added reusable enemy runtime health and terminal death behavior.

Implemented:

- EnemyRuntimeState
- deterministic enemy damage
- terminal enemy death
- death event
- protection against processing already-dead enemies
- XP reward tied to actual death rather than tree removal

Important Decision:

Enemy removal from the scene tree must not itself be considered an
enemy kill.

Verification:

- automated tests passed
- project build passed

Known Limitations:

- no enemy waves
- no bosses
- no elite enemies
- no advanced scaling

---

# Phase 1.3 — Automatic Projectile Combat

Status:

COMPLETE

Summary:

Added the generic automatic projectile combat loop.

Implemented:

- ProjectileWeaponDefinition
- configurable damage
- configurable fire rate
- configurable projectile speed
- projectile scene reference
- nearest living enemy targeting
- projectile spawning
- projectile damage
- automatic firing
- firing stops after player death

Important Decision:

Projectile weapon behavior is driven by reusable weapon data rather
than Bible-specific content.

Verification:

- 18 automated tests passed
- project build passed
- git diff --check passed

Known Limitations:

- no weapon fusion
- no weapon evolution
- no Bible weapon content
- no passive item system

---

# Phase 1.4 — XP and Level Progression

Status:

COMPLETE

Summary:

Completed the basic XP and temporary progression loop.

Implemented:

- PlayerProgressionState
- XP accumulation
- configurable level thresholds
- XP overflow
- multiple level progression
- XP pickup
- XP collection
- level-up event
- temporary upgrade selection
- neutral development upgrades
- gameplay pause during upgrade selection
- gameplay resume after selection

Important Decision:

XP and level progression rules are implemented in a pure runtime state
where practical.

Upgrade selection remains generic and is not coupled to Bible content.

Verification:

- 26 automated tests passed
- project build passed
- git diff --check passed

Known Limitations:

- no final upgrade balancing
- no weapon fusion
- no weapon evolution
- no permanent progression

---

# Phase 1.5 — Escalating Enemy Waves

Status:

IN PROGRESS

Summary:

Implementing the first reusable run-pressure system.

Planned / Implemented:

- run timer
- escalating enemy pressure
- stage progression
- spawn-rate scaling
- population scaling
- stage completion condition

Important Decision:

Enemy pressure should be represented through neutral stage data rather
than hard-coded Biblical stages.

Current State:

IN PROGRESS

Remaining:

- final verification
- acceptance-criteria verification
- documentation synchronization
- manual Godot verification if executable becomes available

Known Limitations:

- no Biblical stages
- no story
- no encounters
- no bosses
- no shop
- no achievements

---

# Future Phase History

Future phases should be appended here only after meaningful
implementation milestones occur.

Do not use this document to describe planned work that has not yet
been implemented.

Planned future phases should remain in:

docs/DEVELOPMENT_ROADMAP.md