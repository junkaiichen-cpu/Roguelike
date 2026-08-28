using Godot;
using System;

public enum EnemyClass
{
    Minion = 0,
    Warrior = 1,
    Archer = 2,
    Mage = 3,

    Boss = 100
}

public enum EliteType
{
    None,
    Charger,
    Tank,
    Archer,
    Mage,
    Swift,
}

public partial class Enemy : RigidBody3D
{
    private const int MaxVisibleHealthBars = 48;
    private static int _visibleHealthBarCount;
    public const uint DefaultMaxHealth = 10;
    private const double HitFeedbackDurationSeconds = 0.1d;
    private const double SpawnFeedbackDurationSeconds = 0.08d;
    private const double DeathFeedbackDurationSeconds = 0.08d;
    private const double SpawnProtectionDurationSeconds = 0.25d;

    [Export]
    public uint MaxHealth { get; set; } = DefaultMaxHealth;

    [Export]
    public uint Damages { get; set; } = 5;

    [Export]
    public float MovementSpeed { get; set; } = 4;

    [Export]
    public float ContactDamageRange { get; private set; } = 2f;

    [Export(PropertyHint.Range, "1,100000,1")]
    public uint ExperienceReward { get; private set; } = 1;

    private EnemyRuntimeState _runtimeState = new(DefaultMaxHealth);
    private GameManager _gameManager;
    private GpuParticles3D _damageParticles;

    private ProgressBar _lifebar;
    private Camera3D _camera;
    private Label _username;
    private Timer _attackCooldown;
    private MeshInstance3D _hitFlash;
    private Timer _hitFlashTimer;
    private double _spawnProtectionRemaining;
    private ProgressBar _healthBar;
    private double _healthBarRemaining;
    private EliteType _eliteType;
    private float _damageMultiplier = 1f;
    private PackedScene _rangedProjectileScene;
    private Timer _rangedTelegraphTimer;
    private MeshInstance3D _rangedTelegraph;
    private Vector3 _rangedTarget;

    [Signal]
    public delegate void OnEnemyHitEventHandler(Enemy enemy, int damages);

    public uint CurrentHealth => _runtimeState.CurrentHealth;

    public bool IsDead => _runtimeState.IsDead;

    public event Action<Enemy> Died;

    public bool IsElite => _eliteType != EliteType.None;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _runtimeState = new(MaxHealth);
        _gameManager = GetNode<GameManager>("/root/GameManager");

        _attackCooldown = GetNode<Timer>("AttackCooldown");
        _damageParticles = GetNode<GpuParticles3D>("DamageParticles");

        _camera = GetNode<Camera3D>("../Player/Camera3D");
        DisableVisualShadows(this);
        ApplyEliteVisual();
        _rangedProjectileScene = GD.Load<PackedScene>("res://Prefabs/Enemies/enemy_attack_projectile.tscn");
        if (_eliteType == EliteType.Mage)
        {
            _rangedTelegraph = new MeshInstance3D
            {
                Visible = false,
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
                Mesh = new CylinderMesh { TopRadius = 1.4f, BottomRadius = 1.4f, Height = 0.04f },
                MaterialOverride = new StandardMaterial3D
                {
                    ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                    AlbedoColor = new Color(0.8f, 0.2f, 1f, 0.45f),
                    Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                    EmissionEnabled = true,
                    Emission = new Color(0.6f, 0.05f, 1f),
                },
            };
            AddChild(_rangedTelegraph);
            _rangedTelegraphTimer = new Timer { OneShot = true, WaitTime = 0.6f };
            _rangedTelegraphTimer.Timeout += FireRangedAttack;
            AddChild(_rangedTelegraphTimer);
        }
        // var hud = GetNode<Control>("../HUD");
        // _lifebar = GD.Load<PackedScene>("res://Prefabs/UI/enemy_life_bar.tscn").Instantiate<ProgressBar>();
        // _lifebar.MaxValue = MaxHealth;
        // hud.AddChild(_lifebar);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);

        if (IsDead || _gameManager.Player == null || _gameManager.Player.IsDead)
        {
            return;
        }

