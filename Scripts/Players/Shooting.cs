

using Godot;
using System.Collections.Generic;

public partial class Shooting : Node3D, IUpgradable, ITemporaryUpgradeReceiver
{
    private const int ProjectilePoolCapacity = 64;
    [Export]
    public ProjectileWeaponDefinition Definition { get; set; }

    private uint _damagesBonus = 0;
    private float _damageMultiplier = 1f;

    private float _attackSpeedBonus = 0;
    private float _attackSpeedMultiplier = 1f;

    public float TotalAttackSpeed => (Definition.AttacksPerSecond + _attackSpeedBonus) * _attackSpeedMultiplier
        * _player?.PassiveCooldownMultiplier ?? (Definition.AttacksPerSecond + _attackSpeedBonus) * _attackSpeedMultiplier;

    private float _bulletSpeedBonus = 0;

    public float TotalBulletSpeed => Definition.ProjectileSpeed + _bulletSpeedBonus;

    private int _projectileCountBonus;
    private int _projectileCountMultiplier = 1;

    public int TotalProjectileCount => (Definition.ProjectileCount + _projectileCountBonus) * _projectileCountMultiplier;

    private float _projectileSpreadBonus;

    public float TotalSpreadDegrees => Definition.SpreadDegrees + _projectileSpreadBonus;

    private float _projectileSizeBonus;

    public float TotalProjectileSizeMultiplier => (Definition.ProjectileSizeMultiplier + _projectileSizeBonus)
        * (_player?.PassiveProjectileSizeMultiplier ?? 1f);

    private GameManager _gameManager;
    private Player _player;

    private Timer _timer;
    private readonly List<ProjectileLifetime> _projectilePool = new();

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

        Vector3 direction = (nearestEnemy.GlobalPosition - GlobalPosition).Normalized();
        for (int projectileIndex = 0; projectileIndex < TotalProjectileCount; projectileIndex++)
        {
            float spreadRatio = TotalProjectileCount == 1
                ? 0
                : (float)projectileIndex / (TotalProjectileCount - 1) - 0.5f;
            Vector3 projectileDirection = direction.Rotated(Vector3.Up, Mathf.DegToRad(spreadRatio * TotalSpreadDegrees));
            ProjectileLifetime bullet = GetProjectile();
            if (bullet == null) continue;
            bullet.Activate();
            bullet.Scale *= TotalProjectileSizeMultiplier;
            bullet.LinearVelocity = TotalBulletSpeed * projectileDirection;
            bullet.GlobalPosition = GlobalPosition + new Vector3(0, 0.5f, 0);
        }
    }

    private ProjectileLifetime GetProjectile()
    {
        foreach (ProjectileLifetime projectile in _projectilePool)
        {
            if (!projectile.IsActive) return projectile;
        }

        if (_projectilePool.Count >= ProjectilePoolCapacity) return null;

        ProjectileLifetime created = Definition.ProjectileScene.Instantiate<ProjectileLifetime>();
        GetTree().CurrentScene.AddChild(created);
        created.BodyEntered += body => OnBodyEntered(created, body);
        created.Expired += ReturnProjectile;
        _projectilePool.Add(created);
        return created;
    }

    private void ReturnProjectile(ProjectileLifetime projectile)
    {
        projectile.Deactivate();
    }

    private void OnBodyEntered(ProjectileLifetime bullet, Node body)
    {
        if (!bullet.IsActive) return;
        bullet.Deactivate();
        if (body is not Enemy enemy) return;
        uint damage = (uint)Mathf.Clamp(
            Mathf.RoundToInt((Definition.BaseDamage + _damagesBonus) * _damageMultiplier
                * (_player?.PassiveDamageMultiplier ?? 1f)
                * (_player?.PermanentDamageMultiplier ?? 1f)),
            1,
            int.MaxValue);
        enemy.TakeDamages(damage);
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
            case TemporaryUpgradeEffect.ProjectileCount:
                int projectileIncrease = Mathf.Max(1, Mathf.FloorToInt(upgrade.Amount));
                _projectileCountBonus = Mathf.Min(15, _projectileCountBonus + projectileIncrease);
                return true;
            case TemporaryUpgradeEffect.ProjectileSpread:
                _projectileSpreadBonus += upgrade.Amount;
                return true;
            case TemporaryUpgradeEffect.ProjectileSize:
                _projectileSizeBonus += upgrade.Amount;
                return true;
            case TemporaryUpgradeEffect.ProjectileCountDouble:
                _projectileCountMultiplier = Mathf.Min(4, _projectileCountMultiplier * 2);
                return true;
            case TemporaryUpgradeEffect.ProjectileDamagePercent:
                _damageMultiplier *= 1f + upgrade.Amount / 100f;
                return true;
            case TemporaryUpgradeEffect.ProjectileAttackSpeedPercent:
                _attackSpeedMultiplier *= 1f + upgrade.Amount / 100f;
                _timer.WaitTime = ProjectileWeaponTiming.GetCooldownSeconds(TotalAttackSpeed);
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
