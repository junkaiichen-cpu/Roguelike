using Godot;

[GlobalClass]
public partial class ProjectileWeaponDefinition : Resource
{
    [Export(PropertyHint.Range, "1,1000,1")]
    public uint BaseDamage { get; set; } = 5;

    [Export(PropertyHint.Range, "0.1,30,0.1")]
    public float AttacksPerSecond { get; set; } = 1f;

    [Export(PropertyHint.Range, "0.1,100,0.1")]
    public float ProjectileSpeed { get; set; } = 5f;

    [Export]
    public PackedScene ProjectileScene { get; set; }
}
