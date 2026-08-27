using System;
using Xunit;

namespace BibleSurvivors.Tests;

public sealed class PlayerRuntimeStateTests
{
    [Fact]
    public void NewStateStartsAliveAtMaximumHealth()
    {
        var state = new PlayerRuntimeState(200);

        Assert.Equal((uint)200, state.MaxHealth);
        Assert.Equal((uint)200, state.CurrentHealth);
        Assert.False(state.IsDead);
    }

    [Fact]
    public void DamageReducesHealthByTheAppliedAmount()
    {
        var state = new PlayerRuntimeState(200);

        uint appliedDamage = state.ApplyDamage(45);

        Assert.Equal((uint)45, appliedDamage);
        Assert.Equal((uint)155, state.CurrentHealth);
        Assert.False(state.IsDead);
    }

    [Fact]
    public void LethalDamageClampsHealthAndTransitionsToDeath()
    {
        var state = new PlayerRuntimeState(100);

        uint appliedDamage = state.ApplyDamage(150);

        Assert.Equal((uint)100, appliedDamage);
        Assert.Equal((uint)0, state.CurrentHealth);
        Assert.True(state.IsDead);
    }

    [Fact]
    public void DeadStateRejectsFurtherDamageAndHealing()
    {
        var state = new PlayerRuntimeState(100);
        state.ApplyDamage(100);

        uint repeatedDamage = state.ApplyDamage(1);
        uint restoredHealth = state.RestoreHealth(50);

        Assert.Equal((uint)0, repeatedDamage);
        Assert.Equal((uint)0, restoredHealth);
        Assert.Equal((uint)0, state.CurrentHealth);
        Assert.True(state.IsDead);
    }

    [Fact]
    public void HealingCannotExceedMaximumHealth()
    {
        var state = new PlayerRuntimeState(100);
        state.ApplyDamage(20);

        uint restoredHealth = state.RestoreHealth(50);

        Assert.Equal((uint)20, restoredHealth);
        Assert.Equal((uint)100, state.CurrentHealth);
    }

    [Fact]
    public void ZeroMaximumHealthIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerRuntimeState(0));
    }
}