        _spawnProtectionRemaining = Math.Max(0, _spawnProtectionRemaining - delta);
        if (_healthBar != null)
        {
            _healthBarRemaining -= delta;
            _healthBar.Visible = _healthBarRemaining > 0;
            _healthBar.Modulate = _eliteType == EliteType.Tank
                && Mathf.PosMod(Time.GetTicksMsec() / 1000f, 3f) < 0.45f
                ? new Color(0.45f, 0.75f, 1f)
                : Colors.White;
            Vector2 healthBarPosition = _camera.UnprojectPosition(GlobalPosition);
            healthBarPosition -= new Vector2(32, 24);
            _healthBar.Position = healthBarPosition;
        }

        if (_attackCooldown.TimeLeft <= 0) Attack();

        LookAt(_gameManager.Player.GlobalPosition, Vector3.Up, true);

        if (_lifebar != null)
        {
            Vector2 lifeBarPos = _camera.UnprojectPosition(GlobalPosition);
            lifeBarPos -= _lifebar.Size / 2;
            lifeBarPos.Y -= 50;
            _lifebar.Position = lifeBarPos;
            _lifebar.Value = CurrentHealth;
        }

        if (_username == null) return;

        Vector2 usernamePos = _camera.UnprojectPosition(GlobalPosition);
        usernamePos -= _username.Size / 2;
        usernamePos.Y -= 40;
        _username.Position = usernamePos;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (IsDead || _gameManager.Player == null || _gameManager.Player.IsDead)
        {
            LinearVelocity = Vector3.Zero;
            return;
        }

        Vector3 toPlayer = _gameManager.Player.GlobalPosition - GlobalPosition;
        if (_eliteType is EliteType.Archer or EliteType.Mage)
        {
            LinearVelocity = GetRangedMovement(toPlayer);
            return;
        }

