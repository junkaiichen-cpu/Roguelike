using Godot;
using System;

public partial class ExperiencePickup : Area3D
{
    private const double AttractionDurationSeconds = 0.15d;
    private const double VacuumDurationSeconds = 0.35d;

    [Export(PropertyHint.Range, "1,100000,1")]
    public uint ExperienceValue { get; set; } = 1;

    [Export(PropertyHint.Range, "0.5,20,0.1")]
    public float PickupRadius { get; set; } = 4.5f;

    private bool _wasCollected;
    private bool _isVacuuming;
    private Player _attractedPlayer;
    private double _attractionElapsed;
    private double _mergePopRemaining;
    private MeshInstance3D _mesh;
    private Vector3 _baseMeshScale;

    public void AddExperience(uint experience)
    {
        ExperienceValue = uint.MaxValue - ExperienceValue < experience
            ? uint.MaxValue
            : ExperienceValue + experience;
        UpdateVisual();
        _mergePopRemaining = 0.16d;
    }

    public void StartVacuum(Player player)
    {
        if (_wasCollected || player == null || player.IsDead) return;
        _wasCollected = true;
        SetDeferred("monitoring", false);
        _attractedPlayer = player;
        _attractionElapsed = 0;
        _isVacuuming = true;
    }

    public override void _Ready()
    {
        CollisionShape3D collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
        if (collisionShape.Shape is SphereShape3D sphere)
        {
            SphereShape3D pickupShape = sphere.Duplicate() as SphereShape3D;
            pickupShape.Radius = PickupRadius;
            collisionShape.Shape = pickupShape;
        }

        _mesh = GetNode<MeshInstance3D>("MeshInstance3D");
        _baseMeshScale = _mesh.Scale;
        UpdateVisual();
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_attractedPlayer == null)
        {
            if (_mergePopRemaining <= 0) return;
            _mergePopRemaining = Math.Max(0, _mergePopRemaining - delta);
            float pop = 1f + 0.18f * (float)(_mergePopRemaining / 0.16d);
            _mesh.Scale = _baseMeshScale * GetValueScale() * pop;
            return;
        }

        double attractionDuration = _isVacuuming ? VacuumDurationSeconds : AttractionDurationSeconds;
        _attractionElapsed += delta;
        float progress = Mathf.Clamp((float)(_attractionElapsed / attractionDuration), 0f, 1f);
        float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
        GlobalPosition = GlobalPosition.Lerp(_attractedPlayer.GlobalPosition, easedProgress);
        _mesh.Rotation = new Vector3(0, (float)_attractionElapsed * 18f, 0);
        float attractionScale = Mathf.Lerp(1.1f, 0.7f, easedProgress);
        _mesh.Scale = _baseMeshScale * GetValueScale() * attractionScale;

        if (progress >= 1f)
        {
            if (!_attractedPlayer.IsDead) _attractedPlayer.CollectExperience(ExperienceValue);
            QueueFree();
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        if (_wasCollected || ExperienceValue == 0 || body is not Player player || player.IsDead) return;
        _wasCollected = true;
        SetDeferred("monitoring", false);
        _attractedPlayer = player;
        _attractionElapsed = 0;
        _isVacuuming = false;
    }

    private void UpdateVisual()
    {
        if (_mesh == null) return;
        _mesh.Scale = _baseMeshScale * GetValueScale();
        if (ExperienceValue <= 1) return;

        StandardMaterial3D material = _mesh.GetActiveMaterial(0) as StandardMaterial3D;
        if (material == null) return;
        StandardMaterial3D materialOverride = material.Duplicate() as StandardMaterial3D;
        materialOverride.AlbedoColor = GetValueColor();
        materialOverride.EmissionEnabled = true;
        materialOverride.Emission = GetValueColor() * 0.55f;
        _mesh.MaterialOverride = materialOverride;
    }

    private float GetValueScale()
    {
        return 1f + Mathf.Clamp(Mathf.Log(ExperienceValue) / Mathf.Log(5f) * 0.35f, 0f, 0.8f);
    }

    private Color GetValueColor() => ExperienceValue switch
    {
        <= 1 => new Color(0.2f, 0.75f, 1f),
        <= 5 => new Color(0.95f, 0.72f, 0.18f),
        _ => new Color(1f, 0.34f, 0.12f),
    };
}
