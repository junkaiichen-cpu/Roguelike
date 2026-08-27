using Godot;

[GlobalClass]
public partial class StagePressureConfiguration : Resource
{
    [Export(PropertyHint.Range, "0.05,60,0.05")]
    public float BaseSpawnIntervalSeconds { get; set; } = 1f;

    [Export(PropertyHint.Range, "1,10000,1")]
    public int BaseMaxActiveEnemies { get; set; } = 20;

    [Export(PropertyHint.Range, "1,1000,0.5")]
    public float EnemySpawnMinimumDistance { get; set; } = 30f;

    [Export(PropertyHint.Range, "1,1000,0.5")]
    public float EnemySpawnMaximumDistance { get; set; } = 36f;

    [Export(PropertyHint.Range, "1,64,1")]
    public int EnemySpawnMaximumAttempts { get; set; } = 8;

    [Export]
    public Godot.Collections.Array<StagePressureDefinition> Stages { get; set; } = new();

    [Export]
    public BossDefinition CompletionBoss { get; set; }

    public RunPressureConfiguration ToRuntimeConfiguration()
    {
        var stages = new StagePressureStage[Stages.Count];
        for (int index = 0; index < Stages.Count; index++)
        {
            if (Stages[index] == null)
            {
                throw new System.InvalidOperationException("Stage pressure configuration contains a missing stage.");
            }

            stages[index] = Stages[index].ToRuntimeStage();
        }

        return new RunPressureConfiguration(BaseSpawnIntervalSeconds, BaseMaxActiveEnemies, stages);
    }

    public EnemySpawnConfiguration ToEnemySpawnConfiguration() => new(
        EnemySpawnMinimumDistance,
        EnemySpawnMaximumDistance,
        EnemySpawnMaximumAttempts);
}
