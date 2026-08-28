using System;
using System.Numerics;

public sealed class EnemySpawnConfiguration
{
    public EnemySpawnConfiguration(
        float minimumDistance,
        float maximumDistance,
        int maximumAttempts,
        float? normalMinimumDistance = null,
        int? normalMaximumAttempts = null,
        float minimumX = -50f,
        float maximumX = 50f,
        float minimumZ = -50f,
        float maximumZ = 50f)
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

        float resolvedNormalMinimumDistance = normalMinimumDistance ?? minimumDistance;
        int resolvedNormalMaximumAttempts = normalMaximumAttempts ?? maximumAttempts;
        if (!float.IsFinite(resolvedNormalMinimumDistance)
            || resolvedNormalMinimumDistance < minimumDistance
            || resolvedNormalMinimumDistance > maximumDistance)
        {
            throw new ArgumentOutOfRangeException(nameof(normalMinimumDistance));
        }

        if (resolvedNormalMaximumAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(normalMaximumAttempts));
        }

        PlayableBounds bounds = new(minimumX, maximumX, minimumZ, maximumZ);

        MinimumDistance = minimumDistance;
        MaximumDistance = maximumDistance;
        MaximumAttempts = maximumAttempts;
        NormalMinimumDistance = resolvedNormalMinimumDistance;
        NormalMaximumAttempts = resolvedNormalMaximumAttempts;
        Bounds = bounds;
    }

    public float MinimumDistance { get; }

    public float MaximumDistance { get; }

    public int MaximumAttempts { get; }

    public float NormalMinimumDistance { get; }

    public int NormalMaximumAttempts { get; }

    public PlayableBounds Bounds { get; }
}

public readonly record struct PlayableBounds
{
    public PlayableBounds(float minimumX, float maximumX, float minimumZ, float maximumZ)
    {
        if (!float.IsFinite(minimumX) || !float.IsFinite(maximumX) || minimumX >= maximumX
            || !float.IsFinite(minimumZ) || !float.IsFinite(maximumZ) || minimumZ >= maximumZ)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumX));
        }

        MinimumX = minimumX;
        MaximumX = maximumX;
        MinimumZ = minimumZ;
        MaximumZ = maximumZ;
    }

    public float MinimumX { get; }
    public float MaximumX { get; }
    public float MinimumZ { get; }
    public float MaximumZ { get; }

    public bool Contains(Vector3 position) => position.X >= MinimumX
        && position.X <= MaximumX
        && position.Z >= MinimumZ
        && position.Z <= MaximumZ;
}

public static class EnemySpawnPlacement
{
    public static bool IsWithinSpawnBand(
        Vector3 playerPosition,
        Vector3 candidatePosition,
        EnemySpawnConfiguration configuration,
        float? minimumDistanceOverride = null,
        PlayableBounds? boundsOverride = null)
    {
        if (configuration == null || !IsFinite(candidatePosition) || !IsFinite(playerPosition))
        {
            return false;
        }

        Vector3 horizontalOffset = candidatePosition - playerPosition;
        horizontalOffset.Y = 0;
        float distanceSquared = horizontalOffset.LengthSquared();
        float minimumDistance = minimumDistanceOverride ?? configuration.MinimumDistance;
        if (!float.IsFinite(minimumDistance)
            || minimumDistance < configuration.MinimumDistance
            || minimumDistance > configuration.MaximumDistance)
        {
            return false;
        }

        return distanceSquared >= minimumDistance * minimumDistance
            && distanceSquared <= configuration.MaximumDistance * configuration.MaximumDistance
            && (boundsOverride ?? configuration.Bounds).Contains(candidatePosition);
    }

    public static Vector3 GetFallbackPosition(
        Vector3 playerPosition,
        EnemySpawnConfiguration configuration,
        float? minimumDistanceOverride = null,
        PlayableBounds? boundsOverride = null)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        float minimumDistance = minimumDistanceOverride ?? configuration.MinimumDistance;
        if (!float.IsFinite(minimumDistance)
            || minimumDistance < configuration.MinimumDistance
            || minimumDistance > configuration.MaximumDistance)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumDistanceOverride));
        }

        PlayableBounds bounds = boundsOverride ?? configuration.Bounds;
        float bestDistanceSquared = -1;
        Vector3 bestPosition = default;
        for (int sample = 0; sample < 360; sample++)
        {
            float angle = sample * MathF.Tau / 360f;
            Vector3 candidate = playerPosition + new Vector3(
                MathF.Cos(angle) * configuration.MaximumDistance,
                0,
                MathF.Sin(angle) * configuration.MaximumDistance);
            if (!IsWithinSpawnBand(playerPosition, candidate, configuration, minimumDistance, bounds))
            {
                continue;
            }

            float distanceSquared = (candidate - playerPosition).LengthSquared();
            if (distanceSquared > bestDistanceSquared)
            {
                bestDistanceSquared = distanceSquared;
                bestPosition = candidate;
            }
        }

        if (bestDistanceSquared < 0)
        {
            throw new InvalidOperationException("No safe position exists within the playable bounds.");
        }

        return bestPosition;
    }

    private static bool IsFinite(Vector3 value) => float.IsFinite(value.X)
        && float.IsFinite(value.Y)
        && float.IsFinite(value.Z);
}
