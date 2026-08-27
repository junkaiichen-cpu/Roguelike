using System;
using System.Numerics;
using Xunit;

namespace BibleSurvivors.Tests;

public sealed class EnemySpawnPlacementTests
{
    private static readonly EnemySpawnConfiguration Configuration = new(30f, 36f, 8);

    [Theory]
    [InlineData(30f, true)]
    [InlineData(33f, true)]
    [InlineData(36f, true)]
    [InlineData(29.99f, false)]
    [InlineData(36.01f, false)]
    public void EnforcesTheConfiguredPlayerSafetyBand(float distance, bool expected)
    {
        bool isWithinBand = EnemySpawnPlacement.IsWithinSpawnBand(
            Vector3.Zero,
            new Vector3(distance, 0, 0),
            Configuration);

        Assert.Equal(expected, isWithinBand);
    }

    [Fact]
    public void FallbackIsSafeAndUsesTheMaximumDistance()
    {
        Vector3 playerPosition = new(5f, 2f, -3f);

        Vector3 fallback = EnemySpawnPlacement.GetFallbackPosition(playerPosition, Configuration);

        Assert.True(EnemySpawnPlacement.IsWithinSpawnBand(playerPosition, fallback, Configuration));
        Assert.Equal(36f, Vector3.Distance(playerPosition, fallback), 5);
    }

    [Fact]
    public void RejectsNonFiniteCandidatePositions()
    {
        Assert.False(EnemySpawnPlacement.IsWithinSpawnBand(
            Vector3.Zero,
            new Vector3(float.NaN, 0, 0),
            Configuration));
    }

    [Theory]
    [InlineData(0f, 36f, 8)]
    [InlineData(30f, 29f, 8)]
    [InlineData(30f, 36f, 0)]
    public void RejectsInvalidConfigurations(float minimumDistance, float maximumDistance, int maximumAttempts)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EnemySpawnConfiguration(
            minimumDistance,
            maximumDistance,
            maximumAttempts));
    }
}
