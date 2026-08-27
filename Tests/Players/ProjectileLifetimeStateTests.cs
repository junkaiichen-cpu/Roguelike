using System;
using Xunit;

namespace BibleSurvivors.Tests;

public sealed class ProjectileLifetimeStateTests
{
    [Fact]
    public void StartsActiveAndExpiresAtItsExactLifetime()
    {
        var state = new ProjectileLifetimeState(2d);

        Assert.False(state.IsExpired);
        Assert.False(state.Advance(1.5d));
        Assert.Equal(1.5d, state.ElapsedSeconds, 5);

        Assert.True(state.Advance(0.5d));
        Assert.True(state.IsExpired);
        Assert.Equal(2d, state.ElapsedSeconds, 5);
    }

    [Fact]
    public void StopsAccumulatingAfterExpiration()
    {
        var state = new ProjectileLifetimeState(1d);

        Assert.True(state.Advance(5d));
        Assert.True(state.Advance(1d));
        Assert.Equal(1d, state.ElapsedSeconds, 5);
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void RejectsInvalidLifetimes(double lifetimeSeconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProjectileLifetimeState(lifetimeSeconds));
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    public void IgnoresInvalidElapsedTime(double deltaSeconds)
    {
        var state = new ProjectileLifetimeState(1d);

        Assert.False(state.Advance(deltaSeconds));
        Assert.Equal(0d, state.ElapsedSeconds, 5);
    }
}
