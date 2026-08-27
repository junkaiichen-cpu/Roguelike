# AGENTS.md

# Faith Fight
# Long-Term AI Development Rules
# Version 2.0

---

# 1. Project Identity

Project Name:
Faith Fight

Repository / Project Folder:
BibleSurvivors

Engine:
Godot 4

Language:
C#

Project Type:
Survivors-like roguelite / action roguelite

Primary Repository:
The current Git repository is the source of truth.

---

# 2. Project Vision

Faith Fight is a long-term Survivors-like roguelite whose gameplay,
characters, world, narrative, encounters, scripture references, and
progression are based on the Bible.

The game must first be a genuinely enjoyable and mechanically solid
game.

Bible content should then become deeply integrated into gameplay,
character progression, narrative progression, unlocks, and long-term
player goals.

Bible content must not merely be decorative text placed on top of an
otherwise unrelated game.

The project is expected to evolve for a long period of time.

Therefore:

DO NOT attempt to build the entire game at once.

Build stable foundations incrementally.

---

# 3. Repository Is the Long-Term Memory

The repository is the ultimate source of truth.

AI chat history is NOT the authoritative source of project state.

Codex MUST NOT depend on previous conversations to understand the
project.

All important project knowledge must eventually be represented in the
repository.

The repository must remain understandable even if:

- the current Codex conversation disappears
- the user changes ChatGPT accounts
- another AI coding agent takes over
- a human developer joins the project
- the project is paused for months

The goal is:

Chat history is temporary.

Repository context is persistent.

---

# 4. Project Context Files

The project uses separate documents for different kinds of knowledge.

## Permanent Development Rules

AGENTS.md

Defines:

- how AI agents should work
- architecture principles
- scope rules
- testing rules
- verification rules
- documentation rules
- phase discipline

AGENTS.md should contain rules, not a detailed history of every
implementation.

---

## Project Vision

PROJECT_VISION.md

Defines:

- long-term product vision
- gameplay philosophy
- Bible integration philosophy
- intended player experience
- long-term goals

This document should change relatively rarely.

---

## Current Project State

PROJECT_STATUS.md

Defines:

- current phase
- completed phases
- current implementation state
- current unfinished work
- known limitations
- current architecture summary
- latest verification results

PROJECT_STATUS.md is the primary persistent development state.

It must describe the ACTUAL repository state.

It must not describe imaginary future architecture.

---

## Development Roadmap

docs/DEVELOPMENT_ROADMAP.md

Defines:

- planned phases
- intended future systems
- development order
- high-level milestones

The roadmap describes what is planned.

It does NOT prove that something has been implemented.

---

## Development History

docs/DEVELOPMENT_LOG.md

Defines:

- historical milestones
- important implementation decisions
- completed phases
- verification history
- significant problems and solutions

PROJECT_STATUS.md describes NOW.

DEVELOPMENT_LOG.md describes HISTORY.

---

## Architecture Documentation

docs/ARCHITECTURE.md

Defines:

- actual architecture
- responsibilities
- important boundaries
- data flow
- runtime state
- Resource usage
- scene ownership
- important architectural decisions

Architecture documentation must describe the actual implementation.

Do not document an architecture merely because it is planned.

---

## Design Documentation

docs/GAME_DESIGN.md

Defines:

- gameplay rules
- progression philosophy
- combat philosophy
- run structure
- balancing principles
- player experience

---

## Narrative Documentation

docs/NARRATIVE_DESIGN.md

Defines:

- story philosophy
- chapter structure
- encounter philosophy
- narrative progression
- scripture integration

---

## Content Pipeline

docs/CONTENT_PIPELINE.md

Defines:

- how game content is authored
- how Resources are used
- how characters, weapons, enemies, encounters, and narrative content
  are represented
- how content moves from definition to runtime

---

## Phase Task Specifications

Significant phase specifications should be stored under:

tasks/

Example:

tasks/phase-1.1.md
tasks/phase-1.2.md
tasks/phase-1.3.md
tasks/phase-1.4.md
tasks/phase-1.5.md
tasks/phase-1.6.md

A task specification defines INTENDED SCOPE.

PROJECT_STATUS.md defines ACTUAL STATE.

