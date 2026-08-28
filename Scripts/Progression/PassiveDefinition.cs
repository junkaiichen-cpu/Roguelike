using Godot;

public enum PassiveEffect
{
    Damage,
    MaxHealth,
    MoveSpeed,
    Cooldown,
    Area,
    ProjectileSize,
    ExperienceGain,
    PickupRange,
}

[GlobalClass]
public partial class PassiveDefinition : Resource
{
    [Export] public string Id { get; set; } = string.Empty;
    [Export] public string DisplayName { get; set; } = string.Empty;
    [Export] public string Description { get; set; } = string.Empty;
    [Export] public PassiveEffect Effect { get; set; }
    [Export] public float Amount { get; set; } = 1f;
    [Export] public uint MaxApplications { get; set; } = 5;
}
