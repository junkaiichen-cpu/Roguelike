# Phase 4 - Performance Foundation and Multi-Weapon Foundation

# Status
- status: in_progress
- date: 2026-08-27

# Phase 4.5 Fast Combat
- status: in_progress
- date: 2026-08-27

# Goal
Preserve projectile-count power fantasy while reducing per-projectile cost, then add one distinct second weapon without building fusion, character, or Android-release systems.

# Changes
- Reduced the existing Bullet trail particle amount from 32 to 8 and its lifetime to 0.35 seconds. Projectile count and Volley Double remain unchanged.
- Added `CrossAttack` as a Player child weapon. It uses an Area3D overlap query for burst damage and two lightweight unshaded BoxMesh beams for a short cross-shaped visual burst.
- Added Cross-specific upgrade effects for damage, reach, and cooldown.
- Added three Cross upgrade Resources to the existing catalog.
- Kept Holy Light represented by the existing `Shooting` projectile weapon; no duplicate projectile weapon was created.
- Ground XP drops within 2 world units now merge into an existing pickup with increased value, reducing Area3D and collision-node growth without reducing XP.
- Spawn direction selection now limits the screen-offscreen preference to the first three valid attempts, preserving a more even world-ring distribution while still usually hiding new enemies offscreen.
- Cross starts locked and is acquired through the `Unlock Cross` upgrade; Cross-specific upgrades are filtered until acquisition.
- Enemy hit feedback now uses a short lightweight health bar and the existing flash without restarting the 50-particle hit effect.
- Development stage durations are now 30, 45, 75, and 150 seconds for a five-minute run.
- Early Player XP thresholds are now 3 XP with +3 per level for faster upgrade cadence.
- Elite eligibility begins at 60 seconds with a modest 10% spawn roll; normal enemy HP scaling remains capped.
- Added locked LightningAttack with instant target strikes, Lightning Count, Damage, Frequency, and Unlock Lightning upgrades.

# Files
- `Prefabs/Powerups/bullet.tscn`
- `Scripts/Progression/TemporaryUpgradeDefinition.cs`
- `Scripts/Players/CrossAttack.cs`
- `Prefabs/player.tscn`
- `Upgrades/development_temporary_upgrade_catalog.tres`
- `Upgrades/development_cross_damage.tres`
- `Upgrades/development_cross_size.tres`
- `Upgrades/development_cross_cooldown.tres`
- `Scripts/Progression/ExperiencePickup.cs`
- `Scripts/GameManager.cs`
- `Scripts/Enemies/EnemyManager.cs`
- `Scripts/Enemies/Enemy.cs`
- `Prefabs/Progression/experience_pickup.tscn`
- `Upgrades/development_unlock_cross.tres`
- `Scripts/Players/LightningAttack.cs`
- `Upgrades/development_lightning_damage.tres`
- `Upgrades/development_lightning_count.tres`
- `Upgrades/development_lightning_frequency.tres`
- `Upgrades/development_unlock_lightning.tres`
- `Stages/development_pressure_stage_01.tres`
- `Stages/development_pressure_stage_02.tres`
- `Stages/development_pressure_stage_03.tres`
- `Stages/development_pressure_stage_04.tres`

# Decisions
- Kept projectile quantity and Volley Double intact; lowered only the low-priority continuous Bullet particle trail.
- Reused the existing Player child composition and `ITemporaryUpgradeReceiver` path.
- Cross is an area burst rather than another projectile variant, with no external assets or particle system.
- Cross upgrades are handled by CrossAttack after Shooting declines them, so weapon-specific choices remain separated by receiver behavior.
- Fusion, Lightning, Bible, new characters, complex enemy systems, and Android export remain deferred.

# Known Issues
- Runtime profiler timing and draw-call counters are not directly available through the current Godot 4.4.1 C# API used by this project.
- Manual Godot verification is required for Cross overlap damage, visual readability, upgrade-card distribution, and high-volley performance.
- Automated tests do not instantiate Godot weapon nodes or simulate full UpgradeView selection.
- Manual verification is required for XP merging, spawn distribution, Cross unlock flow, and health-bar readability.
- Five-minute runtime verification is required for upgrade cadence, Lightning unlock/use, late-stage density, and boss timing.

# Next Phase
Complete Phase 4 only after manual confirmation of Bullet performance and Cross gameplay. Further weapon types should wait for that validation.

# Phase 4 Weapon Expansion Follow-up
- status: in_progress
- date: 2026-08-27

# Changes
- Brightened the temporary enemy health-bar fill to a readable deep red.
- Slowed XP attraction to 0.15 seconds and added low-cost easing, rotation, approach scaling, value-based scale/color tiers, and merge pop feedback.
- Added locked `BibleAttack`, an orbit weapon using the existing spellbook asset, with at most four books and lightweight Area3D contact damage.
- Added Bible unlock, damage, count, orbit-speed, and radius upgrade Resources with ownership-aware filtering.
- Confirmed MapBoundary contains collision shapes only, so it is not the source of visible edge shadows; global lighting was left unchanged.

# Known Issues
- Manual verification remains required for Bible visibility, orbit damage, Bible upgrade flow, XP tier readability, and the actual edge-shadow source.

# Phase 4 Visual Feedback Follow-up
- status: in_progress
- date: 2026-08-27

# Changes
- Reduced the development level-up curve to `2 XP` initially and `+2 XP` per level.
- Enlarged the Bible orbit model to `1.25x` and added a short per-book scale pop on contact damage.
- Replaced the small Lightning sphere marker with a broad cylinder strike, kept short-lived and shadowless.
- Enlarged the Cross beams and disabled their shadow casting.
- Enlarged the Holy Light projectile visual and disabled its shadow casting.
- Kept player-adjacent particles that belong to existing weapon attack feedback; no standalone ambient player particle was found.
- No existing Ground Fire weapon was found, so no Fire system was added in this focused pass.

