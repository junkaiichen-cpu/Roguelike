using Godot;

public partial class CrossAttack : Node3D, ITemporaryUpgradeReceiver
{
    [Export]
    public uint BaseDamage { get; set; } = 8;

    [Export]
    public float Radius { get; set; } = 3.5f;

    [Export]
    public float CooldownSeconds { get; set; } = 4f;

    private uint _damageBonus;
    private float _radiusBonus;
    private float _cooldownReduction;
    private bool _unlocked;
    private Area3D _area;
    private Timer _timer;
    private MeshInstance3D _horizontal;
    private MeshInstance3D _vertical;

    public float TotalRadius => Radius + _radiusBonus;

    public bool IsUnlocked => _unlocked;

    public override void _Ready()
    {
        _area = new Area3D
        {
            CollisionLayer = 0,
            CollisionMask = 2,
            Monitoring = true,
        };
        _area.AddChild(new CollisionShape3D
        {
            Shape = new SphereShape3D { Radius = TotalRadius },
        });
        AddChild(_area);

        _horizontal = CreateBeam();
        _vertical = CreateBeam();
        _horizontal.Rotation = new Vector3(0, Mathf.Pi * 0.25f, 0);
        _vertical.Rotation = new Vector3(0, -Mathf.Pi * 0.25f, 0);
        AddChild(_horizontal);
        AddChild(_vertical);

        _timer = new Timer { OneShot = true, WaitTime = CooldownSeconds };
        _timer.Timeout += TriggerBurst;
        AddChild(_timer);
    }

    private MeshInstance3D CreateBeam()
    {
        return new MeshInstance3D
        {
            Visible = false,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            Mesh = new BoxMesh
            {
                Size = new Vector3(0.7f, 0.2f, TotalRadius * 2f),
            },
            MaterialOverride = new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                AlbedoColor = new Color(1f, 0.85f, 0.25f),
                EmissionEnabled = true,
                Emission = new Color(1f, 0.45f, 0.05f),
            },
        };
    }

    private void TriggerBurst()
    {
        if (!_unlocked) return;

        _timer.Start(Mathf.Max(0.2f, CooldownSeconds - _cooldownReduction));
        uint damage = BaseDamage + _damageBonus;
        foreach (Node3D body in _area.GetOverlappingBodies())
        {
            if (body is Enemy enemy && !enemy.IsDead)
            {
                enemy.TakeDamages(damage);
            }
        }

        _horizontal.Visible = true;
        _vertical.Visible = true;
        _horizontal.Scale = Vector3.Zero;
        _vertical.Scale = Vector3.Zero;
        Tween burstTween = CreateTween();
        burstTween.TweenProperty(_horizontal, "scale", new Vector3(1.25f, 1.25f, 1.25f), 0.07f);
        burstTween.Parallel().TweenProperty(_vertical, "scale", new Vector3(1.25f, 1.25f, 1.25f), 0.07f);
        burstTween.TweenProperty(_horizontal, "scale", Vector3.Zero, 0.11f);
        burstTween.Parallel().TweenProperty(_vertical, "scale", Vector3.Zero, 0.11f);
        burstTween.TweenCallback(Callable.From(() =>
        {
            _horizontal.Visible = false;
            _vertical.Visible = false;
        }));
    }

    public bool TryApplyTemporaryUpgrade(TemporaryUpgradeDefinition upgrade)
    {
        if (upgrade == null || upgrade.Amount <= 0 || !float.IsFinite(upgrade.Amount)) return false;

        switch (upgrade.Effect)
        {
            case TemporaryUpgradeEffect.UnlockCross:
                _unlocked = true;
                _timer.Start();
                return true;
            case TemporaryUpgradeEffect.CrossDamage:
                if (!_unlocked) return false;
                _damageBonus += (uint)Mathf.Max(1, Mathf.FloorToInt(upgrade.Amount));
                return true;
            case TemporaryUpgradeEffect.CrossSize:
                if (!_unlocked) return false;
                _radiusBonus += upgrade.Amount;
                if (_area?.GetChild(0) is CollisionShape3D shape
                    && shape.Shape is SphereShape3D sphere)
                {
                    sphere.Radius = TotalRadius;
                }
                UpdateBeamLengths();
                return true;
            case TemporaryUpgradeEffect.CrossCooldown:
                if (!_unlocked) return false;
                _cooldownReduction += upgrade.Amount;
                return true;
            default:
                return false;
        }
    }

    private void UpdateBeamLengths()
    {
        if (_horizontal?.Mesh is BoxMesh horizontalMesh)
        {
            horizontalMesh.Size = new Vector3(horizontalMesh.Size.X, horizontalMesh.Size.Y, TotalRadius * 2f);
        }

        if (_vertical?.Mesh is BoxMesh verticalMesh)
        {
            verticalMesh.Size = new Vector3(verticalMesh.Size.X, verticalMesh.Size.Y, TotalRadius * 2f);
        }
    }
}