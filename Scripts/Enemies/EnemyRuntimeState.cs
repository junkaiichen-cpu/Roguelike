using System;

public sealed class EnemyRuntimeState
{
    public EnemyRuntimeState(uint maxHealth)
    {
        if (maxHealth == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxHealth), "Maximum health must be greater than zero.");
        }

        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    public uint MaxHealth { get; }

    public uint CurrentHealth { get; private set; }

    public bool IsDead => CurrentHealth == 0;

    public uint ApplyDamage(uint damage)
    {
        if (IsDead || damage == 0)
        {
            return 0;
        }

        uint appliedDamage = damage > CurrentHealth ? CurrentHealth : damage;
        CurrentHealth -= appliedDamage;
        return appliedDamage;
    }
}
