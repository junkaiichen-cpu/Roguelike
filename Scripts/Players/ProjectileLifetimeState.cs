using System;

public sealed class ProjectileLifetimeState
{
    public ProjectileLifetimeState(double lifetimeSeconds)
    {
        if (!double.IsFinite(lifetimeSeconds) || lifetimeSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lifetimeSeconds));
        }

        LifetimeSeconds = lifetimeSeconds;
    }

    public double LifetimeSeconds { get; }

    public double ElapsedSeconds { get; private set; }

    public bool IsExpired => ElapsedSeconds >= LifetimeSeconds;

    public bool Advance(double deltaSeconds)
    {
        if (!double.IsFinite(deltaSeconds) || deltaSeconds <= 0 || IsExpired)
        {
            return IsExpired;
        }

        ElapsedSeconds = Math.Min(LifetimeSeconds, ElapsedSeconds + deltaSeconds);
        return IsExpired;
    }
}
