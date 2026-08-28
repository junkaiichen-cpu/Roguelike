using Godot;
using System;

public partial class Player : CharacterBody3D, ITemporaryUpgradeReceiver
{
    public const uint DefaultMaxHealth = 200;

    [Export]
    public float Speed { get; private set; } = 5.0f;

    [Export]
    public uint MaxHealth { get; private set; } = DefaultMaxHealth;

    [Export]
    public float JumpVelocity { get; private set; } = 4.5f;

    [Export(PropertyHint.Range, "1,100000,1")]
    public uint ExperienceRequiredForFirstLevel { get; private set; } = 5;

    [Export(PropertyHint.Range, "1,100000,1")]
    public uint ExperienceRequirementIncreasePerLevel { get; private set; } = 5;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    private PlayerRuntimeState _runtimeState = new(DefaultMaxHealth);
    private PlayerProgressionState _progressionState = new(1, 5, 5);
    private GameManager _gameManager;
    private Timer _attackTimer;
    private Node3D _visual;
    private ProgressBar _playerLifebar;
    private AnimationTree _animationTree;
    private ColorRect _damageFlash;
    private float _moveSpeedMultiplier = 1f;
    private float _pickupRadiusMultiplier = 1f;
    private float _experienceGainMultiplier = 1f;

    public float TotalMoveSpeed => Speed * _moveSpeedMultiplier;

    public float TotalPickupRadius => 4.5f * _pickupRadiusMultiplier;

    public float PassiveDamageMultiplier { get; private set; } = 1f;
    public float PassiveCooldownMultiplier { get; private set; } = 1f;
    public float PassiveAreaMultiplier { get; private set; } = 1f;
    public float PassiveProjectileSizeMultiplier { get; private set; } = 1f;
    public float PermanentDamageMultiplier { get; private set; } = 1f;
    public float PermanentExperienceMultiplier { get; private set; } = 1f;
    public float PermanentLuckMultiplier { get; private set; } = 1f;

    public bool ActivateWeapon(WeaponPickupType weaponType)
    {
        bool activated = weaponType switch
        {
            WeaponPickupType.Orb => GetNodeOrNull<FloatingSphereAttack>("FloatingSphere")?.Unlock() ?? false,
            WeaponPickupType.Fire => GetNodeOrNull<GroundFireAttack>("GroundFire")?.Unlock() ?? false,
            WeaponPickupType.SpiritWater => ActivateSpiritWater(),
            WeaponPickupType.Lifesteal => ActivateLifesteal(),
            WeaponPickupType.Cross => ActivateUpgradeWeapon< CrossAttack>("CrossAttack", TemporaryUpgradeEffect.UnlockCross),
            WeaponPickupType.Lightning => ActivateUpgradeWeapon<LightningAttack>("LightningAttack", TemporaryUpgradeEffect.UnlockLightning),
            WeaponPickupType.Bible => ActivateUpgradeWeapon<BibleAttack>("BibleAttack", TemporaryUpgradeEffect.UnlockBible),
            _ => false,
        };
        if (activated) BuildChanged?.Invoke(this);
        return activated;
    }

    private bool ActivateSpiritWater()
    {
        return GetNodeOrNull<SpiritWater>("SpiritWater")?.Unlock() ?? false;
    }

    private bool ActivateLifesteal()
    {
        return GetNodeOrNull<LifestealAttack>("Lifesteal")?.Unlock() ?? false;
    }

    private bool ActivateUpgradeWeapon<TWeapon>(string nodePath, TemporaryUpgradeEffect effect)
        where TWeapon : Node, ITemporaryUpgradeReceiver
    {
        TWeapon weapon = GetNodeOrNull<TWeapon>(nodePath);
        bool activated = weapon?.TryApplyTemporaryUpgrade(new TemporaryUpgradeDefinition
        {
            Id = $"pickup_{nodePath}",
            Effect = effect,
            Amount = 1,
        }) ?? false;
        if (activated) BuildChanged?.Invoke(this);
        return activated;
    }

    public uint CurrentHealth => _runtimeState.CurrentHealth;

    public bool IsDead => _runtimeState.IsDead;

    public uint CurrentExperience => _progressionState.CurrentExperience;

    public uint CurrentLevel => _progressionState.CurrentLevel;

