# Architecture

## Purpose and current status

This document records the architecture that exists in the repository after
Phase 0 and Phases 1.1 through 1.6. It defines the boundaries that later phases must
preserve. It is a planning and migration guide, not a claim that the target
architecture has already been implemented.

The current project is a Godot 4 C# Survivors Starter Kit. It is a 3D project
whose main scene is Scenes/main_scene.tscn. Its current project display name
and .NET assembly name still use Survivors Starter Kit. Renaming or upgrading
those identifiers is outside Phase 0.

## Observed repository architecture

### Runtime composition

The following composition is present today:

    Godot scene tree
    ├── GameManager autoload
    │   ├── EnemyManager (plain C# helper)
    │   ├── RunPressureState for timer, stage progression, and spawn pressure
    │   ├── RunResultState for terminal victory/defeat protection
    │   ├── XP-pickup spawning, level-up choice coordination, enemy spawning,
    │   │   completion-boss coordination, and scene-reload restart
    │   └── development temporary-upgrade, stage-pressure, and boss resources
    └── MainScene
        ├── Player prefab
        │   ├── Player runtime wrapper, PlayerRuntimeState, and PlayerProgressionState
        │   ├── Shooting (definition-backed generic projectile runtime)
        │   ├── SpiritWater
        │   ├── Lifesteal
        │   └── FloatingSphere
        ├── HUD
        │   ├── game timer and player bars
        │   ├── upgrade-choice and run-results views
        │   └── damage-label manager
        ├── GridMap/world environment
        ├── spawned Enemy instances
        └── spawned ExperiencePickup instances

GameManager is configured as an autoload in project.godot. It coordinates the
current run timer, stage-pressure state, XP-pickup spawning, level-up choice,
and spawning cadence. It also queries fixed nodes below MainScene by absolute
paths.

EnemyManager is not a scene node. GameManager constructs it directly and it
uses a dictionary from EnemyClass enum values to five packed scenes:
minion, warrior, archer, mage, and boss. Enemy instances are RigidBody3D nodes
that pursue and damage the player. Player is a CharacterBody3D node that
handles movement, health, animation, and its child attack nodes.

The player prefab currently instantiates all four starter attacks. The attacks
are Node3D components that implement IUpgradable and use their own timers.
Shooting now consumes a ProjectileWeaponDefinition Resource for its baseline
values and projectile scene. The other starter attacks remain scene-centric.
Current content is therefore in a staged transition from scene-centric attacks
and enemy variants toward neutral definition resources.

### Existing data and presentation

Powerup, EnemyPowerup, and StatEnemyPowerup are Godot Resource classes.
Individual .tres files under Powerups describe the starter player and enemy
upgrades. PowerupPaths is a static C# list of those resource paths; the older
starter code retains them for compatibility, though Phase 1.4 no longer
presents them in the active level-up flow.

Phase 1.4 adds TemporaryUpgradeDefinition and TemporaryUpgradeCatalog Godot
Resources. The development catalog contains three neutral projectile upgrades.
It is a bounded transition seam: GameManager loads this one catalog by path,
but each upgrade's display text, effect, amount, and application limit are
data, not character-specific C#.

Phase 1.5 adds StagePressureDefinition and StagePressureConfiguration Godot
Resources. Their neutral development data defines a base spawn interval, base
active-enemy cap, an ordered sequence of duration, spawn-rate multiplier, and
population multiplier values, plus a player-safe enemy spawn-distance band and
bounded attempt count. The four development stages total fifteen minutes; their
timing, pressure, and safe spawn values are configuration, not enemy-specific
code. Phase 1.6 adds its typed CompletionBoss reference, which points to a
neutral BossDefinition Resource containing an id, display label, and generic
Enemy scene reference.

UpgradeView renders the available development upgrade choices by instantiating
the existing UI packed scene. DamageLabelManager listens to GameManager.OnEnemyHit
and uses a small label pool; it unsubscribes when the HUD exits the scene tree,
which prevents a scene reload from retaining a freed HUD instance through the
autoload. The HUD and several gameplay classes depend on fixed scene-tree
paths; for example, GameManager expects MainScene/HUD and Player expects HUD
below the current scene.

There are no persistence, character, Bible-content, narrative, encounter,
quiz, or achievement systems in the current repository. The Tests project
contains focused pure-logic coverage for player and enemy state/pursuit,
automatic-weapon timing, and player progression.
The same test project now also covers pure timer, stage, run-lifecycle, and
spawn-pressure behavior.

### Player runtime

Phase 1.1 introduces PlayerRuntimeState as a pure C# model for maximum health,
current health, damage clamping, healing clamping, and terminal death state.
Player remains the Godot CharacterBody3D runtime wrapper: it reads input,
performs the existing movement and animation work, updates the current health
bar, and publishes a Died event exactly once when the state becomes dead.

GameManager subscribes when its Player reference is set. On death it marks the
run over and pauses the scene tree; its Always-mode process loop also exits
early while the run is over. This prevents movement, timers, enemies, spawning,
XP flow, and upgrade continuation from resuming normal gameplay after death.

Maximum health remains an exported property on the Player prefab because that
is the existing composition point for player configuration. It is intentionally
not a character definition or a persistent profile value. No movement bounds
are imposed by Player: the current scene supplies a finite ground collider but
does not define a stage-boundary contract, which belongs to the stage phase.

### Player progression runtime

Phase 1.4 adds PlayerProgressionState, a pure C# model for current run XP,
current level, the next-level XP threshold, overflow, and deterministic
multiple-level handling. Player owns this temporary state and exposes
ExperienceChanged and LeveledUp events; consumers react through those events
instead of directly mutating XP. The first threshold and its linear increase
per level are exported Player configuration values, not GameManager constants.

Enemy ExperienceReward is exported enemy-prefab configuration. EnemyManager
observes only the terminal Died event and creates one ExperiencePickup at that
enemy's final position. The Area3D pickup detects only the Player collision
layer and calls Player.CollectExperience once before freeing itself. Removing
an enemy without Died grants nothing.

### Enemy runtime

Phase 1.2 separates an enemy's configured maximum health from its temporary
EnemyRuntimeState. EnemyPursuit is pure C# and calculates the velocity toward
a target; the Enemy RigidBody3D converts that result into Godot movement,
retains the existing timer-gated range contact damage, and publishes Died once
when lethal damage is received.

EnemyManager applies the existing spawn-time modifiers before the enemy enters
the scene tree, then initializes the runtime state in Enemy._Ready. It now
listens for Died rather than TreeExiting, so only an actual combat death removes
the enemy from the active list and creates its configured XP pickup. Death
immediately stops enemy processing, physics movement, and its contact-damage
timer before queueing the node for deletion.

The same manager now consumes `EnemySpawnConfiguration`, a pure C# value model
created from the development stage-pressure Resource. It samples a position in
the configured 30–36 unit band around the player's current global position,
accepts at most eight samples, and uses a deterministic maximum-distance
fallback. This protects the player from direct spawn overlap without coupling
spawn placement to camera or viewport dimensions. Obstacle, navigation,
frustum, and inter-enemy separation checks remain future stage-world work.

Enemy damage starts one reusable, one-shot particle node already present in the
enemy scene and toggles one reusable short-lived `HitFlash` child. The flash is
created at most once for a living enemy, so repeated hits do not create
unbounded hit-effect nodes or timers. The existing SpiritWater overlap state
also uses a set and discards freed/dead enemies before applying damage.

### Projectile weapon runtime

Phase 1.3 preserves the Starter Kit's nearest-enemy projectile path while
separating its configured baseline values from its runtime behavior.
ProjectileWeaponDefinition is a Godot Resource that owns base damage, attacks
per second, projectile speed, and the projectile PackedScene reference. The
development-only resource at Weapons/development_projectile_weapon.tres binds
the existing neutral bullet scene to those values.

Shooting owns the reusable behavior: timer cadence, target selection,
projectile instantiation, velocity, collision subscription, and applying
damage to Enemy. It stops its timer when its owning Player dies, so no new
projectiles are created after the terminal player state. ProjectileWeaponTiming
keeps the attack-rate-to-cooldown conversion pure and testable. Existing
IUpgradable compatibility remains unchanged; weapon evolution, fusion, and a
general weapon catalogue are not part of this phase.

The development bullet scene now uses `ProjectileLifetime`, whose pure
`ProjectileLifetimeState` expires after its configured eight seconds. A bullet
still frees immediately on collision through `Shooting`; the lifetime provides
the required terminal path for a miss, so missed RigidBody3D projectiles cannot
grow without bound during sustained firing.

### Level-up selection and temporary upgrades

When Player raises LeveledUp, GameManager updates the XP bar, pauses the scene
tree, and passes a small set of eligible definitions from the development
catalog to the existing UpgradeView. The view continues processing while the
tree is paused. Selecting a choice calls ITemporaryUpgradeReceiver on player
child nodes; Shooting is the initial receiver and accepts only its three
neutral projectile effects. GameManager tracks per-upgrade run applications,
then resumes the scene tree after the choice. If one XP collection crosses
multiple thresholds, it queues one choice for each level before resuming.

These applications and their counts are temporary run state. They are not a
shop, permanent profile, passive-item system, weapon evolution, or final
upgrade catalogue.

### Timed run pressure, boss, and results

Phase 1.5 adds RunPressureState, a pure C# run-lifecycle model with
NotStarted, Active, Paused, Completed, and Stopped states. It owns elapsed run
time, deterministic stage selection, stage-transition results, spawn interval,
and active-enemy cap. It advances only while Active; pause, death-driven stop,
and completion prevent further time or pressure progression.

GameManager starts the run when Player is ready. It reads the current pressure
to retain the existing EnemyManager spawn path while using the configured
interval and cap. It pauses and resumes RunPressureState with the existing
level-up view and stops it on Player.Died. When the final stage completes,
the completed pressure state prevents further normal spawning and GameManager
spawns the configuration's one neutral completion boss.

Phase 1.6 adds RunResultState, a pure C# Running/Victory/Defeat model. It
allows only one terminal transition, so a boss death cannot turn an already
defeated run into Victory and player death cannot turn an already victorious
run into Defeat. The boss is still an existing Enemy, so it uses the same
health, damage, terminal death, and XP-drop path. Its Died event changes the
run result to Victory; Player.Died changes it to Defeat. Either result pauses
the tree and RunResultsView displays the result, level, elapsed time, and
Restart. Restart clears tracked enemies and reloads the current scene, which
creates fresh Player, attack, XP/progression, and HUD node state.

## Target logical boundaries

The project will retain Godot scenes for composition and Resources for content,
but later code must divide responsibility as follows.

| Boundary | Owns | Must not own |
| --- | --- | --- |
| Core Gameplay | temporary run state, movement, combat rules, spawning, waves, XP, level-up, victory and defeat | character names, scripture, story order, permanent rewards |
| Meta Progression | currency, permanent upgrades, global unlocks, collections and reward application | temporary enemy instances or active-run combat |
| Character | character and era definitions, base stats, starting loadout, passive definition and unlock links | generic combat implementation or narrative selection logic |
| Bible Content | structured people, eras, locations, events and scripture references | gameplay code paths or full translation text without rights approval |
| Narrative | story arcs, ordered chapters, encounter eligibility, narration data and quiz linkage | random cross-chapter selection or direct combat ownership |
| Persistence | separate serialization and migration of run state, profile progression and settings | scene-node references or UI layout |
| UI | presentation, input routing and view state | authoritative gameplay, progression or narrative decisions |

The boundary names describe ownership, not a mandate to create all folders or
systems now. Each one is introduced only in its roadmap phase.

### Dependency direction

Future dependencies should flow toward stable data and contracts:

    UI ───────────────┐
    Core Gameplay ────┼──> domain contracts and content definitions
    Meta Progression ─┤
    Narrative ────────┤
    Persistence ──────┘

Character and Bible Content definitions may be consumed by narrative and by
run setup. Core Gameplay may consume neutral gameplay values such as stats,
loadout entries, weapon definitions, or enemy definitions. It must not branch
on a Biblical character, event, book, or chapter identifier.

Narrative may ask the current character progression which chapter is active
and may request an eligible encounter for that chapter. It must not choose
from every encounter in the game. Persistence records the resulting durable
state, while Core Gameplay retains only the state needed for the active run.

## Data-driven content rule

Godot Resources are the preferred content format because the repository
already uses them and Godot can inspect, reference, and version them directly.
Future definition resources can include, when their owning phase begins:

| Definition | Introduced by | Purpose |
| --- | --- | --- |
| ProjectileWeaponDefinition | Phase 1.3 | describes one generic projectile's base values and projectile-scene reference |
| TemporaryUpgradeDefinition and TemporaryUpgradeCatalog | Phase 1.4 | describes neutral temporary upgrade effects, amounts, limits, and a small authored choice pool |
| StagePressureDefinition and StagePressureConfiguration | Phase 1.5/1.6 stabilization | describes neutral timed pressure stages, spawn/population scaling, player-safe spawn distance, bounded placement attempts, and total run duration |
| BossDefinition | Phase 1.6 | describes the neutral completion boss id, label, and existing generic Enemy scene reference |
| WeaponDefinition and EnemyDefinition | later core gameplay / weapons | describes broader values and prefab or behavior references once their owning scope requires them |
| CharacterDefinition and CharacterEraDefinition | character system | selects base stats, passive, starting loadout and progression links |
| ScriptureReference | Bible content | book, chapter, verse range and translation identifier only |
| StoryArcDefinition and StoryChapterDefinition | story arcs | ordered, character-scoped narrative structure |
| EncounterDefinition and QuizDefinition | encounters / quizzes | chapter-scoped eligibility, presentation and reward links |
| AchievementDefinition | achievements | data-driven condition metadata and rewards |

Definitions reference stable content identifiers instead of display text where a
relationship must survive renaming. Content-specific display text belongs in
the definition that owns the content, not in generic gameplay C#.

The existing Powerup resources are a useful precedent but are not yet a full
definition framework: their static C# registry and enum-driven implementation
will need an incremental replacement in the core gameplay and weapon phases.

## Character extensibility contract

After the character phase, adding a character should consist primarily of
adding definitions and referenced presentation assets:

1. Character and era definitions select neutral gameplay values and a starting
   loadout.
2. Story and encounter definitions refer to the character identifier and
   ordered chapter identifiers.
3. Achievement and unlock definitions refer to identifiers, not character
   specific C# conditions.
4. Generic runtime systems consume definitions through common interfaces.

No generic combat, enemy, progression, or narrative selector should need a
new conditional branch for a named Biblical character. This is a required
acceptance check for future character work.

## Reuse, modification, and preservation

| Status | Current assets and code | Phase 0 decision |
| --- | --- | --- |
| Reuse | Godot C# setup, main 3D scene, player movement, prefab composition, attack components, enemy prefabs, Jolt configuration, Resource-based upgrade assets, basic HUD, damage-label pooling | retain as the baseline to assess and evolve |
| Modify incrementally | GameManager ownership, EnemyManager prefab registry, PowerupPaths static registry, enum/switch upgrade dispatch, scene-tree path dependencies, mixed player/enemy level-up choices, run lifecycle and data ownership | document now; alter only in the owning future phase |
| Leave untouched initially | imported third-party assets, addon code and binaries, GridMap art, existing scenes, generated .import files, Godot engine migration changes already in the worktree | no Phase 0 changes |

## Architectural risks found

1. GameManager is already a central coordinator for unrelated concerns. Adding
   saves, narrative, characters, or meta progression there would create a God
   object and entangle temporary and durable state.
2. Fixed absolute node paths and direct scene loading couple gameplay code to
   the current MainScene and HUD hierarchy. UI or scene replacement could
   break runtime behavior.
3. Enemy variants and starter upgrade resources still depend on hard-coded
   enum cases and path lists. Shooting is now definition-backed, but the other
   starter attacks and enemy scene registry remain scene-centric, so new
   content still often requires C# edits.
4. The player prefab starts with all four attacks, so it is not yet an
   appropriate template for character-specific starting loadouts.
5. Phase 1.4 removes enemy modifiers from the active level-up choice, but
   GameManager still directly loads one development catalog and tracks the
   temporary selection state. A later weapon/upgrade phase needs a neutral
   catalog-loading seam without an upfront registry rewrite.
6. Phase 1.6 result presentation and restart are intentionally minimal. The
   restart mechanism reloads the current scene; there is no save, reward, or
   scene-transition presentation system.
7. The existing worktree has a Godot 4.7.2 SDK/configuration update and many
   generated import changes. They are pre-existing and are intentionally not
   folded into this documentation phase.
8. Starter Resource metadata is not consistently aligned with its runtime
   behavior. For example, EnemySpawnRate is described as an explosion chance
   while EnemyManager uses it to increase spawn rate. Future content migration
   needs validation of both references and gameplay semantics.
9. Existing enemy variants are still configured through separate prefabs and
   EnemyManager's hard-coded scene registry. Phase 1.2 makes their runtime
   generic but deliberately does not yet introduce EnemyDefinition resources.

## Recommended migration strategy

1. Preserve the runnable starter kit while Phase 1 validates the basic
   Survivors loop. Do not start by reorganizing every script.
2. Continue migrating one combat content family at a time: Phase 1.3 supplies
   the first ProjectileWeaponDefinition without introducing a general weapon
   catalogue. Preserve the development resource and runtime seam until a
   separately scoped weapon phase needs a larger definition model.
3. Keep XP collection and temporary upgrade state run-scoped. Do not attach
   it to persistent progression before that phase is explicitly authorized.
4. Keep timed pressure data neutral and ordered. Future stage content may
   replace the development configuration, but it must not make stage selection
   depend on enemy type, wave clears, narrative state, or Bible identifiers.
5. When a Phase 1 need crosses the current GameManager boundary, extract the
   smallest focused runtime service or pure domain class instead of adding a
   new responsibility to GameManager.
6. Move hard-coded content lookup behind a neutral content-loading seam only
   when Phase 1 or 2 needs it. Migrate one content family at a time and keep
   existing resources playable during the transition.
7. Establish an explicit temporary RunState before introducing persistent
   rewards. Add profile persistence only in the meta-progression phase.
8. Add character definitions only after the core loop is stable. Connect
   character data to run setup rather than embedding it in Player or weapons.
9. Introduce scripture references, story arcs, encounters, quizzes, and audio
   in their roadmap order. Narrative selection must use ordered chapter state
   from the outset.

This strategy favors small compatibility-preserving changes over a wholesale
Starter Kit rewrite.

## Phase 0 non-goals

Phase 0 does not add runtime interfaces, folders for speculative systems,
resources, characters, Bible data, save files, encounters, story content,
quizzes, achievements, shops, or audio. It changes documentation only.
