using System.Numerics;
using Xunit;

namespace BibleSurvivors.Tests;

public sealed class EnemyPursuitTests
{
    [Fact]
    public void MovesTowardADistantTargetAtConfiguredSpeed()
    {
        Vector3 velocity = EnemyPursuit.CalculateVelocity(Vector3.Zero, new Vector3(10, 0, 0), 4);

        Assert.Equal(new Vector3(4, 0, 0), velocity);
    }

    [Fact]
    public void SlowsInsideOneUnitToAvoidOvershootingTheTarget()
    {
        Vector3 velocity = EnemyPursuit.CalculateVelocity(Vector3.Zero, new Vector3(0.5f, 0, 0), 4);

        Assert.Equal(new Vector3(2, 0, 0), velocity);
    }

    [Fact]
    public void DoesNotMoveWhenAlreadyAtTargetOrMovementIsDisabled()
    {
        Assert.Equal(Vector3.Zero, EnemyPursuit.CalculateVelocity(Vector3.Zero, Vector3.Zero, 4));
        Assert.Equal(Vector3.Zero, EnemyPursuit.CalculateVelocity(Vector3.Zero, Vector3.UnitX, 0));
    }
}
