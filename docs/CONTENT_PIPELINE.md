# Content Pipeline

## Purpose

Bible Survivors will scale through authored data rather than character-specific
C# changes. This document defines the planned content ownership, review rules,
and migration path from the Starter Kit's existing Resource usage. Phases 1.3
through 1.5 add bounded development weapon, temporary-upgrade, and
stage-pressure and completion-boss resources; they do not create a general
content database.

## Current content baseline

The repository already uses Godot Resources for starter upgrade data:

| Location | Current purpose |
| --- | --- |
| Powerups/*.tres | player upgrade descriptions and limits |
| Powerups/Enemy/*.tres | enemy upgrade descriptions, limits, and optional numeric values |
| Weapons/development_projectile_weapon.tres | generic development projectile's base damage, cadence, speed, and bullet-scene reference |
| Upgrades/*.tres | neutral temporary upgrade definitions and the small development choice catalog |
| Stages/*.tres | neutral development timed-pressure definitions and their run configuration |
| Bosses/development_boss.tres | neutral completion-boss id, label, and existing enemy-scene reference |
| Prefabs/Powerups | starter attack and projectile scenes |
| Prefabs/Progression/experience_pickup.tscn | generic collectable XP pickup scene |
| Prefabs/Enemies | five starter enemy scenes |
| Prefabs/UI | upgrade-card and life-bar UI scenes |
| Objects and addons | world meshes, materials, textures, and third-party content |

PowerupPaths keeps the existing upgrade resource paths in C#, and upgrade
behavior is driven by enums and switches. ProjectileWeaponDefinition is the
first direct combat-definition seam: its data is consumed by generic Shooting
behavior without a hard-coded projectile path or baseline values. This is a
workable incremental migration, but it does not yet meet the long-term
requirement for all content additions without generic code modification. The
sampled starter descriptions are French and one EnemySpawnRate resource
describes an explosion chance although its runtime handler changes spawn rate.
Treat current authored labels as prototype content that requires semantic
review before reuse.

Phase 1.4 adds a separate temporary-upgrade family rather than repurposing the
mixed player/enemy starter choice resources. TemporaryUpgradeDefinition
contains a stable development id, display text, generic effect, amount, and
per-run application limit. TemporaryUpgradeCatalog owns the choice pool.
These are neutral development data, not character, Bible, or meta-progression
content.

Phase 1.5 adds StagePressureDefinition and StagePressureConfiguration. Each
stage defines a duration, spawn-rate multiplier, and active-population
multiplier; the configuration owns base spawn/population values, a player-safe
enemy spawn-distance range, a bounded placement-attempt count, and stage order.
The development stages are intentionally named only by sequence and carry no
Biblical location, enemy, or narrative reference. Phase 1.6 adds a single
CompletionBoss reference to the run configuration. Its BossDefinition supplies
a neutral id, display label, and existing Enemy scene reference; its health and
combat remain authored by that generic scene.

## Canonical future content shape

Godot Resource definitions are the default representation for authored game
content. A definition describes what exists; C# systems define how it runs.
The following resource families should be introduced only in their roadmap
phases:

| Family | Definition examples | Consumer |
| --- | --- | --- |
| Core gameplay | weapon, enemy, stage, wave, upgrade | combat, spawning, run systems |
| Character | character, character era, passive, starting loadout | character selection and run setup |
| Bible content | biblical person, location, event, scripture reference | character and narrative content |
| Narrative | story arc, story chapter, encounter, narration | narrative progression and presentation |
| Meta progression | achievement, unlock requirement, permanent upgrade | profile progression |
| Quiz | quiz and question definitions | quiz presentation and scoring |

Definition resources may reference Godot scenes, assets, or other definitions
when the relationship is part of content. Runtime nodes and services must not
be serialized into content definitions.

## Identifier conventions

Every future definition must have a stable, unique identifier. Use lowercase
snake_case names that describe the domain rather than the display text. For
example, a character id can be david and a chapter id can be
david_shepherd_01. The exact example ids are illustrative only and do not
create Bible content.

Rules:

1. Ids are immutable after content is shipped because saves and cross-content
   references depend on them.
2. Display name and localized text are separate from ids.
3. Relationships use ids or typed resource references, never a C# character
   name check.
4. Versioned definitions need an explicit migration plan if a saved semantic
   meaning changes.
5. Content filenames should match their primary identifier where practical.

## Planned repository layout

When the relevant systems exist, content should be grouped by domain under a
single content root. A possible structure is:

    Content/
    ├── Core/
    │   ├── Weapons/
    │   ├── Enemies/
    │   └── Stages/
    ├── Characters/
    ├── Bible/
    │   ├── References/
    │   └── Locations/
    ├── Narrative/
    │   ├── StoryArcs/
    │   └── Encounters/
    ├── Progression/
    └── Quizzes/

This is a destination, not a directory to create during Phase 0. Existing
Powerups and Prefabs remain where they are until a scoped migration needs
them. Avoid a broad move that makes starter-kit behavior hard to verify.

## Authoring workflow

For every new content family:

1. Introduce and test the smallest shared Resource schema and its pure
   validation logic.
2. Create a small development-safe sample set rather than a full content
   library.
3. Assign stable ids and add cross-reference validation.
4. Verify the resource loads in Godot and the consuming runtime can handle
   valid and missing optional references.
5. Review the diff for accidental generated imports, external asset changes,
   or source/licensing issues.

Content creation must not require edits to generic gameplay code merely to
recognize a specific Biblical person, era, story, or event.

## Validation expectations

The future content pipeline should validate the highest-value integrity rules:

| Family | Minimum validation |
| --- | --- |
| Character | unique id, valid starting loadout, valid passive and era links |
| Weapon / enemy / stage | unique id, required values in range, valid referenced scenes |
| Temporary upgrade | unique id, supported neutral effect, positive amount, positive application limit |
| Stage pressure | positive duration and multipliers; nonempty ordered stage list; positive base spawn interval/population cap; finite minimum/maximum spawn distance and positive bounded placement attempts |
| Story | unique ids, strictly ordered chapter sequence, valid prerequisites |
| Encounter | valid character/story/chapter reference; no eligibility outside its chapter |
| Scripture reference | valid nonzero chapter and verse range; supported translation id |
| Quiz | question count and valid answer options; valid encounter link |
| Achievement / unlock | valid referenced ids and non-cyclic prerequisite logic |
| Save migration | old ids remain resolvable or map explicitly to replacements |

Validation should be pure C# where possible so it can have automated tests
without requiring a running Godot scene.

## Scripture and source policy

The target translation is Chinese New Punctuation Union Version / 新标点和合本.
Before storing or shipping complete scripture text, verify the applicable
copyright and distribution license. Until then, content may store references,
author-written summaries, and explicitly approved development placeholders.

Every imported external asset or source text needs traceable provenance and its
license must be compatible with project distribution. The Starter Kit's
existing third-party addons and art remain untouched in this phase.

## Migration from the starter resources

The existing Powerup resources are retained as working prototype content.
Phase 1.3 introduces the smallest direct weapon definition:
ProjectileWeaponDefinition and one development projectile resource. Its
runtime remains compatible with the existing IUpgradable pathway, so this is
not a conversion of the Powerup system or a general weapon registry. Later
core and weapons work may migrate one additional content family at a time.
Do not convert the starter resources en masse before a consumer needs the
resulting schema. That approach preserves the runnable baseline while keeping
the eventual pipeline data-driven.

Phase 1.4 follows the same staged approach for temporary run upgrades. The
development catalog is a small authored set consumed through an explicit
receiver interface; it does not migrate existing Powerups or establish a
permanent upgrade registry.

Phase 1.5 adds one development run-pressure configuration and four referenced
stage definitions. Phase 1.6 adds one completion-boss definition plus neutral
spawn-safety values to that configuration. This is not a boss registry, stage
presentation system, or content pipeline for locations or narrative
progression.

## Current non-goals

There is no content registry, localization system, Bible database, or sample
Biblical definition. The development projectile is intentionally neutral and
does not establish final weapon content. This document remains a contract for
future scoped implementation work.