These are different concepts.

---

# 5. Required Context Loading

Before beginning any implementation task, Codex MUST:

1. Read AGENTS.md.
2. Read PROJECT_VISION.md.
3. Read PROJECT_STATUS.md.
4. Read docs/DEVELOPMENT_ROADMAP.md.
5. Read relevant architecture documentation.
6. Read relevant design documentation.
7. Read the task specification if one exists.
8. Inspect the actual repository.
9. Inspect relevant source code.
10. Inspect relevant scenes and Resources.

Codex MUST NOT assume that a feature is implemented merely because:

- the user says it was implemented
- chat history says it was implemented
- documentation says it was implemented
- a task specification says it should exist

The repository must be inspected.

---

# 6. Source-of-Truth Priority

When information conflicts, use this priority:

1. Actual source code
2. Actual Godot scenes and Resources
3. PROJECT_STATUS.md
4. docs/ARCHITECTURE.md
5. docs/GAME_DESIGN.md
6. docs/CONTENT_PIPELINE.md
7. docs/DEVELOPMENT_ROADMAP.md
8. task specifications
9. previous chat history

Actual implementation takes precedence over stale documentation.

If documentation conflicts with the repository:

- inspect the repository
- determine the actual state
- update the documentation when appropriate

Do not blindly trust stale documentation.

---

# 7. Phase Discipline

The project is developed in explicit phases.

Codex MUST determine the current phase using:

- PROJECT_STATUS.md
- docs/DEVELOPMENT_ROADMAP.md
- the current task specification

The current task defines the maximum allowed implementation scope.

Codex MUST NOT silently implement future phases.

If the requested task is Phase 1.4:

DO:
- implement Phase 1.4

DO NOT:
- continue into Phase 1.5
- add boss systems
- add weapon evolution
- add Bible characters
- add meta progression

unless explicitly requested.

---

# 8. Phase Status

Every phase should have one of these states:

- NOT STARTED
- IN PROGRESS
- BLOCKED
- COMPLETE
- DEFERRED

Do not mark a phase COMPLETE until its acceptance criteria and
verification requirements have been satisfied.

---

# 9. Starting a Phase

Before implementing a phase, Codex MUST:

1. Read the phase task.
2. Read PROJECT_STATUS.md.
3. Inspect the repository.
4. Identify which requirements already exist.
5. Identify which requirements are missing.
6. Identify relevant existing systems.
7. Avoid reimplementing completed functionality.
8. Determine the smallest implementation seam.

Codex should explicitly reason about:

- what already works
- what must be added
- what must not be changed
- what later systems must remain untouched

---

# 10. End of Phase Persistence

When a phase is completed, Codex MUST update the repository state.

At minimum:

PROJECT_STATUS.md

must be updated with:

- phase name
- status
- implementation summary
- important architecture changes
- files changed
- scenes changed
- Resources changed
- tests
- build result
- warnings
- manual Godot verification status
- known limitations
- intentionally deferred systems

For significant phases:

docs/DEVELOPMENT_LOG.md

should receive a concise historical entry.

If architecture changed significantly:

docs/ARCHITECTURE.md

must be updated.

If gameplay behavior changed:

docs/GAME_DESIGN.md

should be updated where necessary.

If content authoring changed:

docs/CONTENT_PIPELINE.md

should be updated where necessary.

If roadmap status changed:

docs/DEVELOPMENT_ROADMAP.md

should be updated.

---

# 11. Interrupted Work

If implementation is interrupted because of:

- usage limits
- context limits
- tool failure
- build failure
- unavailable Godot executable
- unresolved technical issue
- user interruption

Codex MUST NOT mark the task COMPLETE.

If possible, persist the current state to PROJECT_STATUS.md.

Record:

Status:
IN PROGRESS

Implemented:
...

Remaining:
...

Known Issues:
...

Next Recommended Step:
...

Another AI agent must be able to continue from the repository without
requiring the previous conversation.

---

# 12. Do Not Fake Progress

Codex MUST NOT claim:

- "implemented"
- "complete"
- "verified"
- "working"
- "tested"

unless there is evidence.

If a system was only statically inspected:

Say:

