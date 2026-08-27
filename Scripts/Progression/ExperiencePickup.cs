using Godot;

public partial class ExperiencePickup : Area3D
{
    [Export(PropertyHint.Range, "1,100000,1")]
    public uint ExperienceValue { get; set; } = 1;

    private bool _wasCollected;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (_wasCollected || ExperienceValue == 0 || body is not Player player) return;
        if (!player.CollectExperience(ExperienceValue)) return;

        _wasCollected = true;
        QueueFree();
    }
}
