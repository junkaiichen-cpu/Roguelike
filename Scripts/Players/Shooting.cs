

using Godot;

public partial class Shooting : Node3D, IUpgradable, ITemporaryUpgradeReceiver
{
    [Export]
    public ProjectileWeaponDefinition Definition { get; set; }

    private uint _damagesBonus = 0;

    private float _attackSpeedBonus = 0;

    public float TotalAttackSpeed => Definition.AttacksPerSecond + _attackSpeedBonus;

    private float _bulletSpeedBonus = 0;

    public float TotalBulletSpeed => Definition.ProjectileSpeed + _bulletSpeedBonus;

    private GameManager _gameManager;
    private Player _player;

    private Timer _timer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");
        if (Definition == null || Definition.ProjectileScene == null || Definition.BaseDamage == 0 ||
            Definition.AttacksPerSecond <= 0 || Definition.ProjectileSpeed <= 0)
        {
            GD.PushError($"{Name} requires a valid projectile weapon definition and projectile scene.");
            _timer.Stop();
            return;
        }

        _gameManager = GetNode<GameManager>("/root/GameManager");
        _player = GetParent<Player>();
        _player.Died += StopAttacking;

        _timer.WaitTime = ProjectileWeaponTiming.GetCooldownSeconds(TotalAttackSpeed);
        _timer.Timeout += Shoot;
        _timer.Start();
    }

    public override void _ExitTree()
    {
        if (_player != null)
        {
            _player.Died -= StopAttacking;
        }

        base._ExitTree();
    }

    private void Shoot()
    {
        if (_player.IsDead) return;

        var nearestEnemy = _gameManager.GetNearestEnemy();
        if (nearestEnemy == null || nearestEnemy.IsDead) return;

        var bullet = Definition.ProjectileScene.Instantiate<RigidBody3D>();
        bullet.LinearVelocity = TotalBulletSpeed * (nearestEnemy.GlobalPosition - GlobalPosition).Normalized();
        bullet.BodyEntered += (body) => OnBodyEntered(bullet, body);
        GetTree().CurrentScene.AddChild(bullet);
        bullet.GlobalPosition = GlobalPosition + new Vector3(0, 0.5f, 0);
    }

    private void OnBodyEntered(RigidBody3D bullet, Node body)
    {
        bullet.QueueFree();
        if (body is not Enemy enemy) return;
        enemy.TakeDamages(Definition.BaseDamage + _damagesBonus);
    }

    private void StopAttacking(Player player)
    {
        _timer.Stop();
    }

    public bool TryApplyTemporaryUpgrade(TemporaryUpgradeDefinition upgrade)
    {
        if (upgrade == null || upgrade.Amount <= 0 || !float.IsFinite(upgrade.Amount)) return false;

        switch (upgrade.Effect)
        {
            case TemporaryUpgradeEffect.ProjectileDamage:
                uint damageIncrease = (uint)Mathf.FloorToInt(upgrade.Amount);
                if (damageIncrease == 0) return false;

                _damagesBonus = uint.MaxValue - _damagesBonus < damageIncrease
                    ? uint.MaxValue
                    : _damagesBonus + damageIncrease;
                return true;
            case TemporaryUpgradeEffect.ProjectileAttackSpeed:
                _attackSpeedBonus += upgrade.Amount;
                _timer.WaitTime = ProjectileWeaponTiming.GetCooldownSeconds(TotalAttackSpeed);
                return true;
            case TemporaryUpgradeEffect.ProjectileSpeed:
                _bulletSpeedBonus += upgrade.Amount;
                return true;
            default:
                return false;
        }
    }

    public void Upgrade(PowerupType powerupType)
    {
        switch (powerupType)
        {
            case PowerupType.ShootingDamages:
                _damagesBonus += 1;
                break;
            case PowerupType.ShootingAttackSpeed:
                _attackSpeedBonus += 0.1f;
                _timer.WaitTime = ProjectileWeaponTiming.GetCooldownSeconds(TotalAttackSpeed);
                break;
            case PowerupType.ShootingBulletSpeed:
                _bulletSpeedBonus += 0.1f;
                break;
            default: break;
        }
    }
}
