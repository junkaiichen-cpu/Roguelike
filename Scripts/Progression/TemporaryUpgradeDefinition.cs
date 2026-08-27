using Godot;

public enum TemporaryUpgradeEffect
{
    ProjectileDamage,
    ProjectileAttackSpeed,
    ProjectileSpeed,
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