    public uint ExperienceRequiredForNextLevel => _progressionState.ExperienceRequiredForNextLevel;

    public event Action<Player> Died;

    public event Action<Player> ExperienceChanged;

    public event Action<Player> HealthChanged;

    public event Action<Player, uint> LeveledUp;

    public event Action<Player> BuildChanged;

    public override void _Ready()
    {
        base._Ready();

        _gameManager = GetNode<GameManager>("/root/GameManager");
        ApplyCharacterDefinition(_gameManager.SelectedCharacter);
        _runtimeState = new(MaxHealth);
        _progressionState = new(
            initialLevel: 1,
            initialExperienceRequired: ExperienceRequiredForFirstLevel,
            experienceRequirementIncreasePerLevel: ExperienceRequirementIncreasePerLevel);

        _gameManager.Player = this;
        if (_gameManager.SelectedCharacter?.StartingPassiveId == "passive_max_health")
        {
            _runtimeState.IncreaseMaxHealth(10);
            MaxHealth = _runtimeState.MaxHealth;
        }
        ApplyMetaProgression(_gameManager.MetaProgression);
        _playerLifebar = GetTree().CurrentScene.GetNode<ProgressBar>("HUD/PlayerLifeBar");
        _playerLifebar.MaxValue = MaxHealth;
        UpdateHealthBar();

        _attackTimer = GetNode<Timer>("AttackCooldown");
        _visual = GetNode<Node3D>("Visual");
        _animationTree = GetNode<AnimationTree>("AnimationTree");
        _damageFlash = GetTree().CurrentScene.GetNodeOrNull<ColorRect>("HUD/PlayerDamageFlash");
    }

    private void ApplyMetaProgression(MetaProgressionState meta)
    {
        if (meta == null) return;
        _runtimeState.IncreaseMaxHealth((uint)(meta.GetUpgradeLevel("max_health") * 10));
        MaxHealth = _runtimeState.MaxHealth;
        _moveSpeedMultiplier *= 1f + meta.GetUpgradeLevel("move_speed") * 0.03f;
        _pickupRadiusMultiplier *= 1f + meta.GetUpgradeLevel("pickup_range") * 0.05f;
        _experienceGainMultiplier *= 1f + meta.GetUpgradeLevel("xp_gain") * 0.05f;
        PermanentDamageMultiplier += meta.GetUpgradeLevel("damage") * 0.05f;
        PermanentLuckMultiplier += meta.GetUpgradeLevel("luck") * 0.05f;
        PermanentExperienceMultiplier = _experienceGainMultiplier;
    }

    private void ApplyCharacterDefinition(CharacterDefinition character)
    {
        if (character == null) return;
        MaxHealth = character.BaseHP;
        _moveSpeedMultiplier *= character.BaseMoveSpeedMultiplier;
        _pickupRadiusMultiplier *= character.BasePickupRangeMultiplier;
        _experienceGainMultiplier *= character.BaseXPGainMultiplier;
        PermanentDamageMultiplier *= character.BaseDamageMultiplier;
        PermanentLuckMultiplier *= character.BaseLuckMultiplier;
        if (character.StartingPassiveId == "passive_xp_gain") _experienceGainMultiplier *= 1.1f;
        if (character.StartingPassiveId == "passive_pickup_range") _pickupRadiusMultiplier *= 1.1f;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsDead)
        {
            Velocity = Vector3.Zero;
            return;
        }

        Vector3 velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
            velocity.Y -= gravity * (float)delta;

