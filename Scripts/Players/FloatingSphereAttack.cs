using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class FloatingSphereAttack : Node3D, IUpgradable, ITemporaryUpgradeReceiver
{
    [Export]
    public uint InitialSpheres = 1;

    [Export]
    public uint Damages = 3;

    private uint _damagesBonus = 0;

    public uint TotalDamages => Damages + _damagesBonus;

    [Export]
    public float RotationSpeed { get; private set; } = 1;

    [Export]
    public float Duration { get; private set; } = 3;

    [Export]
    public float SphereDistance { get; private set; } = 1.4f;

    [Export]
    private PackedScene _spherePrefab;
    private List<Area3D> _spheres = new();
    private Timer _timer;
    private Timer _recoveryTimer;
    private bool _unlocked;

    public bool IsUnlocked => _unlocked;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        foreach (int i in Enumerable.Range(0, (int)InitialSpheres))
            AddSphere();

        _timer = GetNode<Timer>("Timer");
        _timer.WaitTime = Duration;
        _timer.Timeout += OnAttackEnd;
        _timer.Stop();
        _recoveryTimer = new Timer { OneShot = true, WaitTime = 2d };
        _recoveryTimer.Timeout += ResumeAttack;
        AddChild(_recoveryTimer);
        HideSpheres();
    }

    private void OnBodyEntered(Area3D sphere, Node body)
    {
        if (body is not Enemy enemy) return;
        enemy.TakeDamages(TotalDamages);
        MeshInstance3D visual = sphere.GetNodeOrNull<MeshInstance3D>("Visual");
        if (visual == null) return;

        visual.Scale = Vector3.One * 1.25f;
        GetTree().CreateTween().TweenProperty(visual, "scale", Vector3.One, 0.1f);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        RotateY((float)delta * RotationSpeed);

        //if (_rb.GetContactCount() == 0) return;
        //foreach (var contact in _rb.GetCollidingBodies())
        //{
        //    if (contact is not Enemy enemy) continue;
        //    enemy.TakeDamages(Damages);
        //}
    }

    private void AddSphere()
    {
        Area3D area = _spherePrefab.Instantiate<Area3D>();
        area.BodyEntered += body => OnBodyEntered(area, body);
        AddChild(area);
        _spheres.Add(area);
        area.Monitoring = false;

        RepositionSpheres();
        area.Hide();
    }

    private void RepositionSpheres()
    {
        float axis;
        Vector3 basePosition = new(SphereDistance, 0.3f, 0);
        for (int i = 0; i < _spheres.Count; i++)
        {
            axis = Mathf.Lerp(0, 360, (float)i / _spheres.Count);
            Basis basis = new Basis(new Vector3(0, 1, 0), Mathf.DegToRad(axis));
            _spheres[i].Position = basis * basePosition;
        }
    }

    private void OnAttackEnd()
    {
        HideSpheres();
        _recoveryTimer.Start();
    }

    private void ResumeAttack()
    {
        if (!_unlocked) return;
        _timer.Start();
        ShowSpheres();
    }

    private void ShowSpheres()
    {
        foreach (var area in _spheres)
        {
            area.GetNode<GpuParticles3D>("Particles").Restart();
            area.GetNode<GpuParticles3D>("Particles").Emitting = true;
            area.SetPhysicsProcess(true);
            area.Monitoring = true;
            area.Show();
        }
    }

    public bool Unlock()
    {
        if (_unlocked) return false;
        _unlocked = true;
        ShowSpheres();
        _timer.Start();
        return true;
    }

    private void HideSpheres()
    {
        foreach (var area in _spheres)
        {
            area.GetNode<GpuParticles3D>("Particles").Emitting = false;
            area.SetPhysicsProcess(false);
            area.Monitoring = false;
            area.Hide();
        }
    }

    public bool TryApplyTemporaryUpgrade(TemporaryUpgradeDefinition upgrade)
    {
        if (upgrade == null || upgrade.Amount <= 0 || !float.IsFinite(upgrade.Amount)) return false;

        switch (upgrade.Effect)
        {
            case TemporaryUpgradeEffect.UnlockOrb:
                Unlock();
                return true;
            case TemporaryUpgradeEffect.OrbDamage:
                if (!_unlocked) return false;
                _damagesBonus += (uint)Mathf.Max(1, Mathf.FloorToInt(upgrade.Amount));
                return true;
            case TemporaryUpgradeEffect.OrbCount:
                if (!_unlocked || _spheres.Count >= 4) return false;
                AddSphere();
                return true;
            case TemporaryUpgradeEffect.OrbSpeed:
                if (!_unlocked) return false;
                RotationSpeed += upgrade.Amount;
                return true;
            default:
                return false;
        }
    }

    public void Upgrade(PowerupType powerupType)
    {
        switch (powerupType)
        {
            case PowerupType.FloatingSphereCount:
                AddSphere();
                break;
            case PowerupType.FloatingSphereDamages:
                _damagesBonus += 1;
                break;
            default: break;
        }
    }
}
