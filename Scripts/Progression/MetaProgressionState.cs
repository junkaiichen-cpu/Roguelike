using System;
using System.Collections.Generic;

public sealed class MetaProgressionState
{
    public const int CurrentSaveVersion = 1;

    private readonly Dictionary<string, int> _permanentUpgrades = new();
    private readonly HashSet<string> _unlocks = new();

    public int SaveVersion { get; private set; } = CurrentSaveVersion;
    public int FaithCurrency { get; private set; }
    public IReadOnlyDictionary<string, int> PermanentUpgrades => _permanentUpgrades;
    public IReadOnlyCollection<string> Unlocks => _unlocks;

    public void AddFaith(int amount)
    {
        FaithCurrency = (int)Math.Clamp((long)FaithCurrency + amount, 0L, int.MaxValue);
    }

    public bool SpendFaith(int amount)
    {
        if (amount <= 0 || FaithCurrency < amount) return false;
        FaithCurrency -= amount;
        return true;
    }

    public int GetUpgradeLevel(string upgradeId) =>
        _permanentUpgrades.TryGetValue(upgradeId, out int level) ? level : 0;

    public void SetUpgradeLevel(string upgradeId, int level)
    {
        if (string.IsNullOrWhiteSpace(upgradeId) || level < 0) return;
        _permanentUpgrades[upgradeId] = level;
    }

    public void Unlock(string unlockId)
    {
        if (!string.IsNullOrWhiteSpace(unlockId)) _unlocks.Add(unlockId);
    }

    public bool IsUnlocked(string unlockId) => _unlocks.Contains(unlockId);

    public int GetUpgradeCost(string upgradeId)
    {
        int level = GetUpgradeLevel(upgradeId);
        return Math.Clamp(20 * (level + 1), 1, int.MaxValue);
    }

    public bool TryPurchaseUpgrade(string upgradeId)
    {
        if (string.IsNullOrWhiteSpace(upgradeId)) return false;
        int cost = GetUpgradeCost(upgradeId);
        if (!SpendFaith(cost)) return false;
        SetUpgradeLevel(upgradeId, GetUpgradeLevel(upgradeId) + 1);
        return true;
    }
}
