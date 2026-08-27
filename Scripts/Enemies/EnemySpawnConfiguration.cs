using System;
using System.Numerics;

public sealed class EnemySpawnConfiguration
{
    public EnemySpawnConfiguration(
        float minimumDistance,
        float maximumDistance,
        int maximumAttempts)
    {
        if (!float.IsFinite(minimumDistance) || minimumDistance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumDistance));
        }

        if (!float.IsFinite(maximumDistance) || maximumDistance < minimumDistance)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumDistance));
        }

        if (maximumAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumAttempts));
        }

        MinimumDistance = minimumDistance;
        MaximumDistance = maximumDistance;
        MaximumAttempts = maximumAttempts;
    }

    public float MinimumDistance { get; }

    public float MaximumDistance { get; }

    public int MaximumAttempts { get; }
}

public static class EnemySpawnPlacement
{
    public static bool IsWithinSpawnBand(
        Vector3 playerPosition,
        Vector3 candidatePosition,
        EnemySpawnConfiguration configuration)
    {
        if (configuration == null || !IsFinite(candidatePosition) || !IsFinite(playerPosition))
        {
            return false;
        }

        Vector3 horizontalOffset = candidatePosition - playerPosition;
        horizontalOffset.Y = 0;
        float distanceSquared = horizontalOffset.LengthSquared();
        return distanceSquared >= configuration.MinimumDistance * configuration.MinimumDistance
            && distanceSquared <= configuration.MaximumDistance * configuration.MaximumDistance;
    }

    public static Vector3 GetFallbackPosition(Vector3 playerPosition, EnemySpawnConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        return playerPosition + new Vector3(configuration.MaximumDistance, 0, 0);
    }

    private static bool IsFinite(Vector3 value) => float.IsFinite(value.X)
        && float.IsFinite(value.Y)
        && float.IsFinite(value.Z);
}