"Statically inspected."

If a build passed:

Say:

"dotnet build passed."

If tests passed:

Say:

"X tests passed."

If Godot could not be launched:

Say:

"Godot manual verification was not available."

Never imply that a manual game test occurred when it did not.

---

# 13. Scope Discipline

Do not implement systems merely because they may be useful later.

Implement the smallest system required by the current task.

Prefer:

small isolated changes

over:

large refactors.

Prefer:

reusable seams

over:

premature frameworks.

Prefer:

incremental architecture

over:

speculative architecture.

---

# 14. No Large Refactors Without Explicit Need

Do not rewrite working systems merely because another architecture
might eventually be cleaner.

Before refactoring:

1. Identify the concrete problem.
2. Determine whether the current task actually requires the refactor.
3. Identify the smallest safe boundary.
4. Preserve existing behavior.
5. Test the affected system.

A future architecture is not sufficient justification for a large
refactor.

---

# 15. Architecture Principle

Use a data-driven architecture.

Core principle:

C# defines HOW the game works.

Data definitions define WHAT exists.

Conceptually:

Game System
    ↓
Runtime Model
    ↓
Data Definition
    ↓
Game Content

Example:

CombatSystem
    ↓
WeaponDefinition
    ↓
FutureWeaponContent

NOT:

CombatSystem
    ↓
if character == David
    ...

Bible-specific content must not be hard-coded throughout generic
gameplay systems.

---

# 16. Separation of Concerns

Keep these concerns separate.

## Core Gameplay

- Player
- Movement
- Combat
- Weapons
- Projectiles
- Enemies
- Spawning
- Experience
- Leveling
- Waves
- Bosses
- Victory
- Defeat
- Restart

## Run State

- current HP
- current XP
- current level
- current temporary upgrades
- current weapons
- run timer
- current enemies
- current run progression

## Meta Progression

- Currency
- Permanent upgrades
- Shop
- Unlocks
- Collections
- Achievements

## Character

- Character definitions
- Base stats
- Starting weapons
- Passive abilities
- Character progression
- Character eras

## Bible Content

- Biblical characters
- Historical eras
- Locations
- Events
- Scripture references
- Scripture content
- Bible-specific gameplay content

## Narrative

- Story arcs
- Chapters
- Encounters
- Narrative state
- Story progression
- Narration
- Quiz

These systems may communicate through explicit boundaries.

Do not turn them into one giant interconnected implementation.

---

# 17. Runtime State Architecture

When deterministic gameplay state can be separated from Godot-specific
Node behavior, prefer a pure runtime state.

Examples:

PlayerRuntimeState
EnemyRuntimeState
PlayerProgressionState
RunRuntimeState

Pure runtime state should preferably:

- contain deterministic state
- contain validation
- contain calculations
- contain transitions
- be independent from Godot Node APIs
- be easy to unit test

Godot-specific classes should handle:

- Nodes
- signals
- scene lifecycle
- physics
- input
- rendering
- resource loading
- scene composition

Do not put Godot Node logic into pure runtime models.

---

# 18. Data Definitions

Prefer Godot Resources for structured game data when appropriate.

Examples:

CharacterDefinition
WeaponDefinition
EnemyDefinition
StageDefinition
UpgradeDefinition
EncounterDefinition
StoryChapterDefinition
ScriptureReference
QuizDefinition
AchievementDefinition

Do not introduce a large JSON infrastructure when Godot Resources
already provide a clean solution.

Use the simplest repository-compatible data architecture.

---

# 19. Survivors Gameplay Philosophy

Faith Fight should preserve the fundamental strengths of the
Survivors-like genre:

- immediate controls
- automatic attacks
- large enemy populations
- satisfying enemy destruction
- XP collection
- frequent level-up choices
- rapidly increasing player power
- build experimentation
- escalating enemy pressure
- elites
- bosses
- meaningful run progression
- permanent progression
- unlocks
- achievements
- replayability

The implementation must be original.

Do NOT copy:

- protected commercial source code
- protected assets
- exact UI
- sound effects
- artwork
- exact numerical designs
- proprietary implementation details

The target is the gameplay category and player experience,
not cloning a specific commercial game.

---

