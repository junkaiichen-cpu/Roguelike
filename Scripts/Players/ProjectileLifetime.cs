using Godot;
using System;

public partial class ProjectileLifetime : RigidBody3D
{
    [Export(PropertyHint.Range, "0.1,60,0.1")]
    public double LifetimeSeconds { get; set; } = 8d;

    private ProjectileLifetimeState _lifetimeState;
    private uint _activeCollisionLayer;
    private uint _activeCollisionMask;
    private bool _isActive;

    public bool IsActive => _isActive;
    public event Action<ProjectileLifetime> Expired;

    public override void _Ready()
    {
        if (!double.IsFinite(LifetimeSeconds) || LifetimeSeconds <= 0)
        {
            GD.PushError($"{Name} requires a positive finite projectile lifetime.");
            QueueFree();
            return;
        }

        _lifetimeState = new ProjectileLifetimeState(LifetimeSeconds);
        _activeCollisionLayer = CollisionLayer;
        _activeCollisionMask = CollisionMask;
        Deactivate();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_isActive) return;
        if (_lifetimeState != null && _lifetimeState.Advance(delta))
        {
            Expired?.Invoke(this);
            Deactivate();
        }
    }

    public void Activate()
    {
        if (_lifetimeState == null) return;
        _lifetimeState = new ProjectileLifetimeState(LifetimeSeconds);
        CollisionLayer = _activeCollisionLayer;
        CollisionMask = _activeCollisionMask;
        Scale = Vector3.One;
        LinearVelocity = Vector3.Zero;
        Freeze = false;
        Show();
        _isActive = true;
    }

    public void Deactivate()
    {
        _isActive = false;
        LinearVelocity = Vector3.Zero;
        Freeze = true;
        CollisionLayer = 0;
        CollisionMask = 0;
        Hide();
    }
}
