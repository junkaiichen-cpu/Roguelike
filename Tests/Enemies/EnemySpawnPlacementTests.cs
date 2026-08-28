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
    public void SpawnBandUsesHorizontalDistanceOnly()
    {
        bool isWithinBand = EnemySpawnPlacement.IsWithinSpawnBand(
            new Vector3(5f, 20f, -3f),
            new Vector3(35f, -100f, -3f),
            Configuration);

        Assert.True(isWithinBand);
    }

    [Fact]
    public void FallbackIsSafeAndUsesTheMaximumDistance()
    {
        Vector3 playerPosition = new(5f, 2f, -3f);

        Vector3 fallback = EnemySpawnPlacement.GetFallbackPosition(playerPosition, Configuration);

        Assert.True(EnemySpawnPlacement.IsWithinSpawnBand(playerPosition, fallback, Configuration));
        Assert.True(float.IsFinite(fallback.X));
        Assert.True(float.IsFinite(fallback.Y));
        Assert.True(float.IsFinite(fallback.Z));
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

    [Fact]
    public void RejectsNonFinitePlayerPositions()
    {
        Assert.False(EnemySpawnPlacement.IsWithinSpawnBand(
            new Vector3(float.PositiveInfinity, 0, 0),
            new Vector3(30, 0, 0),
            Configuration));
    }

    [Fact]
    public void StoresTheStricterNormalEnemySpawnProfile()
    {
        var configuration = new EnemySpawnConfiguration(30f, 36f, 8, 34f, 24);

        Assert.Equal(34f, configuration.NormalMinimumDistance);
        Assert.Equal(24, configuration.NormalMaximumAttempts);
    }

    [Fact]
    public void NormalFallbackRespectsTheStricterMinimumDistance()
    {
        Vector3 fallback = EnemySpawnPlacement.GetFallbackPosition(
            Vector3.Zero,
            Configuration,
            Configuration.NormalMinimumDistance);

        Assert.True(EnemySpawnPlacement.IsWithinSpawnBand(
            Vector3.Zero,
            fallback,
            Configuration,
            Configuration.NormalMinimumDistance));
        Assert.True(Vector3.Distance(Vector3.Zero, fallback) >= Configuration.NormalMinimumDistance);
    }

    [Fact]
    public void SpawnPlacementRejectsCandidatesOutsidePlayableBounds()
    {
        var bounds = new PlayableBounds(-10f, 10f, -10f, 10f);

        Assert.False(EnemySpawnPlacement.IsWithinSpawnBand(
            Vector3.Zero,
            new Vector3(12f, 0, 0),
            Configuration,
            Configuration.MinimumDistance,
            bounds));
    }

    [Fact]
    public void FallbackRemainsInsidePlayableBounds()
    {
        var configuration = new EnemySpawnConfiguration(3f, 8f, 8, 3f, 8, -10f, 10f, -10f, 10f);
        Vector3 playerPosition = new(6f, 0, 0);

        Vector3 fallback = EnemySpawnPlacement.GetFallbackPosition(
            playerPosition,
            configuration,
            configuration.MinimumDistance,
            configuration.Bounds);

        Assert.True(configuration.Bounds.Contains(fallback));
        Assert.True(EnemySpawnPlacement.IsWithinSpawnBand(
            playerPosition,
            fallback,
            configuration,
            configuration.MinimumDistance,
            configuration.Bounds));
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
