public sealed class PassiveRuntimeState
{
    public PassiveRuntimeState(string passiveId)
    {
        PassiveId = passiveId;
    }

    public string PassiveId { get; }
    public int Level { get; private set; }
    public float Value { get; private set; }

    public void Apply(float amount)
    {
        Level++;
        Value += amount;
    }
}