# 20. MVP Philosophy

The first playable milestone should prove the fundamental game loop:

Player
→ Movement
→ Automatic Attack
→ Enemies
→ Damage
→ Enemy Death
→ XP
→ Level Up
→ Stronger Build
→ Escalating Pressure
→ Boss
→ Victory / Defeat
→ Results
→ Restart

If the core game loop is not enjoyable,
adding narrative systems will not solve the underlying problem.

Bible content should be introduced after the core gameplay foundation
is sufficiently stable.

---

# 21. Character Architecture

Characters must be data-driven.

Conceptually:

CharacterDefinition

    id
    display_name
    description
    era
    base_stats
    starting_weapon
    passive
    story_arc_id
    achievement_track_id

Example conceptual progression:

David
    ↓
Young David
    ↓
Warrior David
    ↓
King David

Character eras may become unlockable progression states.

Do not hard-code individual Biblical characters into generic gameplay
systems.

---

# 22. Bible Content Architecture

Bible-specific content belongs in data/content layers.

Generic gameplay systems should not contain:

if character == David

or:

if weapon == GoliathWeapon

or:

if chapter == DavidChapter

Instead:

Generic System
    ↓
Definition / Resource
    ↓
Content Data

Bible-specific behavior should be represented by data or explicitly
defined content systems.

---

# 23. Story Architecture

Stories must remain chronologically coherent.

IMPORTANT:

Gameplay randomness != narrative randomness.

Example:

David Story Arc

Chapter 01
Shepherd

Chapter 02
Anointed

Chapter 03
Goliath

Chapter 04
Saul

Chapter 05
Fugitive

Chapter 06
King

Gameplay may select randomly from the valid content of the current
chapter.

Gameplay MUST NOT randomly jump from an early chapter to an unrelated
late chapter.

Narrative progression must remain chapter-constrained.

---

# 24. Encounter Architecture

Conceptual encounter flow:

Run
→ Run milestone / victory condition
→ Encounter eligibility
→ Encounter selection
→ Narrative
→ Scripture references
→ Story completion
→ Quiz
→ Result
→ Achievement / progression

Encounter content should be data-driven.

An encounter should be capable of representing:

- character
- story arc
- chapter
- prerequisites
- difficulty
- narrative content
- scripture references
- quiz
- rewards
- achievement linkage

Do not implement the full encounter system until its phase is
explicitly requested.

---

# 25. Narrative Philosophy

Narrative content should feel like an actual journey through a
character's life.

Do not reduce Bible characters to random trivia.

The player should experience a chronological progression.

Each completed encounter should contribute to the larger story arc.

---

# 26. Audio Narrative

Narrative systems should eventually support approximately
five-minute audiobook-style storytelling.

The architecture may support:

- narration text
- chapters
- paragraphs
- scripture references
- optional subtitles
- audio asset references
- playback state
- completion state

Do not make a third-party TTS provider a hard dependency of the core
game.

Audio implementation must remain replaceable.

Do not implement this system before its designated phase.

---

# 27. Scripture

Scripture should be represented as structured content.

Prefer references such as:

Book
Chapter
VerseStart
VerseEnd
Translation

Do not scatter scripture text throughout C# source files.

Development placeholders may be used where necessary.

Target translation:

Chinese Union Version / 新标点和合本

Do not assume that translation text may be redistributed without
checking applicable licensing and copyright requirements.

---

# 28. Quiz System

Narrative events may eventually produce quizzes.

Questions should test comprehension of:

- narrative content
- referenced scripture
- story events

Quiz systems should avoid random guessing as the primary mechanic.

Possible rewards include:

- achievement
- story completion
- character progression
- unlock
- currency
- other progression defined by the relevant phase

Do not implement quiz systems before their designated phase.

---

# 29. Achievement System

Achievements must eventually be data-driven.

Possible categories:

- global
- character-specific
- story-specific
- gameplay-specific
- collection-based
- quiz-based

Do not create hard-coded character-specific achievement code paths.

Do not implement achievements before their designated phase.

---

# 30. Meta Progression

Eventually the game may support:

Run Currency
→ Permanent Upgrades
→ Unlocks
→ Shop
→ Characters
→ Weapons
→ Stages
→ Achievements
→ Narrative Content

