public sealed class WeaponRuntimeState
{
    public WeaponRuntimeState(string weaponId)
    {
        WeaponId = weaponId;
    }

    public string WeaponId { get; }

    public int Level { get; private set; }

    public bool IsUnlocked { get; private set; }

    public bool IsActive { get; private set; }

    public uint UpgradeApplications { get; private set; }

    public void Unlock()
    {
        if (IsUnlocked) return;
        IsUnlocked = true;
        IsActive = true;
        Level = 1;
    }

    public void RecordUpgrade()
    {
        if (!IsUnlocked) return;
        UpgradeApplications++;
        Level++;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
