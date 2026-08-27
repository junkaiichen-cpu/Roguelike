using System;
using Xunit;

namespace BibleSurvivors.Tests;

public sealed class EnemyRuntimeStateTests
{
    [Fact]
    public void NewStateStartsAliveAtMaximumHealth()
    {
        var state = new EnemyRuntimeState(20);

        Assert.Equal((uint)20, state.MaxHealth);
        Assert.Equal((uint)20, state.CurrentHealth);
        Assert.False(state.IsDead);
    }

    [Fact]
    public void DamageReducesHealthByTheAppliedAmount()
    {
        var state = new EnemyRuntimeState(20);

        uint appliedDamage = state.ApplyDamage(7);

        Assert.Equal((uint)7, appliedDamage);
        Assert.Equal((uint)13, state.CurrentHealth);
        Assert.False(state.IsDead);
    }

    [Fact]
    public void LethalDamageClampsHealthAndTransitionsToDeath()
    {
        var state = new EnemyRuntimeState(20);

        uint appliedDamage = state.ApplyDamage(30);

        Assert.Equal((uint)20, appliedDamage);
        Assert.Equal((uint)0, state.CurrentHealth);
        Assert.True(state.IsDead);
    }

    [Fact]
    public void DeadStateRejectsFurtherDamage()
    {
        var state = new EnemyRuntimeState(20);
        state.ApplyDamage(20);

        uint appliedDamage = state.ApplyDamage(1);

        Assert.Equal((uint)0, appliedDamage);
        Assert.Equal((uint)0, state.CurrentHealth);
        Assert.True(state.IsDead);
    }

    [Fact]
    public void ZeroMaximumHealthIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyRuntimeState(0));
    }
}
