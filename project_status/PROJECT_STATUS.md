# Faith Fight Project Status

# Current Version Goal
Android-capable 15-minute Survivors-like foundation: the player grows stronger, handles increasing enemy numbers, encounters rare elites, and reaches a meaningful boss conclusion. Android release polish remains a later phase.

# Completed Phases
- Phase 0: documentation foundation.
- Phase 1.1: player health and death.
- Phase 1.2: enemy runtime and death.
- Phase 1.3: automatic projectile combat.
- Phase 1.4: XP and temporary level-up upgrades.
- Phase 1.5: timed escalating stage pressure.
- Phase 1.6: first playable run result and restart foundation.
- Phase 1: spawn/map foundation implemented; manual boundary and spawn-distribution confirmation remains noted.
- Phase 2: combat projectile volley and upgrade feel.
- Phase 3: upgrade feel and player growth implemented.
- Phase 4: performance foundation and multi-weapon foundation in progress.
- Phase 4.5: five-minute fast-combat and performance stabilization in progress.
- Rendering scale correction: ordinary KayKit enemy visuals were enlarged from the imported 0.0072 world scale to the intended gameplay scale.
- Retina performance correction: `window/stretch/mode="viewport"` keeps the 1280x720 project viewport while the window grows.

# Current Phase
- Phase 5: Elite Enemy + Combat HUD, with Phase 4.5 fast-combat verification retained.
- status: in_progress
- date: 2026-08-28

# Phase 5 Changes
- Existing parameterized EliteType now supports Charger, Tank, Archer, and Mage variants using the existing Minion scene.
- Charger periodically surges at double movement speed; Tank is larger, slower, high-health, and takes reduced damage.
- Archer maintains a ranged spacing band and attacks from extended range; Mage maintains spacing and attacks on a shorter area-style cadence.
- Elite visuals use larger scale and simple unshaded tinting; elite visuals do not add particles, lights, or shadows.
- Added event-driven HUD player status for `JK House`, HP, and level in the upper-right corner.
- Added a fixed bottom weapon bar that shows only acquired weapons and Bible count.
- HUD refreshes on health, experience/level, pickup, and upgrade events rather than rebuilding controls per frame.

# Phase 5 Known Issues
- Manual gameplay verification remains required for Elite behavior/readability, HUD placement on the target mobile aspect ratio, Orb/Fire pickup flow, and five-minute pacing.

# Phase 5 Gameplay Fixes
- Confirmed and removed two unintended default automatic damage weapons: `SpiritWater` and `LifestealAttack`, both with base `Damages = 1`.
- Both weapons remain as dormant Player child nodes and are activated only by their map pickups; their dedicated upgrade paths are registered in the temporary catalog.
- Added map pickups for all non-core weapons: Cross, Lightning, Bible, Orb, Fire, Spirit Water, and Lifesteal.
- Fire now cycles through active duration, cooldown, and reactivation while retaining one Area3D and one visual Mesh.
- Lightning now chains across two to five targets with reusable short-lived Mesh visuals and no chain Physics bodies.
- Archer/Mage elites now emit visible short-lived projectiles; Mage telegraphs the target ground for 0.6 seconds.
- Elite scale is now approximately 1.6x for Charger/Archer/Mage and 1.9x for Tank, with simple tinting and stronger death scale feedback.

# Phase 5 Verification Status
- IN_PROGRESS: automated validation pending after the current gameplay fixes.
- KNOWN_ISSUE: manual Godot playtest remains required for pickup collection, Fire cycling, chain readability, elite ranged telegraphs, HUD placement, and five-minute pacing.

# Current Gameplay Fixes
- Root cause of missing ordinary enemies: Player `_Ready()` bound the HUD before the HUD child labels had entered the scene tree; `CombatHud.Refresh()` threw a null-reference exception and interrupted Player initialization before the run could spawn enemies.
- Fixed by deferring CombatHud binding until its own `_Ready()` has initialized the labels; existing EnemyManager spawn timing, 360-degree placement, bounds, and elite branch remain unchanged.
- Bottom weapon HUD is now a permanent horizontal icon-plus-level inventory. It shows only owned weapons and derives levels from actual GameManager upgrade applications.
- Moved the two confirmed default `Damages = 1` automatic weapons, SpiritWater and Lifesteal, to map pickup activation; no new weapon was added.

# Current Verification
- IN_PROGRESS: final build/test/diff validation pending.
- KNOWN_ISSUE: manual gameplay confirmation remains required for ordinary spawn visibility, pickup activation, weapon level pop, and mobile HUD spacing.

