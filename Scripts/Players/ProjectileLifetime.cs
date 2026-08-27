using Godot;

public partial class ProjectileLifetime : RigidBody3D
{
    [Export(PropertyHint.Range, "0.1,60,0.1")]
    public double LifetimeSeconds { get; set; } = 8d;

    private ProjectileLifetimeState _lifetimeState;

    public override void _Ready()
    {
        if (!double.IsFinite(LifetimeSeconds) || LifetimeSeconds <= 0)
        {
            GD.PushError($"{Name} requires a positive finite projectile lifetime.");
            QueueFree();
            return;
        }

        _lifetimeState = new ProjectileLifetimeState(LifetimeSeconds);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_lifetimeState != null && _lifetimeState.Advance(delta))
        {
            QueueFree();
        }
    }
}