Meta progression should create meaningful long-term goals.

Avoid meaningless numerical inflation.

Do not implement meta progression before its designated phase.

---

# 31. Save System

Temporary run state must eventually remain separate from persistent
player state.

## Temporary Run State

Examples:

- current HP
- current XP
- current level
- current weapons
- current temporary upgrades
- current enemies
- current run timer
- current stage

## Persistent State

Examples:

- currency
- permanent upgrades
- unlocked characters
- unlocked eras
- unlocked weapons
- achievements
- completed story chapters
- completed encounters
- quiz results
- settings

Do not mix these categories.

Do not implement persistence before its designated phase.

---

# 32. Randomness

Randomness should create replayability.

Randomness must NOT destroy:

- story chronology
- unlock requirements
- player agency
- progression integrity

Important progression should be deterministic once its prerequisites
are satisfied.

When random selection affects narrative content, selection must remain
constrained by the current narrative chapter and prerequisites.

---

# 33. Difficulty

Difficulty may eventually influence:

- enemy strength
- enemy density
- spawn rate
- elite frequency
- boss behavior
- narrative encounter difficulty
- quiz difficulty

Difficulty should not simply multiply every number without considering
player experience.

Do not introduce advanced balancing systems prematurely.

---

# 34. Godot Rules

Follow existing Godot project conventions whenever reasonable.

Before changing scene architecture:

1. Inspect scene ownership.
2. Inspect node hierarchy.
3. Inspect node lifecycle.
4. Inspect signals.
5. Inspect Resource loading.
6. Inspect C# integration.
7. Inspect existing prefab composition.

Do not restructure scenes merely for aesthetics.

Preserve existing working scene composition unless the current task
requires a change.

---

# 35. Existing Starter Kit Compatibility

The project originated from a Godot 4 C# Survivors Starter Kit.

Preserve useful existing foundations when they remain compatible.

Examples may include:

- movement
- enemy pursuit
- combat components
- projectile behavior
- enemy prefabs
- HUD primitives
- Resource architecture
- Jolt configuration
- scene composition

Do not rewrite Starter Kit systems without a concrete reason.

When replacing a legacy system is necessary:

- preserve externally visible behavior where possible
- isolate the change
- test it
- document the architectural reason

---

# 36. GameManager Discipline

GameManager currently acts as a run-level coordinator.

Do not perform a large GameManager refactor merely because it has
multiple responsibilities.

Extract logic only when:

- the current task requires it
- the extraction creates a meaningful boundary
- the new component can be independently tested
- the change reduces concrete complexity

Prefer small scoped seams.

Avoid creating an elaborate framework around GameManager prematurely.

---

# 37. Code Quality

Prefer:

- small classes
- explicit responsibilities
- composition
- data-driven definitions
- pure runtime state
- testable logic
- clear boundaries

Avoid:

- God classes
- giant switch statements
- character-specific if/else chains
- unnecessary global state
- hard-coded content
- unnecessary singleton usage
- speculative frameworks
- premature abstraction

---

# 38. Testing Philosophy

Every new gameplay subsystem should receive the highest-value tests
that can reasonably be written.

Prioritize:

- pure domain logic
- progression calculations
- state transitions
- unlock requirements
- random selection constraints
- encounter prerequisites
- story ordering
- quiz scoring
- achievement conditions
- serialization logic

Not every Godot visual scene requires unit tests.

When a system contains deterministic logic that can be extracted into a
pure C# class, prefer testing that class directly.

---

# 39. Required Verification

Before declaring a task complete:

1. Run relevant automated tests.
2. Build the project.
3. Inspect compiler errors.
4. Inspect warnings.
5. Run `git diff --check`.
6. Inspect affected scenes.
7. Inspect affected Resources.
8. Check for obvious regressions.
9. Perform Godot manual verification when the executable is available.
10. Persist verification results in PROJECT_STATUS.md.

At minimum, when applicable:

