using Godot;

[GlobalClass]
public partial class BossDefinition : Resource
{
    [Export]
    public string Id { get; set; } = "development_boss";

    [Export]
    public string DisplayName { get; set; } = "Development Boss";

    [Export]
    public PackedScene EnemyScene { get; set; }
}
