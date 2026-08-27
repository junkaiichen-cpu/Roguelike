using Godot;
using System;

public partial class Player : CharacterBody3D
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

    public uint CurrentHealth => _runtimeState.CurrentHealth;

    public bool IsDead => _runtimeState.IsDead;

    public uint CurrentExperience => _progressionState.CurrentExperience;

    public uint CurrentLevel => _progressionState.CurrentLevel;

    public uint ExperienceRequiredForNextLevel => _progressionState.ExperienceRequiredForNextLevel;

    public event Action<Player> Died;

    public event Action<Player> ExperienceChanged;

    public event Action<Player, uint> LeveledUp;

    public override void _Ready()
    {
        base._Ready();

        _runtimeState = new(MaxHealth);
        _progressionState = new(
            initialLevel: 1,
            initialExperienceRequired: ExperienceRequiredForFirstLevel,
            experienceRequirementIncreasePerLevel: ExperienceRequirementIncreasePerLevel);

        _gameManager = GetNode<GameManager>("/root/GameManager");
        _gameManager.Player = this;
        _playerLifebar = GetTree().CurrentScene.GetNode<ProgressBar>("HUD/PlayerLifeBar");
        _playerLifebar.MaxValue = MaxHealth;
        UpdateHealthBar();

        _attackTimer = GetNode<Timer>("AttackCooldown");
        _visual = GetNode<Node3D>("Visual");
        _animationTree = GetNode<AnimationTree>("AnimationTree");
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
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
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

        _runtimeState.ApplyDamage(damages);
        UpdateHealthBar();

        if (IsDead)
        {
            Die();
        }
    }

    internal void Heal(uint health)
    {
        _runtimeState.RestoreHealth(health);
        UpdateHealthBar();
    }

    public bool CollectExperience(uint experience)
    {
        if (IsDead || experience == 0) return false;

        PlayerProgressionResult result = _progressionState.AddExperience(experience);
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
}
