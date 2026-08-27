using System;
using Xunit;

namespace BibleSurvivors.Tests;

public sealed class PlayerProgressionStateTests
{
    [Fact]
    public void NewStateStartsAtTheConfiguredLevelWithNoExperience()
    {
        var state = new PlayerProgressionState(1, 5, 5);

        Assert.Equal((uint)1, state.CurrentLevel);
        Assert.Equal((uint)0, state.CurrentExperience);
        Assert.Equal((uint)5, state.ExperienceRequiredForNextLevel);
    }

    [Fact]
    public void AddsExperienceWithoutLevelingBelowTheThreshold()
    {
        var state = new PlayerProgressionState(1, 5, 5);

        PlayerProgressionResult result = state.AddExperience(3);

        Assert.Equal((uint)3, result.ExperienceAdded);
        Assert.False(result.HasLeveledUp);
        Assert.Equal((uint)3, state.CurrentExperience);
        Assert.Equal((uint)1, state.CurrentLevel);
    }

    [Fact]
    public void ExactThresholdLevelsUpAndAdvancesTheRequirement()
    {
        var state = new PlayerProgressionState(1, 5, 5);

        PlayerProgressionResult result = state.AddExperience(5);

        Assert.Equal((uint)1, result.LevelsGained);
        Assert.Equal((uint)2, state.CurrentLevel);
        Assert.Equal((uint)0, state.CurrentExperience);
        Assert.Equal((uint)10, state.ExperienceRequiredForNextLevel);
    }

    [Fact]
    public void AccumulatedCollectionsReachTheThresholdDeterministically()
    {
        var state = new PlayerProgressionState(1, 5, 5);

        state.AddExperience(2);
        PlayerProgressionResult result = state.AddExperience(3);

        Assert.Equal((uint)1, result.LevelsGained);
        Assert.Equal((uint)2, state.CurrentLevel);
        Assert.Equal((uint)0, state.CurrentExperience);
    }

    [Fact]
    public void RetainsOverflowAfterCrossingOneThreshold()
    {
        var state = new PlayerProgressionState(1, 5, 5);

        state.AddExperience(8);

        Assert.Equal((uint)2, state.CurrentLevel);
        Assert.Equal((uint)3, state.CurrentExperience);
        Assert.Equal((uint)10, state.ExperienceRequiredForNextLevel);
    }

    [Fact]
    public void HandlesMultipleLevelUpsFromOneCollectionDeterministically()
    {
        var state = new PlayerProgressionState(1, 5, 5);

        PlayerProgressionResult result = state.AddExperience(30);

        Assert.Equal((uint)3, result.LevelsGained);
        Assert.Equal((uint)4, state.CurrentLevel);
        Assert.Equal((uint)0, state.CurrentExperience);
        Assert.Equal((uint)20, state.ExperienceRequiredForNextLevel);
    }

    [Fact]
    public void ZeroExperienceDoesNotChangeProgression()
    {
        var state = new PlayerProgressionState(1, 5, 5);

        PlayerProgressionResult result = state.AddExperience(0);

        Assert.Equal((uint)0, result.ExperienceAdded);
        Assert.Equal((uint)0, result.LevelsGained);
        Assert.Equal((uint)1, state.CurrentLevel);
        Assert.Equal((uint)0, state.CurrentExperience);
    }

    [Fact]
    public void RejectsNegativeExperienceAndInvalidConfiguration()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerProgressionState(0, 5, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerProgressionState(1, 0, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerProgressionState(1, 5, 0));

        var state = new PlayerProgressionState(1, 5, 5);
        Assert.Throws<ArgumentOutOfRangeException>(() => state.AddExperience(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => state.AddExperience((long)uint.MaxValue + 1));
    }
}
