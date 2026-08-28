# Phase 3 - Upgrade Feel and Progression

# Status
- status: completed
- date: 2026-08-27

# Goal
Make each level-up choice produce an immediately noticeable gameplay change while preserving the existing random three-choice flow and 15-minute run structure.

# Changes
- Reduced the development Player XP curve from `4 + 4 per level` through the Player scene configuration, shortening the early feedback loop without changing progression state logic.
- Extended `TemporaryUpgradeEffect` with percentage damage, percentage attack speed, pickup radius, XP gain, and movement speed effects.
- Shooting now applies percentage damage and attack-speed multipliers to the live projectile weapon.
- Player now implements the existing `ITemporaryUpgradeReceiver`; movement speed and XP gain apply immediately, and pickup-radius growth is passed to newly spawned XP pickups.
- ExperiencePickup now accepts a configurable `PickupRadius` and duplicates its collision shape before applying the runtime radius.
- Added distinct upgrade Resources for +2 projectiles, damage +25%/+50%, attack speed +15%/+25%, pickup radius +25%, XP gain +25%, and movement speed +10%.
- Kept existing count +1, spread, size, projectile speed, and volley doubling upgrades in the same catalog.
- Preserved the random three-choice selection and bounded projectile-count multiplier.

# Files
- `Scripts/Progression/TemporaryUpgradeDefinition.cs`
- `Scripts/Players/Shooting.cs`
- `Scripts/Players/Player.cs`
- `Scripts/Progression/ExperiencePickup.cs`
- `Scripts/GameManager.cs`
- `Prefabs/Progression/experience_pickup.tscn`
- `Prefabs/player.tscn`
- `Upgrades/development_temporary_upgrade_catalog.tres`
- `Upgrades/development_projectile_count_plus_two.tres`
- `Upgrades/development_projectile_damage_25.tres`
- `Upgrades/development_projectile_damage_50.tres`
- `Upgrades/development_projectile_attack_speed_15.tres`
- `Upgrades/development_projectile_attack_speed_25.tres`
- `Upgrades/development_pickup_radius_25.tres`
- `Upgrades/development_experience_gain_25.tres`
- `Upgrades/development_move_speed_10.tres`

# Decisions
- Reused the existing Resource catalog, GameManager application path, UpgradeView, and receiver interface.
- Percentage upgrades stack multiplicatively so high-value upgrades are visibly stronger without adding a second progression architecture.
- Pickup radius applies to newly spawned pickups; existing pickups retain their configured runtime radius.
- No fusion, new weapons, characters, persistence, or Android export work was started.

# Known Issues
- Automated tests do not instantiate Godot Shooting/Player nodes or click UpgradeView cards.
- Manual Godot playtest is required to confirm that the new volley, percentage, XP, pickup-radius, and movement upgrades feel appropriately distinct in the 15-minute run.

# Next Phase
After manual acceptance of Phase 3, proceed to Phase 4 multi-weapon work. Do not start fusion or passive architecture before that phase.