        LinearVelocity = ToGodotVector3(EnemyPursuit.CalculateVelocity(
            ToNumericsVector3(GlobalPosition),
            ToNumericsVector3(_gameManager.Player.GlobalPosition),
            GetEffectiveMovementSpeed()));

    }

    public override void _ExitTree()
    {
        base._ExitTree();

        _lifebar?.QueueFree();
        _healthBar?.QueueFree();
        if (_healthBar != null) _visibleHealthBarCount = Math.Max(0, _visibleHealthBarCount - 1);
        _username?.QueueFree();
    }

    private void Attack()
    {
        if (IsDead || _gameManager.Player == null || _gameManager.Player.IsDead) return;
        if (_spawnProtectionRemaining > 0) return;
        float attackRange = _eliteType == EliteType.Archer ? 14f : ContactDamageRange;
        if (_eliteType == EliteType.Mage) attackRange = 16f;
        if ((_gameManager.Player.GlobalPosition - GlobalPosition).Length() > attackRange) return;

        _attackCooldown.Start();
        if (_eliteType is EliteType.Archer or EliteType.Mage)
        {
            _rangedTarget = _gameManager.Player.GlobalPosition;
            if (_eliteType == EliteType.Mage)
            {
                _rangedTelegraph.GlobalPosition = _rangedTarget + new Vector3(0, 0.03f, 0);
                _rangedTelegraph.Visible = true;
                _rangedTelegraphTimer.Start();
            }
            else
            {
                FireRangedAttack();
            }
            return;
        }

        _gameManager.Player.TakeDamages(Damages);
    }

    private void FireRangedAttack()
    {
        if (_rangedTelegraph != null) _rangedTelegraph.Visible = false;
        if (_rangedProjectileScene == null || _gameManager.Player == null || _gameManager.Player.IsDead) return;

        EnemyAttackProjectile projectile = _rangedProjectileScene.Instantiate<EnemyAttackProjectile>();
        GetTree().CurrentScene.AddChild(projectile);
        projectile.GlobalPosition = GlobalPosition + new Vector3(0, 1f, 0);
        projectile.Configure(_rangedTarget, 8f, Damages);
    }

    internal void SetName(string username)
    {
        _username = new() { Text = username };
        GetNode<Control>("../HUD").AddChild(_username);
    }

    internal void ConfigureElite(EliteType eliteType)
    {
        _eliteType = eliteType;
        switch (eliteType)
        {
            case EliteType.Charger:
                MovementSpeed *= 1.75f;
                MaxHealth = (uint)Math.Clamp((long)MaxHealth * 2, 1L, uint.MaxValue);
                Scale *= 1.65f;
                break;
            case EliteType.Tank:
                MovementSpeed *= 0.65f;
                MaxHealth = (uint)Math.Clamp((long)MaxHealth * 3, 1L, uint.MaxValue);
                _damageMultiplier = 0.8f;
                Scale *= 1.9f;
                break;
            case EliteType.Archer:
                MovementSpeed *= 0.9f;
                MaxHealth = (uint)Math.Clamp((long)MaxHealth * 2, 1L, uint.MaxValue);
                Scale *= 1.6f;
                break;
            case EliteType.Mage:
                MovementSpeed *= 0.75f;
                MaxHealth = (uint)Math.Clamp((long)MaxHealth * 2, 1L, uint.MaxValue);
                ContactDamageRange = 6f;
                Scale *= 1.65f;
                break;
            case EliteType.Swift:
                MovementSpeed *= 0.85f;
                MaxHealth = (uint)Math.Clamp((long)MaxHealth * 2, 1L, uint.MaxValue);
                Scale *= 1.8f;
                break;
        }
    }

    internal void TakeDamages(uint damages = 1)
    {
        if (_eliteType == EliteType.Tank && Mathf.PosMod(Time.GetTicksMsec() / 1000f, 3f) < 0.45f) return;
        uint effectiveDamage = (uint)Mathf.Max(1, Mathf.RoundToInt(damages * _damageMultiplier));
        uint appliedDamage = _runtimeState.ApplyDamage(effectiveDamage);
        if (appliedDamage == 0) return;

        ShowHealthBar();
        ShowHitFeedback();

        EmitSignal(SignalName.OnEnemyHit, this, ToDisplayDamage(appliedDamage));
        if (!IsDead) return;

        Die();
    }

    internal void PlayLightningHitFeedback()
    {
        if (IsDead) return;
        ShowHitFeedback();
        Scale = Scale * 1.08f;
        CreateTween().TweenProperty(this, "scale", Scale / 1.08f, 0.1f);
    }

    internal void PlaySpawnFeedback()
    {
        Vector3 targetScale = Scale;
        Scale = targetScale * 0.72f;
        _spawnProtectionRemaining = SpawnProtectionDurationSeconds;

        Tween spawnTween = CreateTween();
        spawnTween.TweenProperty(this, "scale", targetScale, SpawnFeedbackDurationSeconds)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }

    private void Die()
    {
        LinearVelocity = Vector3.Zero;
        _attackCooldown.Stop();
        SetProcess(false);
        SetPhysicsProcess(false);
        Died?.Invoke(this);

        Tween deathTween = CreateTween();
        deathTween.TweenProperty(this, "scale", Scale * 1.2f, 0.08f);
        deathTween.TweenProperty(this, "scale", Vector3.Zero, 0.14f);
        deathTween.TweenCallback(Callable.From(QueueFree));
    }

    internal void EmitParticles()
    {
        if (_damageParticles == null) return;

        _damageParticles.Restart();
        _damageParticles.Emitting = true;
    }

    private void ShowHealthBar()
    {
        if (_healthBar == null)
        {
            if (_visibleHealthBarCount >= MaxVisibleHealthBars) return;
            _healthBar = new ProgressBar
            {
                MaxValue = MaxHealth,
                ShowPercentage = false,
                Size = new Vector2(64, 6),
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            _healthBar.AddThemeStyleboxOverride("background", new StyleBoxFlat
            {
                BgColor = new Color(0.08f, 0.015f, 0.015f, 0.9f),
            });
            _healthBar.AddThemeStyleboxOverride("fill", new StyleBoxFlat
            {
                BgColor = new Color(0.68f, 0.035f, 0.045f, 1f),
            });
            GetNode<Control>("../HUD").AddChild(_healthBar);
            _visibleHealthBarCount++;
        }

        _healthBar.Value = CurrentHealth;
        _healthBarRemaining = 1;
        _healthBar.Visible = true;
    }

    private static void DisableVisualShadows(Node node)
    {
        if (node is GeometryInstance3D geometry)
        {
            geometry.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        }

        foreach (Node child in node.GetChildren())
        {
            DisableVisualShadows(child);
        }
    }

    private float GetEffectiveMovementSpeed()
    {
        if (_eliteType == EliteType.Swift)
        {
            float swiftPhase = Mathf.PosMod(Time.GetTicksMsec() / 1000f, 2.5f);
            return swiftPhase < 0.35f ? 0f : MovementSpeed * 2.8f;
        }

        if (_eliteType != EliteType.Charger) return MovementSpeed;

        float phase = Mathf.PosMod(Time.GetTicksMsec() / 1000f, 3f);
        return phase < 0.5f ? MovementSpeed * 2f : MovementSpeed;
    }

    private void ApplyEliteVisual()
    {
        if (_eliteType == EliteType.None) return;

        Color tint = _eliteType switch
        {
            EliteType.Charger => new Color(1f, 0.45f, 0.25f),
            EliteType.Tank => new Color(0.55f, 0.7f, 1f),
            EliteType.Archer => new Color(0.35f, 1f, 0.55f),
            EliteType.Mage => new Color(0.85f, 0.35f, 1f),
            _ => new Color(1f, 0.9f, 0.2f),
        };
        ApplyTint(this, tint);
    }

    private Vector3 GetRangedMovement(Vector3 toPlayer)
    {
        float distance = toPlayer.Length();
        if (distance < 8f)
        {
            return -toPlayer.Normalized() * GetEffectiveMovementSpeed();
        }

        if (distance > 12f)
        {
            return toPlayer.Normalized() * GetEffectiveMovementSpeed();
        }

        return Vector3.Zero;
    }

    private static void ApplyTint(Node node, Color tint)
    {
        if (node is MeshInstance3D mesh && mesh.GetActiveMaterial(0) is BaseMaterial3D material)
        {
            BaseMaterial3D materialOverride = material.Duplicate() as BaseMaterial3D;
            materialOverride.AlbedoColor = material.AlbedoColor.Lerp(tint, 0.35f);
            mesh.MaterialOverride = materialOverride;
        }

        foreach (Node child in node.GetChildren())
        {
            ApplyTint(child, tint);
        }
    }

    private void ShowHitFeedback()
    {
        if (_hitFlash == null)
        {
            _hitFlash = new MeshInstance3D
            {
                Name = "HitFlash",
                Position = new Vector3(0f, 0.75f, 0f),
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
                Mesh = new SphereMesh
                {
                    Radius = 0.85f,
                    Height = 1.7f,
                },
                MaterialOverride = new StandardMaterial3D
                {
                    Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                    ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                    AlbedoColor = new Color(1f, 0.85f, 0.15f, 0.6f),
                    EmissionEnabled = true,
                    Emission = new Color(1f, 0.55f, 0.05f),
                },
            };
            AddChild(_hitFlash);

            _hitFlashTimer = new Timer
            {
                Name = "HitFlashTimer",
                OneShot = true,
                WaitTime = HitFeedbackDurationSeconds,
            };
            _hitFlashTimer.Timeout += HideHitFeedback;
            AddChild(_hitFlashTimer);
        }

        _hitFlash.Visible = true;
        _hitFlashTimer.Start();
    }

    private void HideHitFeedback()
    {
        if (_hitFlash != null)
        {
            _hitFlash.Visible = false;
        }
    }

    private static Vector3 ToGodotVector3(System.Numerics.Vector3 vector) => new(vector.X, vector.Y, vector.Z);

    private static System.Numerics.Vector3 ToNumericsVector3(Vector3 vector) => new(vector.X, vector.Y, vector.Z);

    private static int ToDisplayDamage(uint damage) => damage > int.MaxValue ? int.MaxValue : (int)damage;
}
