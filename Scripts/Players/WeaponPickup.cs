using Godot;

public enum WeaponPickupType
{
    Orb,
    Fire,
    SpiritWater,
    Lifesteal,
    Cross,
    Lightning,
    Bible,
}

public partial class WeaponPickup : Area3D
{
    [Export] public WeaponPickupType WeaponType { get; set; }
    private MeshInstance3D _visual;
    private bool _collected;
    private Vector3 _baseScale = Vector3.One;

    public override void _Ready()
    {
        _visual = GetNode<MeshInstance3D>("Visual");
        _baseScale = _visual.Scale;
        if (WeaponType == WeaponPickupType.Fire && _visual.Mesh is SphereMesh)
        {
            _visual.Mesh = new CylinderMesh { TopRadius = 1f, BottomRadius = 1.2f, Height = 0.12f };
        }
        else if (WeaponType == WeaponPickupType.SpiritWater && _visual.Mesh is SphereMesh)
        {
            _visual.Mesh = new CylinderMesh { TopRadius = 0.9f, BottomRadius = 0.9f, Height = 0.25f };
        }
        else if (WeaponType == WeaponPickupType.Lifesteal && _visual.Mesh is SphereMesh)
        {
            _visual.Mesh = new TorusMesh { InnerRadius = 0.65f, OuterRadius = 0.95f };
        }
        else if (WeaponType is WeaponPickupType.Cross or WeaponPickupType.Lightning && _visual.Mesh is SphereMesh)
        {
            _visual.Mesh = new BoxMesh { Size = new Vector3(0.35f, 0.35f, 2.2f) };
        }
        else if (WeaponType == WeaponPickupType.Bible && _visual.Mesh is SphereMesh)
        {
            _visual.Mesh = new BoxMesh { Size = new Vector3(1.3f, 0.2f, 0.9f) };
        }
        _visual.MaterialOverride = CreatePickupMaterial();
        BodyEntered += OnBodyEntered;
    }

    private StandardMaterial3D CreatePickupMaterial() => new()
    {
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        AlbedoColor = WeaponType == WeaponPickupType.Orb
            ? new Color(0.2f, 0.65f, 1f)
            : WeaponType == WeaponPickupType.Fire
                ? new Color(1f, 0.2f, 0.04f)
                : WeaponType == WeaponPickupType.SpiritWater
                    ? new Color(0.15f, 0.85f, 1f)
                    : WeaponType == WeaponPickupType.Cross
                        ? new Color(1f, 0.8f, 0.2f)
                        : WeaponType == WeaponPickupType.Lightning
                            ? new Color(0.4f, 0.8f, 1f)
                            : WeaponType == WeaponPickupType.Bible
                                ? new Color(0.85f, 0.65f, 0.25f)
                                : new Color(0.45f, 0.35f, 1f),
        EmissionEnabled = true,
        Emission = WeaponType == WeaponPickupType.Orb
            ? new Color(0.05f, 0.3f, 1f)
            : WeaponType == WeaponPickupType.Fire
                ? new Color(1f, 0.05f, 0.01f)
                : WeaponType == WeaponPickupType.SpiritWater
                    ? new Color(0.05f, 0.6f, 1f)
                    : WeaponType == WeaponPickupType.Cross
                        ? new Color(1f, 0.5f, 0.05f)
                        : WeaponType == WeaponPickupType.Lightning
                            ? new Color(0.1f, 0.5f, 1f)
                            : WeaponType == WeaponPickupType.Bible
                                ? new Color(0.6f, 0.3f, 0.05f)
                                : new Color(0.2f, 0.1f, 0.8f),
        EmissionEnergyMultiplier = 2f,
    };

    public override void _Process(double delta)
    {
        if (_collected) return;
        _visual.RotateY((float)delta * 1.5f);
        _visual.Scale = _baseScale * (1f + 0.08f * Mathf.Sin(Time.GetTicksMsec() / 180f));
    }

    private void OnBodyEntered(Node3D body)
    {
        if (_collected || body is not Player player) return;
        if (!player.ActivateWeapon(WeaponType)) return;
        _collected = true;
        SetDeferred("monitoring", false);
        Tween pickupTween = CreateTween();
        pickupTween.TweenProperty(_visual, "scale", Vector3.One * 1.25f, 0.08f);
        pickupTween.TweenProperty(_visual, "scale", Vector3.Zero, 0.1f);
        pickupTween.TweenCallback(Callable.From(QueueFree));
    }
}