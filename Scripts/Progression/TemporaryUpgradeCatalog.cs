using Godot;

[GlobalClass]
public partial class TemporaryUpgradeCatalog : Resource
{
    [Export]
    public Godot.Collections.Array<TemporaryUpgradeDefinition> Upgrades { get; set; } = new();
}
