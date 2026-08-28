# Phase 2 - Combat and Upgrade Feel

# Status
- status: completed
- date: 2026-08-27

# Goal
Make level-up choices immediately communicate stronger combat while extending the existing projectile weapon without adding new weapons or a new combat architecture.

# Changes
- Extended `ProjectileWeaponDefinition` with projectile count, spread degrees, and projectile size multiplier.
- Extended `Shooting` so one automatic attack can fire a bounded volley toward the current target.
- Volley shots are distributed symmetrically around the target direction using the configured spread angle.
- Projectile size is applied to each spawned bullet, making the upgrade visible in play.
- Added temporary upgrade effects for projectile count, spread, size, and a bounded projectile-count doubling upgrade.
- Expanded the existing development catalog from three choices to seven: damage, attack speed, projectile speed, projectile count, projectile spread, projectile size, and Volley Overload.
- Existing damage, attack-speed, and projectile-speed upgrades remain on the same receiver and UI path.

# Files
- `Scripts/Players/ProjectileWeaponDefinition.cs`
- `Scripts/Players/Shooting.cs`
- `Scripts/Progression/TemporaryUpgradeDefinition.cs`
- `Weapons/development_projectile_weapon.tres`
- `Upgrades/development_temporary_upgrade_catalog.tres`
- `Upgrades/development_projectile_count.tres`
- `Upgrades/development_projectile_spread.tres`
- `Upgrades/development_projectile_size.tres`
- `Upgrades/development_projectile_count_double.tres`

# Decisions
- Reused the current automatic-targeting and projectile scene path.
- Kept volley count bounded to avoid unbounded Android/rendering cost; the multiplier is capped at four times the base volley.
- Kept one target per attack and used directional spread rather than adding a new targeting system.
- XP gain, pickup radius, movement-speed, passive upgrades, and new weapons remain deferred to later phases.

# Known Issues
- Automated tests cover existing pure timing/progression logic but do not currently instantiate Godot `Shooting` nodes or simulate upgrade-card selection.
- Godot interactive combat feel still requires manual verification for volley readability, projectile size, and upgrade pacing.

# Next Phase
Proceed to Phase 3 only after manual confirmation that the projectile count/spread/size upgrades produce clearly readable combat changes.
