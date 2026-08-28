using Godot;

public partial class FaithSurgePickup : Area3D
{
    private MeshInstance3D _visual;
    private bool _collected;

    public override void _Ready()
    {
        _visual = GetNode<MeshInstance3D>("Visual");
        _visual.MaterialOverride = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = new Color(1f, 0.8f, 0.2f),
            EmissionEnabled = true,
            Emission = new Color(1f, 0.3f, 0.02f),
            EmissionEnergyMultiplier = 2.5f,
        };
        BodyEntered += OnBodyEntered;
    }

    public override void _Process(double delta)
    {
        if (_collected) return;
        _visual.RotateY((float)delta * 1.8f);
        _visual.Scale = Vector3.One * (1f + 0.1f * Mathf.Sin(Time.GetTicksMsec() / 160f));
    }

    private void OnBodyEntered(Node3D body)
    {
        if (_collected || body is not Player player || player.IsDead) return;
        _collected = true;
        SetDeferred("monitoring", false);
        GameManager gameManager = GetNode<GameManager>("/root/GameManager");
        gameManager.ActivateExperienceVacuum(player);
        ColorRect flash = GetTree().CurrentScene.GetNodeOrNull<ColorRect>("HUD/PlayerDamageFlash");
        if (flash != null)
        {
            Color previousColor = flash.Color;
            flash.Color = new Color(1f, 0.78f, 0.15f, 0.3f);
            Tween flashTween = flash.CreateTween();
            flashTween.TweenProperty(flash, "color", previousColor, 0.28f);
        }
        Tween surgeTween = CreateTween();
        surgeTween.TweenProperty(_visual, "scale", Vector3.Zero, 0.16f);
        surgeTween.TweenCallback(Callable.From(QueueFree));
    }
}
