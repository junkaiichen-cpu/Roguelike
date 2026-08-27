using System;
using System.Collections.Generic;
using System.Linq;

public enum RunLifecycleStatus
{
    NotStarted,
    Active,
    Paused,
    Completed,
    Stopped,
}

public readonly record struct StagePressureStage
{
    public StagePressureStage(
        double durationSeconds,
        double spawnRateMultiplier,
        double populationMultiplier)
    {
        if (!double.IsFinite(durationSeconds) || durationSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Stage duration must be greater than zero.");
        }

        if (!double.IsFinite(spawnRateMultiplier) || spawnRateMultiplier <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(spawnRateMultiplier),
                "Spawn-rate multiplier must be greater than zero.");
        }

        if (!double.IsFinite(populationMultiplier) || populationMultiplier <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(populationMultiplier),
                "Population multiplier must be greater than zero.");
        }

        DurationSeconds = durationSeconds;
        SpawnRateMultiplier = spawnRateMultiplier;
        PopulationMultiplier = populationMultiplier;
    }

    public double DurationSeconds { get; }

    public double SpawnRateMultiplier { get; }

    public double PopulationMultiplier { get; }
}

public sealed class RunPressureConfiguration
{
    public RunPressureConfiguration(
        double baseSpawnIntervalSeconds,
        int baseMaxActiveEnemies,
        IEnumerable<StagePressureStage> stages)
    {
        if (!double.IsFinite(baseSpawnIntervalSeconds) || baseSpawnIntervalSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(baseSpawnIntervalSeconds),
                "Base spawn interval must be greater than zero.");
        }

        if (baseMaxActiveEnemies <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(baseMaxActiveEnemies),
                "Base maximum active enemies must be greater than zero.");
        }

        if (stages == null)
        {
            throw new ArgumentNullException(nameof(stages));
        }

        Stages = stages.ToArray();
        if (Stages.Count == 0)
        {
            throw new ArgumentException("At least one pressure stage is required.", nameof(stages));
        }

        BaseSpawnIntervalSeconds = baseSpawnIntervalSeconds;
        BaseMaxActiveEnemies = baseMaxActiveEnemies;
    }

    public double BaseSpawnIntervalSeconds { get; }

    public int BaseMaxActiveEnemies { get; }

    public IReadOnlyList<StagePressureStage> Stages { get; }
}

public readonly record struct SpawnPressure(double SpawnIntervalSeconds, int MaxActiveEnemies);

public readonly record struct RunPressureAdvanceResult(int StageTransitions, bool RunCompleted);

public sealed class RunPressureState
{
    private readonly RunPressureConfiguration _configuration;

    public RunPressureState(RunPressureConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public RunLifecycleStatus Status { get; private set; } = RunLifecycleStatus.NotStarted;

    public double ElapsedSeconds { get; private set; }

    public int CurrentStageIndex { get; private set; }

    public int CurrentStageNumber => CurrentStageIndex + 1;

    public double TotalDurationSeconds => _configuration.Stages.Sum(stage => stage.DurationSeconds);

    public bool IsSpawningEnabled => Status == RunLifecycleStatus.Active;

    public void Start()
    {
        if (Status != RunLifecycleStatus.NotStarted) return;

        Status = RunLifecycleStatus.Active;
    }

    public void Pause()
    {
        if (Status == RunLifecycleStatus.Active)
        {
            Status = RunLifecycleStatus.Paused;
        }
    }

    public void Resume()
    {
        if (Status == RunLifecycleStatus.Paused)
        {
            Status = RunLifecycleStatus.Active;
        }
    }

    public void Stop()
    {
        if (Status is RunLifecycleStatus.Active or RunLifecycleStatus.Paused)
        {
            Status = RunLifecycleStatus.Stopped;
        }
    }

    public RunPressureAdvanceResult Advance(double deltaSeconds)
    {
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Elapsed time must be finite and non-negative.");
        }

        if (Status != RunLifecycleStatus.Active || deltaSeconds == 0)
        {
            return default;
        }

        int previousStage = CurrentStageIndex;
        ElapsedSeconds = Math.Min(ElapsedSeconds + deltaSeconds, TotalDurationSeconds);

        if (ElapsedSeconds >= TotalDurationSeconds)
        {
            CurrentStageIndex = _configuration.Stages.Count - 1;
            Status = RunLifecycleStatus.Completed;
            return new RunPressureAdvanceResult(CurrentStageIndex - previousStage, true);
        }

        CurrentStageIndex = GetStageIndexAt(ElapsedSeconds);
        return new RunPressureAdvanceResult(CurrentStageIndex - previousStage, false);
    }

    public SpawnPressure GetCurrentSpawnPressure()
    {
        StagePressureStage stage = _configuration.Stages[CurrentStageIndex];
        double spawnInterval = _configuration.BaseSpawnIntervalSeconds / stage.SpawnRateMultiplier;
        int maximumActiveEnemies = CalculateMaximumActiveEnemies(stage.PopulationMultiplier);
        return new SpawnPressure(spawnInterval, maximumActiveEnemies);
    }

    private int GetStageIndexAt(double elapsedSeconds)
    {
        double stageEnd = 0;
        for (int index = 0; index < _configuration.Stages.Count; index++)
        {
            stageEnd += _configuration.Stages[index].DurationSeconds;
            if (elapsedSeconds < stageEnd)
            {
                return index;
            }
        }

        return _configuration.Stages.Count - 1;
    }

    private int CalculateMaximumActiveEnemies(double populationMultiplier)
    {
        double scaledPopulation = Math.Floor(_configuration.BaseMaxActiveEnemies * populationMultiplier);
        return (int)Math.Clamp(scaledPopulation, 1d, int.MaxValue);
    }
}
