using Godot;

public enum RunEventType
{
    XpRain,
    EliteHorde,
    FaithSurge,
    HolyGround,
    AngelBlessing,
    DemonWave,
}

[GlobalClass]
public partial class EventDefinition : Resource
{
    [Export] public string Id { get; set; } = string.Empty;
    [Export] public string DisplayName { get; set; } = string.Empty;
    [Export] public RunEventType Type { get; set; }
    [Export] public float DurationSeconds { get; set; } = 12f;
    [Export] public int Intensity { get; set; } = 1;
}
