using System;
using Xunit;

namespace BibleSurvivors.Tests;

public sealed class RunPressureStateTests
{
    [Fact]
    public void NewRunStartsAtZeroInTheFirstStage()
    {
        var run = CreateRun();

        Assert.Equal(RunLifecycleStatus.NotStarted, run.Status);
        Assert.Equal(0d, run.ElapsedSeconds);
        Assert.Equal(0, run.CurrentStageIndex);
        Assert.Equal(1, run.CurrentStageNumber);
        Assert.False(run.IsSpawningEnabled);
    }

    [Fact]
    public void ActiveRunAdvancesElapsedTimeDeterministically()
    {
        var run = CreateRun();
        run.Start();

        RunPressureAdvanceResult result = run.Advance(3.5);

        Assert.Equal(3.5d, run.ElapsedSeconds, 6);
        Assert.Equal(RunLifecycleStatus.Active, run.Status);
        Assert.Equal(0, result.StageTransitions);
        Assert.False(result.RunCompleted);
        Assert.True(run.IsSpawningEnabled);
    }

    [Fact]
    public void PausedAndStoppedRunsDoNotAdvance()
    {
        var run = CreateRun();
        run.Start();
        run.Advance(2);
        run.Pause();

        run.Advance(5);

        Assert.Equal(2d, run.ElapsedSeconds, 6);
        Assert.Equal(RunLifecycleStatus.Paused, run.Status);
        Assert.False(run.IsSpawningEnabled);

        run.Resume();
        run.Advance(1);
        run.Stop();
        run.Advance(10);

        Assert.Equal(3d, run.ElapsedSeconds, 6);
        Assert.Equal(RunLifecycleStatus.Stopped, run.Status);
        Assert.False(run.IsSpawningEnabled);
    }

    [Fact]
    public void AdvancesToTheNextStageAtTheExactThreshold()
    {
        var run = CreateRun();
        run.Start();

        RunPressureAdvanceResult result = run.Advance(10);

        Assert.Equal(1, result.StageTransitions);
        Assert.Equal(1, run.CurrentStageIndex);
        Assert.Equal(2, run.CurrentStageNumber);
    }

    [Fact]
    public void RemainsInTheNextStageAfterItsTransitionThreshold()
    {
        var run = CreateRun();
        run.Start();

        run.Advance(10.1);

        Assert.Equal(1, run.CurrentStageIndex);
        Assert.Equal(10.1d, run.ElapsedSeconds, 6);
    }

    [Fact]
    public void AdvancesAcrossMultipleStagesWhenOneFrameCrossesMultipleThresholds()
    {
        var run = CreateRun();
        run.Start();

        RunPressureAdvanceResult result = run.Advance(21);

        Assert.Equal(2, result.StageTransitions);
        Assert.Equal(2, run.CurrentStageIndex);
        Assert.Equal(21d, run.ElapsedSeconds, 6);
    }

    [Fact]
    public void CompletesAtTheFinalThresholdAndNeverProgressesFurther()
    {
        var run = CreateRun();
        run.Start();

        RunPressureAdvanceResult result = run.Advance(30);
        run.Advance(10);

        Assert.True(result.RunCompleted);
        Assert.Equal(RunLifecycleStatus.Completed, run.Status);
        Assert.Equal(30d, run.ElapsedSeconds, 6);
        Assert.Equal(2, run.CurrentStageIndex);
        Assert.False(run.IsSpawningEnabled);
    }

    [Fact]
    public void PressureUsesConfiguredSpawnAndPopulationScaling()
    {
        var run = CreateRun();
        run.Start();

        SpawnPressure stageOne = run.GetCurrentSpawnPressure();
        run.Advance(10);
        SpawnPressure stageTwo = run.GetCurrentSpawnPressure();
        run.Advance(10);
        SpawnPressure stageThree = run.GetCurrentSpawnPressure();

        Assert.Equal(1d, stageOne.SpawnIntervalSeconds, 6);
        Assert.Equal(20, stageOne.MaxActiveEnemies);
        Assert.Equal(0.5d, stageTwo.SpawnIntervalSeconds, 6);
        Assert.Equal(30, stageTwo.MaxActiveEnemies);
        Assert.Equal(0.25d, stageThree.SpawnIntervalSeconds, 6);
        Assert.Equal(40, stageThree.MaxActiveEnemies);
    }

    [Fact]
    public void RejectsInvalidConfigurationAndElapsedTime()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new StagePressureStage(0, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new RunPressureConfiguration(0, 20, new[] { new StagePressureStage(1, 1, 1) }));

        var run = CreateRun();
        Assert.Throws<ArgumentOutOfRangeException>(() => run.Advance(-1));
    }

    private static RunPressureState CreateRun() => new(new RunPressureConfiguration(
        baseSpawnIntervalSeconds: 1,
        baseMaxActiveEnemies: 20,
        stages: new[]
        {
            new StagePressureStage(durationSeconds: 10, spawnRateMultiplier: 1, populationMultiplier: 1),
            new StagePressureStage(durationSeconds: 10, spawnRateMultiplier: 2, populationMultiplier: 1.5),
            new StagePressureStage(durationSeconds: 10, spawnRateMultiplier: 4, populationMultiplier: 2),
        }));
}
