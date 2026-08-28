using Godot;
using System.Collections.Generic;

public partial class SpiritWater : Node3D, IUpgradable, ITemporaryUpgradeReceiver
{
    [Export]
    public uint Damages = 1;

    private uint _damagesBonus = 0;

    public uint TotalDamages => Damages + _damagesBonus;

    [Export]
    public float Duration = 4f;

    private float _durationBonus = 0;

    public float TotalDuration => Duration + _durationBonus;

    [Export]
    public float Cooldown = 5f;

    private float _cooldownBonus = 0;

    public float TotalCooldown => Cooldown - _cooldownBonus;

    [Export]
    public float ProjectileRange = 2;

    [Export]
    public PackedScene ProjectilePrefab;
    private Timer _projectileCooldown;
    private Timer _damageCooldown;

    private readonly HashSet<Enemy> _enemies = new();
    private GameManager _gameManager;
    private bool _unlocked;

    public bool IsUnlocked => _unlocked;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
        _projectileCooldown = GetNode<Timer>("ProjectileCooldown");
        _projectileCooldown.WaitTime = TotalCooldown;
        _projectileCooldown.Timeout += OnAttackReady;

        _damageCooldown = GetNode<Timer>("DamageCooldown");
        _damageCooldown.Timeout += OnDamageReady;
    }

    public bool Unlock()
    {
        if (_unlocked) return false;
        _unlocked = true;
        _projectileCooldown.Start();
        _damageCooldown.Start();
        return true;
    }

    private void OnAttackReady()
    {
        if (!_unlocked) return;
        _projectileCooldown.Start();
        var projectile = ProjectilePrefab.Instantiate<Area3D>();
        projectile.BodyEntered += OnBodyEntered;
        projectile.BodyExited += OnBodyExited;
        GetTree().CurrentScene.AddChild(projectile);
        projectile.GlobalPosition = _gameManager.GetRandomPosAroundPlayer(ProjectileRange) + new Vector3(0, 0.1f, 0);

        var tweener = GetTree().CreateTween();
        tweener.TweenProperty(projectile.GetNode("Visual"), "scale", new Vector3(0.01f, 0.01f, 0.01f), 1).SetDelay(TotalDuration);
        tweener.Parallel().TweenCallback(Callable.From(() => projectile.SetPhysicsProcess(false))).SetDelay(TotalDuration);
        tweener.TweenCallback(Callable.From(projectile.QueueFree));
    }

    private void OnBodyExited(Node3D body)
    {
        if (body is not Enemy enemy) return;
        _enemies.Remove(enemy);
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is not Enemy enemy) return;
        if (!enemy.IsDead)
        {
            _enemies.Add(enemy);
        }
    }

    private void OnDamageReady()
    {
        if (!_unlocked) return;
        _damageCooldown.Start();
        _enemies.RemoveWhere(enemy => !GodotObject.IsInstanceValid(enemy) || enemy.IsDead);
        foreach (var enemy in _enemies)
            enemy.TakeDamages(TotalDamages);
    }

    public void Upgrade(PowerupType powerupType)
    {
        switch (powerupType)
        {
            case PowerupType.SpiritWaterDamages: _damagesBonus += 1; break;
            case PowerupType.SpiritWaterDuration: _durationBonus += 0.2f; break;
            case PowerupType.SpiritWaterCooldown: _cooldownBonus += 0.25f; break;
            default: break;
        }
    }

    public bool TryApplyTemporaryUpgrade(TemporaryUpgradeDefinition upgrade)
    {
        if (upgrade == null || upgrade.Amount <= 0 || !float.IsFinite(upgrade.Amount)) return false;
        if (upgrade.Effect == TemporaryUpgradeEffect.UnlockSpiritWater) return Unlock();
        if (!_unlocked) return false;

        switch (upgrade.Effect)
        {
            case TemporaryUpgradeEffect.SpiritWaterDamage:
                _damagesBonus += (uint)Mathf.Max(1, Mathf.FloorToInt(upgrade.Amount));
                return true;
            case TemporaryUpgradeEffect.SpiritWaterDuration:
                _durationBonus += upgrade.Amount;
                return true;
            case TemporaryUpgradeEffect.SpiritWaterCooldown:
                _cooldownBonus += upgrade.Amount;
                _projectileCooldown.WaitTime = TotalCooldown;
                return true;
            default:
                return false;
        }
    }
}
