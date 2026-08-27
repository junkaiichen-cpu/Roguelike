# Development Roadmap

## Roadmap rules

This roadmap defines sequencing, not blanket authorization to build every
phase. Each phase requires its own scoped task, repository inspection,
implementation plan, verification, and stop point. Later phases may be
reordered only with an explicit decision recorded in the relevant task.

The current repository starts from a Survivors Starter Kit prototype. The
project must first prove an enjoyable and stable action loop before Biblical
narrative, quizzes, or character-era progression are implemented.

## Current status

**Phase 1.6 stabilization in progress — first playable Survivors loop**

Phase 0 is complete. Phase 1.1 establishes the minimum player runtime:
movement remains in the existing Player scene, temporary health/death state is
pure and testable, and player death stops active gameplay. Phase 1 as a whole
is not complete. Phase 1.2 adds generic enemy health, pursuit, lethal death,
and timer-gated contact damage; enemy death now grants XP only through an
actual death event. Phase 1.3 connects the Player's definition-backed generic
development projectile to enemy damage and death without manual attack input.
Phase 1.4 adds enemy-configured XP pickups, pure temporary player progression,
overflow and multiple-level handling, and a paused neutral temporary-upgrade
selection that applies to the projectile runtime. Phase 1.5 adds a pure
run-pressure timer/state and data-driven timed spawn/population scaling. Phase
1.6 adds a configuration-backed neutral completion boss, terminal victory and
defeat states, a minimal result panel, and a scene-reload restart path. Its
stabilization work also adds bounded missed-projectile cleanup, player-safe
configuration-backed enemy spawn distances, reusable hit feedback, and reload
subscription cleanup. An end-to-end Godot playtest remains required before
treating the milestone as player-validated; at present it is blocked by invalid
third-party FBX import metadata in the working tree.

## Phases

| Phase | Objective | Completion evidence | Explicitly deferred |
| --- | --- | --- | --- |
| 0 | Audit the Starter Kit and establish documentation, boundaries, content rules, and staged plan | architecture, game design, narrative design, content pipeline, and roadmap documents match the observed repository | all runtime and content implementation |
| 1 | Establish the small, reliable core Survivors run loop | movement, one automatic attack, basic enemy damage/death, XP, level choices, escalation, boss, victory/defeat, restart; relevant tests/build | evolutions, meta, character and Bible systems |
| 2 | Build generic weapons, upgrades, and evolution/fusion foundations | data-driven weapon/upgrade behavior and tested combination rules | stages, permanent systems, narrative |
| 3 | Add stages, waves, elites, bosses, and run completion structure | deterministic stage/wave flow, boss milestone, verified victory/defeat behavior | permanent progression and Bible narrative |
| 4 | Add meta progression and persistence foundations | separate profile and run state; saved currency/unlocks and migration tests | shop, achievements, characters, story |
| 5 | Add shop and achievement systems | data-driven rewards, purchases, achievement conditions, and persistence verification | character-specific progression and narrative |
| 6 | Add data-driven character selection and runtime setup | character definitions select neutral stats/loadout/passives without core character branches | Bible history, eras, story progression |
| 7 | Add Bible content models | structured identifiers for persons, locations, events, eras, and scripture references; no unlicensed scripture database | story presentation and encounters |
| 8 | Add linear story arc progression | ordered character-scoped chapters and persistent prerequisite rules with tests | random cross-chapter events, quizzes, audio |
| 9 | Add chapter-constrained encounter flow | eligible encounter selection inside the active chapter, presentation state, and persisted outcomes | scripture text import, full quiz and audio systems |
| 10 | Add scripture-reference presentation | structured reference display and verified content/source policy | unapproved full translation text |
| 11 | Add comprehension quiz system | five-question content definitions, scoring, persistence, and reward linkage tests | character-era progression unless separately scoped |
| 12 | Add character mastery and eras | data-driven era unlocks linked to achievements/story state and run setup | audio narration implementation |
| 13 | Add optional audio narrative | replaceable narration asset integration, subtitles, playback/completion state | provider-specific hard dependency |
| 14 | Expand content and polish | audited additions of characters, stories, encounters, balancing, performance, accessibility, and quality assurance | unreviewed feature expansion |

## Post-Phase 1.6 scope

The existing generic development projectile, XP pickup, temporary upgrade
catalog, timed-pressure configuration, and completion boss are development
baselines. A future task must define the next scoped objective before changing
them; it must not infer Bible content, weapon evolution, meta progression, or
other later systems from their presence.

It must not begin work from later roadmap phases without an explicit task.

## Cross-phase acceptance standards

Every phase must:

1. preserve unrelated working functionality;
2. keep C# behavior separate from authored content;
3. avoid character-specific paths in generic systems;
4. keep narrative progression chronological where narrative is in scope;
5. add the highest-value automated tests practical for pure logic;
6. build/compile the project and inspect errors and warnings;
7. document architecture changes that actually occurred;
8. report limitations and stop at the approved scope.

## Milestone gates

| Gate | Required condition before proceeding |
| --- | --- |
| Core gameplay gate | a complete restartable run is fun, stable, and independently verified |
| Progression gate | temporary run state and persistent profile state are separated and testable |
| Character gate | a new character can be configured without changing generic combat code |
| Narrative gate | story chapters and encounters cannot violate chronological prerequisites |
| Scripture gate | reference/content source policy is reviewed before translation text distribution |
| Expansion gate | content authoring validation and performance checks support safe growth |

## Phase 0 record

Phase 0 documents the observed dependency risks: GameManager owns several
unrelated run responsibilities, scene-tree paths are tightly coupled, and
starter content still uses hard-coded path and enum registries. The prescribed
migration is incremental: preserve the runnable Starter Kit, extract only
scoped seams when a later phase needs them, and do not perform an upfront
rewrite.