        // Handle Jump.
        //if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
        //    velocity.Y = JumpVelocity;

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * TotalMoveSpeed;
            velocity.Z = direction.Z * TotalMoveSpeed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, TotalMoveSpeed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, TotalMoveSpeed);
        }

        Velocity = velocity;
        MoveAndSlide();

        velocity.Y = 0;
        _animationTree.Set("parameters/walking/blend_amount", velocity.LimitLength(1).Length());
        if (Mathf.IsZeroApprox(velocity.Length())) return;

        _visual.LookAt(Position + 10 * velocity, Vector3.Up, true);
        //var angle = 0;
        //var transform = _visual.Transform;
        //transform.Basis = new(Vector3.Up, angle);
        //_visual.Transform = transform;
    }

    public void OnAttackTimeOut()
    {
        if (IsDead) return;

        _attackTimer.Start();

        var nearestEnemy = _gameManager.GetNearestEnemy();
        if (nearestEnemy == null) return;

        //nearestEnemy.TakeDamages();
    }

    public void TakeDamages(uint damages)
    {
        if (IsDead) return;

        uint appliedDamage = _runtimeState.ApplyDamage(damages);
        UpdateHealthBar();
        if (appliedDamage > 0) HealthChanged?.Invoke(this);
        if (appliedDamage > 0)
        {
            ShowDamageFeedback();
        }

        if (IsDead)
        {
            Die();
        }
    }

    internal void Heal(uint health)
    {
        _runtimeState.RestoreHealth(health);
        UpdateHealthBar();
        HealthChanged?.Invoke(this);
    }

    public bool CollectExperience(uint experience)
    {
        if (IsDead || experience == 0) return false;

        uint adjustedExperience = (uint)Mathf.Clamp(
            Mathf.RoundToInt(experience * _experienceGainMultiplier),
            1,
            int.MaxValue);
        PlayerProgressionResult result = _progressionState.AddExperience(adjustedExperience);
        ExperienceChanged?.Invoke(this);

        if (result.HasLeveledUp)
        {
            LeveledUp?.Invoke(this, result.LevelsGained);
        }

        return true;
    }

    private void Die()
    {
        Velocity = Vector3.Zero;
        _attackTimer.Stop();
        _animationTree.Set("parameters/walking/blend_amount", 0f);
        SetPhysicsProcess(false);
        Died?.Invoke(this);
    }

    private void UpdateHealthBar()
    {
        _playerLifebar.Value = CurrentHealth;
    }

    private void ShowDamageFeedback()
    {
        if (_damageFlash == null) return;

        _damageFlash.Color = new Color(0.9f, 0.08f, 0.04f, 0.3f);
        Tween flashTween = CreateTween();
        flashTween.TweenProperty(_damageFlash, "color", new Color(0.9f, 0.08f, 0.04f, 0f), 0.18f);
    }

    public bool TryApplyTemporaryUpgrade(TemporaryUpgradeDefinition upgrade)
    {
        if (upgrade == null || upgrade.Amount <= 0 || !float.IsFinite(upgrade.Amount)) return false;

        switch (upgrade.Effect)
        {
            case TemporaryUpgradeEffect.PassiveMaxHealth:
                _runtimeState.IncreaseMaxHealth((uint)Mathf.Max(1, Mathf.FloorToInt(upgrade.Amount)));
                MaxHealth = _runtimeState.MaxHealth;
                UpdateHealthBar();
                HealthChanged?.Invoke(this);
                return true;
            case TemporaryUpgradeEffect.PassiveMoveSpeed:
                _moveSpeedMultiplier *= 1f + upgrade.Amount / 100f;
                return true;
            case TemporaryUpgradeEffect.PassiveExperienceGain:
                _experienceGainMultiplier *= 1f + upgrade.Amount / 100f;
                return true;
            case TemporaryUpgradeEffect.PassivePickupRange:
                _pickupRadiusMultiplier *= 1f + upgrade.Amount / 100f;
                return true;
            case TemporaryUpgradeEffect.PassiveDamage:
                PassiveDamageMultiplier *= 1f + upgrade.Amount / 100f;
                return true;
            case TemporaryUpgradeEffect.PassiveCooldown:
                PassiveCooldownMultiplier *= 1f + upgrade.Amount / 100f;
                return true;
            case TemporaryUpgradeEffect.PassiveArea:
                PassiveAreaMultiplier *= 1f + upgrade.Amount / 100f;
                return true;
            case TemporaryUpgradeEffect.PassiveProjectileSize:
                PassiveProjectileSizeMultiplier *= 1f + upgrade.Amount / 100f;
                return true;
            case TemporaryUpgradeEffect.PickupRadiusPercent:
                _pickupRadiusMultiplier *= 1f + upgrade.Amount / 100f;
                return true;
            case TemporaryUpgradeEffect.ExperienceGainPercent:
                _experienceGainMultiplier *= 1f + upgrade.Amount / 100f;
                return true;
            case TemporaryUpgradeEffect.MoveSpeedPercent:
                _moveSpeedMultiplier *= 1f + upgrade.Amount / 100f;
                return true;
            default:
                return false;
        }
    }
}
