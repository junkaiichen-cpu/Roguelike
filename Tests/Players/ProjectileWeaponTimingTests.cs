using System;
using Xunit;

namespace BibleSurvivors.Tests;

public sealed class ProjectileWeaponTimingTests
{
    [Theory]
    [InlineData(1f, 1f)]
    [InlineData(2.5f, 0.4f)]
    public void ConvertsAttackRateToCooldown(float attacksPerSecond, float expectedCooldown)
    {
        float cooldown = ProjectileWeaponTiming.GetCooldownSeconds(attacksPerSecond);

        Assert.Equal(expectedCooldown, cooldown, 5);
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    public void RejectsNonPositiveAttackRates(float attacksPerSecond)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ProjectileWeaponTiming.GetCooldownSeconds(attacksPerSecond));
    }
}
