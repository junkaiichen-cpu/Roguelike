using Godot;

public partial class GroundFireAttack : Node3D, ITemporaryUpgradeReceiver
{
    [Export] public uint BaseDamage { get; set; } = 4;
    [Export] public float Radius { get; set; } = 4f;
    [Export] public float DurationSeconds { get; set; } = 4f;
    [Export] public float TickSeconds { get; set; } = 0.35f;

    private uint _damageBonus;
    private float _radiusBonus;
    private float _durationBonus;
    private float _tickReduction;
    private bool _unlocked;
    private Area3D _area;
    private MeshInstance3D _visual;
    private Timer _durationTimer;
    private Timer _tickTimer;
    private Timer _cooldownTimer;

    public bool IsUnlocked => _unlocked;

    public override void _Ready()
    {
        _area = new Area3D { CollisionLayer = 0, CollisionMask = 2, Monitoring = true };
        _area.AddChild(new CollisionShape3D { Shape = new CylinderShape3D { Radius = Radius, Height = 0.2f } });
        AddChild(_area);
        _visual = new MeshInstance3D
        {
            Visible = false,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            Mesh = new CylinderMesh { TopRadius = Radius, BottomRadius = Radius, Height = 0.08f },
            MaterialOverride = new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                AlbedoColor = new Color(1f, 0.22f, 0.04f),
                EmissionEnabled = true,
                Emission = new Color(1f, 0.08f, 0.01f),
            },
        };
        AddChild(_visual);
        _durationTimer = new Timer { OneShot = true };
        _durationTimer.Timeout += EndFire;
        AddChild(_durationTimer);
        _tickTimer = new Timer { WaitTime = TickSeconds };
        _tickTimer.Timeout += DamageEnemies;
        AddChild(_tickTimer);
        _cooldownTimer = new Timer { OneShot = true, WaitTime = 1.2f };
        _cooldownTimer.Timeout += BeginFire;
        AddChild(_cooldownTimer);
    }

    public bool Unlock()
    {
        if (_unlocked) return false;
        _unlocked = true;
        BeginFire();
        return true;
    }

    private void BeginFire()
    {
        if (!_unlocked) return;
        _visual.Visible = true;
        _durationTimer.Start(DurationSeconds + _durationBonus);
        _tickTimer.Start();
    }

    private void DamageEnemies()
    {
        if (!_unlocked) return;
        _tickTimer.WaitTime = Mathf.Max(0.15f, TickSeconds - _tickReduction);
        foreach (Node3D body in _area.GetOverlappingBodies())
        {
            if (body is Enemy enemy && !enemy.IsDead) enemy.TakeDamages(BaseDamage + _damageBonus);
        }

        _visual.Scale = Vector3.One * 0.95f;
        GetTree().CreateTween().TweenProperty(_visual, "scale", Vector3.One, 0.12f);
    }

    private void EndFire()
    {
        _visual.Visible = false;
        _tickTimer.Stop();
        _cooldownTimer.Start();
    }

    public bool TryApplyTemporaryUpgrade(TemporaryUpgradeDefinition upgrade)
    {
        if (upgrade == null || upgrade.Amount <= 0 || !float.IsFinite(upgrade.Amount)) return false;
        if (upgrade.Effect == TemporaryUpgradeEffect.UnlockFire) { Unlock(); return true; }
        if (!_unlocked) return false;

        switch (upgrade.Effect)
        {
            case TemporaryUpgradeEffect.FireDamage:
                _damageBonus += (uint)Mathf.Max(1, Mathf.FloorToInt(upgrade.Amount)); return true;
            case TemporaryUpgradeEffect.FireArea:
                _radiusBonus += upgrade.Amount;
                (_area.GetChild(0) as CollisionShape3D).Shape = new CylinderShape3D { Radius = Radius + _radiusBonus, Height = 0.2f };
                (_visual.Mesh as CylinderMesh).TopRadius = Radius + _radiusBonus;
                (_visual.Mesh as CylinderMesh).BottomRadius = Radius + _radiusBonus;
                return true;
            case TemporaryUpgradeEffect.FireDuration:
                _durationBonus += upgrade.Amount; return true;
            case TemporaryUpgradeEffect.FireFrequency:
                _tickReduction += upgrade.Amount; return true;
            default: return false;
        }
    }
}