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

public partial class Enemy : RigidBody3D
{
    public const uint DefaultMaxHealth = 10;
    private const double HitFeedbackDurationSeconds = 0.1d;

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

    [Signal]
    public delegate void OnEnemyHitEventHandler(Enemy enemy, int damages);

    public uint CurrentHealth => _runtimeState.CurrentHealth;

    public bool IsDead => _runtimeState.IsDead;

    public event Action<Enemy> Died;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _runtimeState = new(MaxHealth);
        _gameManager = GetNode<GameManager>("/root/GameManager");

        _attackCooldown = GetNode<Timer>("AttackCooldown");
        _damageParticles = GetNode<GpuParticles3D>("DamageParticles");

        _camera = GetNode<Camera3D>("../Player/Camera3D");

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

        LinearVelocity = ToGodotVector3(EnemyPursuit.CalculateVelocity(
            ToNumericsVector3(GlobalPosition),
            ToNumericsVector3(_gameManager.Player.GlobalPosition),
            MovementSpeed));
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        _lifebar?.QueueFree();
        _username?.QueueFree();
    }

    private void Attack()
    {
        if (IsDead || _gameManager.Player == null || _gameManager.Player.IsDead) return;
        if ((_gameManager.Player.GlobalPosition - GlobalPosition).Length() > ContactDamageRange) return;

        _attackCooldown.Start();
        _gameManager.Player.TakeDamages(Damages);
    }

    internal void SetName(string username)
    {
        _username = new() { Text = username };
        GetNode<Control>("../HUD").AddChild(_username);
    }

    internal void TakeDamages(uint damages = 1)
    {
        uint appliedDamage = _runtimeState.ApplyDamage(damages);
        if (appliedDamage == 0) return;

        EmitParticles();
        ShowHitFeedback();

        EmitSignal(SignalName.OnEnemyHit, this, ToDisplayDamage(appliedDamage));
        if (!IsDead) return;

        Die();
    }

    private void Die()
    {
        LinearVelocity = Vector3.Zero;
        _attackCooldown.Stop();
        SetProcess(false);
        SetPhysicsProcess(false);
        Died?.Invoke(this);
        QueueFree();
    }

    internal void EmitParticles()
    {
        if (_damageParticles == null) return;

        _damageParticles.Restart();
        _damageParticles.Emitting = true;
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
