using Godot;

[GlobalClass]
public partial class CharacterDefinition : Resource
{
    [Export] public string CharacterId { get; set; } = string.Empty;
    [Export] public string DisplayName { get; set; } = string.Empty;
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = string.Empty;
    [Export] public uint BaseHP { get; set; } = 200;
    [Export] public float BaseDamageMultiplier { get; set; } = 1f;
    [Export] public float BaseMoveSpeedMultiplier { get; set; } = 1f;
    [Export] public float BasePickupRangeMultiplier { get; set; } = 1f;
    [Export] public float BaseXPGainMultiplier { get; set; } = 1f;
    [Export] public float BaseLuckMultiplier { get; set; } = 1f;
    [Export] public WeaponPickupType StartingWeapon { get; set; } = WeaponPickupType.None;
    [Export] public string StartingPassiveId { get; set; } = string.Empty;
}