# Current Gameplay Pass
- DONE (code): Map-only weapon unlock rule is enforced by removing all weapon Unlock resources from the active random upgrade array; map pickups remain the activation source.
- DONE (code): Elite deaths create one merged XP pickup worth approximately 20 normal XP and have a 15% Faith Surge drop chance.
- DONE (code): Faith Surge accelerates all existing XP pickups toward the player without creating new XP entities.
- DONE (code): Enemy spawn cycles use bounded time-based bursts of 1, 2, 3, or 4 ordinary enemies while preserving existing bounds and 360-degree placement; Elite rolls remain separate.
- DONE (code): Boss victory clears enemies, XP, Faith Surge, and projectile bodies, stops player combat child nodes, then delays Victory UI briefly.
- IN_PROGRESS: manual playtest is still required for Elite readability/behavior, Faith Surge feel, five-minute density, and Victory cleanup presentation.

# Phase 3 Changes
- Reduced the Player's development XP thresholds to `4 + 4 per level`.
- Added high-impact percentage damage and attack-speed upgrades.
- Added Player receiver support for XP gain, pickup radius, and movement speed.
- Added +2 projectile and distinct damage/attack-speed/pickup/XP/movement upgrade Resources.
- Preserved random three-choice selection and the existing bounded projectile volley system.

# Phase 4 Changes
- Reduced per-Bullet trail particles from 32 to 8 with a 0.35 second lifetime; projectile count and Volley Double remain intact.
- Added CrossAttack as a distinct Area3D burst weapon with lightweight cross-shaped mesh feedback.
- Added Cross damage, reach, and cooldown upgrade Resources to the existing catalog.
- Nearby ground XP drops now merge into higher-value pickups to reduce Area3D and collision-node count while preserving total XP.
- Normal spawn keeps only a limited offscreen preference so camera projection does not bias the uniform world ring toward the lower screen.
- Cross is now acquired through an `Unlock Cross` upgrade; its follow-up cards are unavailable before unlock.
- Enemy hit feedback uses a short lightweight health bar and existing flash instead of restarting the 50-particle hit effect.
- The development run now totals five minutes through 30/45/75/150 second stages.
- Player early XP thresholds are 3 XP with +3 per level to target 20-30 second early upgrades.
- Elite rolls begin at 60 seconds with 10% probability; normal HP scaling remains capped.
- LightningAttack is a locked instant-strike weapon with unlock, damage, count, and frequency upgrades.

# Phase 2 Changes
- Existing projectile weapon now supports a bounded volley with configurable count, symmetric spread, and projectile size.
- Existing temporary upgrade receiver now handles projectile count, spread, size, and a one-time bounded volley-doubling upgrade.
- Development upgrade catalog now offers seven combat choices: damage, attack speed, projectile speed, projectile count, projectile spread, projectile size, and Volley Overload.
- No new weapon, enemy, mobile export, XP-system, or meta-progression system was added.

# Changes This Phase
- `MapBoundary` is now a single scene body with four walls; duplicate walls and incomplete `MapBoundary2` were removed.
- The boundary script and enemy spawn logic read the same playable X/Z bounds from `Stages/development_run_pressure.tres`.
- Candidate validation requires both the normal safe-distance profile and playable-bounds containment.
- Deterministic fallback scans the ring for the farthest valid in-bounds point instead of using a fixed eastward position.
- The existing uniform 360-degree ring sampling and independent Boss spawn path are preserved.
- Focused placement tests cover out-of-bounds rejection and safe fallback.

# Files
- `Scripts/Enemies/EnemySpawnConfiguration.cs`
- `Scripts/Enemies/EnemyManager.cs`
- `Scripts/Run/StagePressureConfiguration.cs`
- `Scripts/Run/PlayableBoundary.cs`
- `Stages/development_run_pressure.tres`
- `Scenes/main_scene.tscn`
- `Tests/Enemies/EnemySpawnPlacementTests.cs`
- `tasks/phase1_spawn.md`
- `project_status/PROJECT_STATUS.md`
- `tasks/phase3_progression.md`
- `tasks/phase4_weapons.md`
- `Upgrades/development_unlock_cross.tres`
- `Scripts/Players/LightningAttack.cs`
- `Upgrades/development_lightning_damage.tres`
- `Upgrades/development_lightning_count.tres`
- `Upgrades/development_lightning_frequency.tres`
- `Upgrades/development_unlock_lightning.tres`

