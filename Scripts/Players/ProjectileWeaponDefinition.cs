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

    [Export(PropertyHint.Range, "1,16,1")]
    public int ProjectileCount { get; set; } = 1;

    [Export(PropertyHint.Range, "0,180,1")]
    public float SpreadDegrees { get; set; } = 14f;

    [Export(PropertyHint.Range, "0.1,10,0.05")]
    public float ProjectileSizeMultiplier { get; set; } = 1f;

    [Export]
    public PackedScene ProjectileScene { get; set; }
}
