using System;

public static class ProjectileWeaponTiming
{
    public static float GetCooldownSeconds(float attacksPerSecond)
    {
        if (attacksPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(attacksPerSecond), "Attack rate must be greater than zero.");
        }

        return 1f / attacksPerSecond;
    }
}
