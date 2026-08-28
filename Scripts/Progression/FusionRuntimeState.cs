public sealed class FusionRuntimeState
{
    public FusionRuntimeState(string fusionId, string resultWeaponId)
    {
        FusionId = fusionId;
        ResultWeaponId = resultWeaponId;
    }

    public string FusionId { get; }
    public string ResultWeaponId { get; }
    public int Level { get; private set; } = 1;
    public bool Active { get; private set; }

    public void Activate() => Active = true;

    public void ActivateWithLevel(int level)
    {
        Active = true;
        Level = System.Math.Max(1, level);
    }

    public void RecordUpgrade()
    {
        if (Active) Level++;
    }

    public void Deactivate()
    {
        Active = false;
    }
}