```text
dotnet test Tests/BibleSurvivors.Core.Tests.csproj --no-restore

dotnet build Survivors Starter Kit.sln --no-restore

git diff --check

# 40. Project Status Maintenance

PROJECT_STATUS.md is the authoritative snapshot of the current
development state.

Codex MUST read PROJECT_STATUS.md before beginning implementation.

Codex MUST update PROJECT_STATUS.md when:

- a phase starts
- a phase reaches a meaningful milestone
- a phase is completed
- implementation is interrupted
- verification results change
- important architecture changes occur
- known limitations change

PROJECT_STATUS.md must describe the ACTUAL repository state.

It must NOT become:

- a future design document
- a copy of AGENTS.md
- a duplicate of DEVELOPMENT_ROADMAP.md
- a chat transcript
- a speculative architecture proposal

For each active or completed phase, PROJECT_STATUS.md should record:

- phase number
- phase name
- status
- objective
- implemented functionality
- files changed
- scenes changed
- Resources changed
- tests
- build result
- warnings
- manual Godot verification status
- known limitations
- intentionally deferred systems
- next recommended step

Codex MUST NOT mark work as COMPLETE merely because the code exists.

A phase is COMPLETE only when its acceptance criteria and required
verification have been satisfied.

If a phase is partially implemented:

Status:

IN PROGRESS

The document should clearly distinguish:

Implemented

from:

Remaining

and:

Not Started.

# 41. Context Recovery

A new AI coding agent must be able to continue development
without access to previous chat history.

When entering an existing project, the agent should reconstruct
context from the repository.

Preferred context loading order:

1. AGENTS.md
2. PROJECT_VISION.md
3. PROJECT_STATUS.md
4. docs/DEVELOPMENT_ROADMAP.md
5. docs/ARCHITECTURE.md
6. relevant design documentation
7. relevant task specification
8. actual source code
9. actual scenes and Resources

Chat history is supplementary and must never be treated as the
authoritative project state.

If PROJECT_STATUS.md conflicts with the actual repository:

1. inspect the repository
2. determine the actual implementation state
3. correct PROJECT_STATUS.md
4. continue only after the state is understood

Do not blindly trust PROJECT_STATUS.md.

The repository is the final source of truth.


# 42. Phase Completion Protocol

When a requested phase reaches completion, Codex MUST perform the
following sequence:

1. Verify the acceptance criteria.

2. Run the required automated tests.

3. Build the project.

4. Run git diff --check.

5. Inspect affected scenes and Resources.

6. Perform manual Godot verification when possible.

7. Record exact verification results.

8. Update PROJECT_STATUS.md.

9. Update docs/DEVELOPMENT_LOG.md for significant milestones.

10. Update docs/DEVELOPMENT_ROADMAP.md if phase status changed.

11. Update docs/ARCHITECTURE.md if architecture changed.

12. Update docs/GAME_DESIGN.md if gameplay behavior changed.

13. Report:
    - implementation
    - tests
    - build
    - warnings
    - files changed
    - scenes changed
    - Resources changed
    - manual verification
    - known limitations
    - deferred systems

14. STOP.

Codex MUST NOT automatically begin the next phase.

The user must explicitly request the next phase.


# 43. Interruption Recovery Protocol

If a task cannot be completed in the current session, Codex MUST
preserve the current implementation state when possible.

Before stopping, update PROJECT_STATUS.md with:

Status:
IN PROGRESS

Implemented:
- ...

Remaining:
- ...

Known Issues:
- ...

Verification:
- ...

Next Recommended Step:
- ...

Do NOT mark the task COMPLETE.

Do NOT claim that acceptance criteria were satisfied.

A future agent should be able to continue the task by reading the
repository without requiring the previous conversation.


# 44. State Consistency

The following project state must remain consistent:

PROJECT_STATUS.md
docs/DEVELOPMENT_ROADMAP.md
docs/DEVELOPMENT_LOG.md
docs/ARCHITECTURE.md
actual source code
actual Godot scenes
actual Resources

These documents serve different purposes, but they must not
contradict the actual implementation.

When a contradiction is discovered:

1. inspect the implementation
2. determine the actual state
3. preserve historical information where appropriate
4. update the appropriate documentation
5. continue development only after resolving the contradiction

Never modify source code simply to make documentation appear correct.

Never modify documentation to hide incomplete implementation.