# Known Issues
- Headless Godot 4.4.1 startup passed for boundary wiring, but interactive edge-blocking and four-direction spawn distribution still require manual gameplay confirmation.
- Godot reports existing exit-time RID/resource leak messages during headless shutdown.
- Manual runtime confirmation is still required for Phase 3 upgrade feel and pacing.
- Phase 4 manual verification is still required for high-volley performance and Cross combat behavior.
- Phase 4 manual verification is also required for XP merge feel, spawn distribution, Cross unlock flow, and enemy health bars.
- Phase 4.5 manual verification is required for five-minute pacing, Lightning unlock/use, late density, and boss timing.

# Next Step
Manually verify Phase 4.5 five-minute pacing, Lightning behavior, late density, and boss timing before adding more weapons. Do not start fusion, character, or Android-release systems.

# Phase 4 Weapon Expansion Follow-up
- status: in_progress
- date: 2026-08-27

# Changes
- Brightened the temporary enemy health-bar fill to a readable deep red.
- XP attraction now uses a slightly slower eased rotating path with value-based scale/color and merge-pop feedback.
- Added locked BibleAttack orbit weapon using the existing spellbook asset, capped at four orbiting books with lightweight Area3D contact damage.
- Added Bible unlock, damage, count, orbit-speed, and radius upgrades with ownership-aware filtering.
- Edge-shadow investigation found MapBoundary has collision shapes only and no renderable shadow-casting geometry; global lighting settings were not changed without stronger runtime evidence.

# Known Issues
- Manual verification is still required for Bible orbit behavior, Bible upgrade filtering, XP visual tiers, and the actual source of edge shadows.

# Phase 4 Visual Feedback Follow-up
- status: in_progress
- date: 2026-08-27

# Changes
- Reduced development XP requirements to `2 XP` initially and `+2 XP` per level.
- Enlarged Bible visuals and added a short hit scale pop; Bible model shadows are disabled.
- Replaced Lightning's small marker with a broad shadowless cylinder strike.
- Enlarged and shadow-disabled Cross and Holy Light visuals.
- No standalone decorative player particle was found; existing weapon particles remain intact.
- No existing Ground Fire weapon was found, so Fire remains deferred.

# Known Issues
- Manual verification is required for visual feedback strength, upgrade cadence, XP tiers, and future Fire weapon priority.

# Orb and Ground Fire Pickup Follow-up
- status: in_progress
- date: 2026-08-28

# Changes
- Orb/FloatingSphere now starts inactive and activates from one fixed in-bounds Orb pickup; its existing counter-clockwise orbit and four-sphere cap remain.
- Added a minimal GroundFireAttack with one Area3D damage zone and one large shadowless cylinder visual.
- Added one reusable shadowless weapon pickup scene with Orb and Fire variants placed inside shared playable bounds.
- Added Orb and Fire weapon-specific upgrades with ownership-aware filtering.
- Preserved XP total value, existing weapons, projectile count, Volley Double, and performance viewport settings.

# Known Issues
- Manual verification is required for map pickup collection, Orb/Fire activation, Ground Fire damage, and upgrade filtering.

# Weapon Feedback Pass
- status: in_progress
- date: 2026-08-28

# Changes
- Brightened Enemy health bars, enlarged Holy Light/Cross/Lightning/Bible feedback, and disabled low-value weapon/enemy/XP shadows.
- Cross Reach now changes both the AOE hit radius and beam length; Cross uses a short large mesh pop.
- Bible remains capped at four orbiting books with lightweight collision and hit pop.
- No standalone ambient Player particle was found; weapon-linked particles were preserved.
- No Ground Fire weapon exists yet, so Fire remains deferred.

# Known Issues
- Manual verification is required for weapon silhouettes, Cross/Lightning/Bible readability, and the remaining edge-shadow source.

# Shadow Investigation Follow-up
- status: in_progress
- date: 2026-08-27

# Changes
- Runtime shadow enumeration found low-value casters in player weapon effects and active scene geometry; MapBoundary itself had collision shapes only.
- Disabled shadow casting on Enemy visuals, XP meshes, FloatingSphere visual/particles, and Lifesteal mesh/particles. Bullet, Bible, Lightning, and Cross were already shadow-disabled.
- Preserved player and ground DirectionalLight shadows. Current main-scene Environment does not enable SDFGI, so it was left unchanged.

# Known Issues
- Manual visual confirmation is still required for the reported left-lower edge shadow source after this cleanup.