# Known Issues
- Manual verification remains required for the stronger Bible/Lightning/Cross/Holy Light readability, five-minute level-up cadence, and whether a future Fire weapon is needed.

# Orb and Ground Fire Pickup Follow-up
- status: in_progress
- date: 2026-08-28

# Changes
- Existing FloatingSphere/Orb now starts inactive and is activated by the fixed Orb map pickup.
- Orb keeps its existing counter-clockwise rotation, caps at four spheres, and uses existing damage/count/speed upgrade paths.
- Enlarged the Orb visual and added a short scale pop on enemy contact; Orb shadows remain disabled.
- Added a minimal GroundFireAttack with one large Area3D damage zone, one shadowless cylinder visual, duration/tick/area/damage upgrades, and no particles.
- Added one Orb pickup and one Fire pickup inside the shared playable bounds using one reusable shadowless `weapon_pickup.tscn` scene.
- Added ownership filtering so Orb/Fire-specific upgrades appear only after the corresponding pickup activation.

# Known Issues
- Manual verification remains required for pickup positions, Orb/Fire activation, Orb contact damage, Ground Fire duration/area behavior, and upgrade-card availability.

# Phase 5 Handoff
- Phase 5 Elite Enemy and event-driven combat HUD work has started in the existing runtime boundaries.
- Phase 4/4.5 remains in progress until manual five-minute gameplay verification is completed.

# Current Gameplay Fixes
- Confirmed `SpiritWater` was an unintended default automatic damage weapon with `Damages = 1`; it now starts dormant and is activated by `SpiritWaterPickup`.
- Confirmed `LifestealAttack` was also a default automatic damage/healing weapon with `Damages = 1`; it now starts dormant and is activated by `LifestealPickup`.
- Added map pickups for Spirit Water, Lifesteal, Cross, Lightning, Bible, Orb, and Fire using the existing rotating/pulsing shadowless pickup scene.
- Fire now repeats its active duration after a short cooldown instead of stopping permanently after its first duration.
- Lightning now chains across two to five nearby targets using reusable short-lived cylinder visuals and a bounded Chain Count upgrade.
- Archer and Mage elite attacks now use short-lived visible projectiles; Mage shows a brief ground warning before firing.

# Known Issues
- Manual playtest is still required for pickup placement, all weapon activation flows, Fire repeat timing, Lightning chain readability, elite telegraph readability, and mobile HUD placement.

# Ordinary Spawn and Weapon HUD Fix
- Fixed a startup-order regression where Player bound CombatHud before HUD labels were ready; the exception interrupted Player initialization and prevented ordinary Enemy spawning.
- Bottom inventory now uses a permanent horizontal icon-plus-level layout with no weapon names or combat-stat text.
- Weapon levels are derived from actual GameManager upgrade application counts; pickup starts at level 1 and subsequent upgrades increment the displayed level.

# Gameplay Feel Pass
- Removed all map-weapon Unlock entries from the active temporary upgrade array; Cross, Lightning, Bible, Orb, Fire, Spirit Water, and Lifesteal now unlock only through their map pickups.
- Elite deaths award one merged XP pickup worth approximately 20 normal XP and can drop one Faith Surge pickup at 15% probability.
- Faith Surge directly accelerates every existing XP pickup toward the player.
- Ordinary enemy spawning now uses bounded time-based bursts of 1/2/3/4 while Elite spawning remains a separate roll and playable bounds are unchanged.
- Boss death now performs a short cleanup phase before the Victory UI appears.

# Known Issues
- Manual gameplay verification remains required for Burst density, Faith Surge pickup/vacuum feel, Elite behavior, and Victory cleanup presentation.

# Weapon Feedback Pass
- status: in_progress
- date: 2026-08-28

# Changes
- Enlarged the Holy Light visual mesh and disabled both projectile and projectile-particle shadows.
- Enlarged Cross beams and changed the burst to a short `0 -> 1.25x -> 0` mesh pop; Cross Reach now updates the visual beam length as well as the hit area.
- Reworked Lightning feedback as a broad shadowless cylinder strike.
- Enlarged Bible orbit visuals, disabled imported Bible mesh shadows, and retained a short hit scale pop.
- Enemy damage remains a short hit flash plus temporary lightweight health bar; no damage numbers or hit particles are used.
- No standalone ambient Player particle was found. Existing weapon particles remain because they are tied to weapon feedback.
- No Ground Fire weapon exists yet; Fire remains deferred rather than adding a fifth weapon in this visual-only pass.

# Known Issues
- Manual Godot verification is required for the stronger weapon silhouettes, Cross burst, Lightning readability, Bible visibility, and five-minute upgrade cadence.

# Shadow Investigation Follow-up
- status: in_progress
- date: 2026-08-27

# Changes
- Runtime enumeration found low-value shadow casters in player weapon effects and active scene geometry; MapBoundary had no renderable geometry.
- Disabled shadow casting on runtime Enemy visuals, XP pickup meshes, FloatingSphere visual/particles, Lifesteal mesh/particles, and retained the existing Bullet/Bible/Lightning/Cross shadow-disabled settings.
- Kept player body and ground DirectionalLight shadow support; SDFGI was not enabled in the current main-scene Environment, so it was not changed.

# Known Issues
- The precise visual owner of the reported left-lower edge shadow requires manual visual confirmation after the low-value caster cleanup.
