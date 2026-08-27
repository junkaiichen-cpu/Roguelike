# Game Design

## Product statement

Bible Survivors is an original Survivors-like action roguelite. Its first
responsibility is to be satisfying as an action game. Biblical characters,
history, and learning are long-term layers that give the game its identity and
continuity; they do not replace the need for responsive movement, meaningful
build choices, escalating danger, and repeatable runs.

The project draws on the genre's design strengths but must not copy protected
assets, names, numerical designs, user interface, sound, or code from another
game.

## Design pillars

1. Fun first: movement, automatic combat, enemy destruction, XP collection,
   level-up choice, and power growth must stand on their own.
2. Meaningful builds: choices should create recognizable play styles and
   tradeoffs rather than merely raise a number.
3. Escalating survival: enemy pressure, elites, and bosses should create
   readable tension and a satisfying run arc.
4. Biblical identity through systems: characters, locations, events,
   progression, and encounters should eventually originate from content data.
5. Coherent learning: narrative is chronological per character and reinforces
   the story through contextual scripture references and comprehension.
6. Long-term mastery: permanent progress and character eras are later goals,
   not substitutes for an enjoyable first run.

## Intended gameplay loops

### Moment-to-moment loop

    Move → automatically attack → position and dodge → defeat enemies
    → collect XP → choose an upgrade → repeat

### Run loop

    Start run → survive escalating waves → form a build → face elites/boss
    → victory or defeat → resolve run rewards

### Long-term loop

    Play runs → earn progression → unlock options → master characters
    → continue their ordered stories

### Narrative loop

    Run milestone → eligible encounter → story → scripture reference
    → optional quiz → achievement/progression

The narrative loop is deliberately absent from the first playable MVP. It
begins only after the gameplay foundation and supporting character/content
models are in place.

## Current playable baseline

The repository currently provides a Starter Kit prototype, not the Bible
Survivors MVP. It includes:

| Area | Observed behavior |
| --- | --- |
| Movement | WASD/arrow-key movement of a CharacterBody3D player |
| Player runtime | exported health and XP-threshold settings; pure temporary health and progression state; deterministic damage, death, XP overflow, and level transitions |
| Attacks | a generic development projectile weapon automatically targets the nearest living enemy and fires on a timer; its base values are in a Resource. Three additional Starter Kit attack components remain present but are not Phase 1.3 weapon content. |
| Enemies | generic enemy runtime with pure health/pursuit state; existing minion, warrior, archer, mage, and boss scene variants remain configuration-only prototypes; minions begin in the spawn pool |
| Combat | the development projectile applies generic damage on contact and expires after a bounded miss lifetime; living enemies pursue the player and apply timer-gated range contact damage; lethal damage stops them before removal. A short neutral flash makes successful non-lethal hits observable. |
| XP / level | an actual enemy death drops its configured XP pickup; player collection updates temporary XP/level state, pauses play, and offers neutral development projectile upgrades |
| Timed pressure | a neutral four-stage, fifteen-minute development configuration increases spawn cadence and maximum active enemies over elapsed active run time; normal enemies appear in a configured player-safe 30–36 unit band with bounded attempts; completion produces the configured neutral boss encounter |
| Run conclusion | player death produces Defeat; the configured boss's death produces Victory; either shows minimal level/time results and offers restart |
| UI | game timer, player life and XP bars, upgrade choice cards, floating damage labels, and a minimal results panel |
| World | a 3D dungeon GridMap, lighting, camera, and third-party art assets |

The current prototype does not yet establish a complete run because it has no
elite system, currency, permanent progression, character selection, or Biblical
content. The Phase 1.6 result panel intentionally provides only completion
evidence and restart; it has no rewards, narrative consequence, or final UI
polish.

## MVP definition

The first Bible Survivors playable milestone is intentionally limited to the
following core loop:

    player → movement → basic automatic weapon → basic enemy → damage/death
    → XP → level-up choice → stronger build → escalating waves → boss
    → victory or defeat → restart

The MVP excludes shops, achievements, character eras, story, encounters,
scripture text, quizzes, and audio. It must be tested for feel and reliability
before narrative layers receive implementation effort.

## Design constraints for future gameplay

### Player agency

Level-up options must present understandable player-facing choices. Randomness
may vary the offered build options, but it should not silently undermine the
player's chosen character or narrative progress. Difficulty must be designed as
pressure the player can read and respond to, rather than a hidden arbitrary
penalty.

### Difficulty and pacing

Difficulty will eventually consider enemy density, enemy durability, damage,
movement, elite frequency, boss behavior, and encounter/quiz challenge. A
single multiplier applied to every system is not an acceptable difficulty
model. Exact curves, timings, and numbers are deferred until the core loop is
tested.

### Weapons and upgrades

Weapon behavior belongs in generic runtime systems. Weapon definitions provide
the values and references that determine what a weapon does. A character
selects a starting loadout through data; the combat code must not contain
character-name branches. Evolution or fusion is a separate Phase 2 concern and
will not be inferred from the current starter attacks. Phase 1.3 establishes
only a development ProjectileWeaponDefinition and its automatic projectile
runtime; it makes no final weapon-content or balance decision.

### XP and temporary run upgrades

XP belongs to an enemy's configured reward value and is collected through a
generic pickup. The player owns temporary XP, level, and threshold state;
overflow carries forward and multiple thresholds resolve deterministically.
Level-ups pause normal gameplay while the player selects an authored neutral
development upgrade. That upgrade changes active run behavior only and is not
permanent progression, a passive-item system, or final balancing.

### Timed stage pressure

A phase-1 stage is a timed pressure segment, not a kill-all wave or a
narrative location. Its duration, spawn-rate multiplier, and population
multiplier are neutral data. The active run timer pauses for upgrade selection
and stops after player death or the final timed-pressure milestone. That
milestone deterministically spawns the configuration's neutral development
boss; its terminal death produces Victory, while player death produces Defeat.
Results show only outcome, level, elapsed time, and Restart. Rewards, story
consequences, advanced boss behavior, and final balancing remain out of scope.

Normal-enemy placement must create readable pressure rather than unavoidable
contact damage. The current development configuration guarantees only a finite
distance band around the player with a bounded fallback; it does not claim
camera-frustum, obstacle, or navigation-aware placement. Those rules require a
separately scoped stage-world contract.

### Enemies, stages, and bosses

Enemy and stage content should be data-driven as the corresponding systems are
introduced. A boss should be a deliberate run milestone, not merely an enemy
scene with higher values. Stage identity must ultimately support Biblical
settings without embedding those settings in generic spawning logic.

### Run state versus profile state

Temporary state includes health, XP, level, selected temporary upgrades,
weapons, elapsed run time, enemy instances, and wave progress. Permanent state
will include currency, unlocked content, achievements, story completion, quiz
results, and settings. These must not share one mutable state object.

## Character and Biblical design principles

Biblical characters are future data-driven play identities. A character era
may change stats, visual presentation, passive ability, starting weapon, and
available story chapters, but those differences must be defined in character
data. No generic attack or enemy system should know whether it is operating
for a specific Biblical character.

Scripture is represented structurally and responsibly. The intended target
translation is Chinese New Punctuation Union Version / 新标点和合本, but no
translation text will be bundled until distribution rights are verified.

## Phase 0 decisions

This document establishes gameplay intent and records the starter-kit
baseline. It makes no balance decisions, does not rename existing mechanics,
and does not approve any new weapon, enemy, character, stage, visual style, or
narrative content.
