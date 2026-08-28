using Godot;

public partial class EnemyAttackProjectile : Area3D
{
    private Vector3 _direction;
    private float _speed;
    private uint _damage;
    private float _remainingLifetime;

    public void Configure(Vector3 target, float speed, uint damage)
    {
        _direction = (target - GlobalPosition).Normalized();
        _speed = speed;
        _damage = damage;
    }

    public override void _Ready()
    {
        if (GetNodeOrNull<MeshInstance3D>("Visual") is MeshInstance3D visual)
        {
            visual.MaterialOverride = new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                AlbedoColor = new Color(1f, 0.25f, 0.7f),
                EmissionEnabled = true,
                Emission = new Color(1f, 0.05f, 0.3f),
                EmissionEnergyMultiplier = 2f,
            };
        }
        BodyEntered += OnBodyEntered;
        _remainingLifetime = 2.5f;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += _direction * _speed * (float)delta;
        _remainingLifetime -= (float)delta;
        if (_remainingLifetime <= 0) QueueFree();
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is Player player && !player.IsDead)
        {
            player.TakeDamages(_damage);
            QueueFree();
        }
    }
}
