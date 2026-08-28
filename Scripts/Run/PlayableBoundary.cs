using Godot;

public partial class PlayableBoundary : StaticBody3D
{
    private const float WallThickness = 1f;
    private const float WallHeight = 4f;

    public override void _Ready()
    {
        StagePressureConfiguration configuration = GD.Load<StagePressureConfiguration>(
            "res://Stages/development_run_pressure.tres");
        if (configuration == null)
        {
            GD.PushError("The playable boundary could not load the stage pressure configuration.");
            return;
        }

        PlayableBounds bounds = new(
            configuration.PlayableMinimumX,
            configuration.PlayableMaximumX,
            configuration.PlayableMinimumZ,
            configuration.PlayableMaximumZ);
        ConfigureWall("West", new Vector3(bounds.MinimumX - WallThickness * 0.5f, WallHeight * 0.5f, 0),
            new Vector3(WallThickness, WallHeight, bounds.MaximumZ - bounds.MinimumZ + WallThickness * 2));
        ConfigureWall("East", new Vector3(bounds.MaximumX + WallThickness * 0.5f, WallHeight * 0.5f, 0),
            new Vector3(WallThickness, WallHeight, bounds.MaximumZ - bounds.MinimumZ + WallThickness * 2));
        ConfigureWall("North", new Vector3(0, WallHeight * 0.5f, bounds.MinimumZ - WallThickness * 0.5f),
            new Vector3(bounds.MaximumX - bounds.MinimumX + WallThickness * 2, WallHeight, WallThickness));
        ConfigureWall("South", new Vector3(0, WallHeight * 0.5f, bounds.MaximumZ + WallThickness * 0.5f),
            new Vector3(bounds.MaximumX - bounds.MinimumX + WallThickness * 2, WallHeight, WallThickness));
    }

    private void ConfigureWall(string wallName, Vector3 position, Vector3 size)
    {
        CollisionShape3D wall = GetNode<CollisionShape3D>(wallName);
        wall.Position = position;
        wall.Shape = new BoxShape3D { Size = size };
    }
}
