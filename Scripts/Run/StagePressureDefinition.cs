using Godot;

[GlobalClass]
public partial class StagePressureDefinition : Resource
{
    [Export(PropertyHint.Range, "0.1,3600,0.1")]
    public float DurationSeconds { get; set; } = 60f;

    [Export(PropertyHint.Range, "0.1,100,0.1")]
    public float SpawnRateMultiplier { get; set; } = 1f;

    [Export(PropertyHint.Range, "0.1,100,0.1")]
    public float PopulationMultiplier { get; set; } = 1f;

    public StagePressureStage ToRuntimeStage() => new(
        DurationSeconds,
        SpawnRateMultiplier,
        PopulationMultiplier);
}
