public sealed class EventRuntimeState
{
    public bool IsActive { get; private set; }
    public float RemainingSeconds { get; private set; }

    public void Start(float durationSeconds)
    {
        IsActive = true;
        RemainingSeconds = durationSeconds;
    }

    public void Advance(float delta)
    {
        if (!IsActive) return;
        RemainingSeconds -= delta;
        if (RemainingSeconds <= 0) IsActive = false;
    }
}
