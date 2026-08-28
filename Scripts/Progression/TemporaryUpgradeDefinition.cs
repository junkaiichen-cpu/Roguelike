using Godot;

public enum TemporaryUpgradeEffect
{
    ProjectileDamage,
    ProjectileAttackSpeed,
    ProjectileSpeed,
    ProjectileCount,
    ProjectileSpread,
    ProjectileSize,
    ProjectileCountDouble,
    ProjectileDamagePercent,
    ProjectileAttackSpeedPercent,
    PickupRadiusPercent,
    ExperienceGainPercent,
    MoveSpeedPercent,
    CrossDamage,
    CrossSize,
    CrossCooldown,
    UnlockCross,
    LightningDamage,
    LightningCount,
    LightningFrequency,
    UnlockLightning,
    BibleDamage,
    BibleCount,
    BibleOrbitSpeed,
    BibleRadius,
    UnlockBible,
    OrbDamage,
    OrbCount,
    OrbSpeed,
    UnlockOrb,
    FireDamage,
    FireArea,
    FireDuration,
    FireFrequency,
    UnlockFire,
    SpiritWaterDamage,
    SpiritWaterDuration,
    SpiritWaterCooldown,
    UnlockSpiritWater,
    LifestealDamage,
    LifestealCooldown,
    UnlockLifesteal,
    LightningChainCount,
    FusionDamage,
    FusionArea,
    FusionFrequency,
    PassiveDamage,
    PassiveMaxHealth,
    PassiveMoveSpeed,
    PassiveCooldown,
    PassiveArea,
    PassiveProjectileSize,
    PassiveExperienceGain,
    PassivePickupRange,
}

[GlobalClass]
public partial class TemporaryUpgradeDefinition : Resource
{
    [Export]
    public string Id { get; set; } = string.Empty;

    [Export]
    public string DisplayName { get; set; } = string.Empty;

    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = string.Empty;

    [Export]
    public TemporaryUpgradeEffect Effect { get; set; }

    [Export(PropertyHint.Range, "0.1,1000,0.1")]
    public float Amount { get; set; } = 1f;

    [Export(PropertyHint.Range, "1,100,1")]
    public uint MaxApplications { get; set; } = 1;
}
