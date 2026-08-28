using Godot;

[GlobalClass]
public partial class FusionDefinition : Resource
{
    [Export] public string Id { get; set; } = string.Empty;
    [Export] public string WeaponAId { get; set; } = string.Empty;
    [Export] public int WeaponARequiredLevel { get; set; } = 5;
    [Export] public string WeaponBId { get; set; } = string.Empty;
    [Export] public int WeaponBRequiredLevel { get; set; } = 5;
    [Export] public string ResultWeaponId { get; set; } = string.Empty;
    [Export] public string DisplayName { get; set; } = string.Empty;
}
