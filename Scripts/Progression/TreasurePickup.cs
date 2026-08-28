using Godot;

public partial class TreasurePickup : Area3D
{
    private MeshInstance3D _visual;
    private bool _opened;

    public override void _Ready()
    {
        _visual = GetNode<MeshInstance3D>("Visual");
        _visual.MaterialOverride = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = new Color(1f, 0.75f, 0.18f),
            EmissionEnabled = true,
            Emission = new Color(1f, 0.3f, 0.02f),
            EmissionEnergyMultiplier = 2.5f,
        };
        BodyEntered += OnBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (_opened) return;
        _visual.RotateY((float)delta * 1.5f);
        _visual.Scale = Vector3.One * (1f + 0.08f * Mathf.Sin(Time.GetTicksMsec() / 170f));
    }

    private void OnBodyEntered(Node3D body)
    {
        if (_opened || body is not Player player || player.IsDead) return;
        _opened = true;
        SetDeferred("monitoring", false);
        GetNode<GameManager>("/root/GameManager").CollectTreasure(player);
        Tween openTween = CreateTween();
        openTween.TweenProperty(_visual, "scale", Vector3.One * 1.35f, 0.08f);
        openTween.TweenProperty(_visual, "scale", Vector3.Zero, 0.14f);
        openTween.TweenCallback(Callable.From(QueueFree));
    }
}
