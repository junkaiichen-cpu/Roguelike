public enum RunResultStatus
{
    Running,
    Victory,
    Defeat,
}

public sealed class RunResultState
{
    public RunResultStatus Status { get; private set; } = RunResultStatus.Running;

    public bool IsTerminal => Status is RunResultStatus.Victory or RunResultStatus.Defeat;

    public bool TryDeclareVictory() => TrySetTerminalStatus(RunResultStatus.Victory);

    public bool TryDeclareDefeat() => TrySetTerminalStatus(RunResultStatus.Defeat);

    public void Reset()
    {
        Status = RunResultStatus.Running;
    }

    private bool TrySetTerminalStatus(RunResultStatus terminalStatus)
    {
        if (IsTerminal)
        {
            return false;
        }

        Status = terminalStatus;
        return true;
    }
}
