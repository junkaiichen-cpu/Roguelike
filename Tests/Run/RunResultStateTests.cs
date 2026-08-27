using Xunit;

namespace BibleSurvivors.Tests;

public sealed class RunResultStateTests
{
    [Fact]
    public void NewRunIsRunning()
    {
        var result = new RunResultState();

        Assert.Equal(RunResultStatus.Running, result.Status);
        Assert.False(result.IsTerminal);
    }

    [Fact]
    public void VictoryIsATerminalTransition()
    {
        var result = new RunResultState();

        Assert.True(result.TryDeclareVictory());
        Assert.Equal(RunResultStatus.Victory, result.Status);
        Assert.True(result.IsTerminal);
        Assert.False(result.TryDeclareVictory());
        Assert.False(result.TryDeclareDefeat());
        Assert.Equal(RunResultStatus.Victory, result.Status);
    }

    [Fact]
    public void DefeatIsATerminalTransition()
    {
        var result = new RunResultState();

        Assert.True(result.TryDeclareDefeat());
        Assert.Equal(RunResultStatus.Defeat, result.Status);
        Assert.True(result.IsTerminal);
        Assert.False(result.TryDeclareDefeat());
        Assert.False(result.TryDeclareVictory());
        Assert.Equal(RunResultStatus.Defeat, result.Status);
    }

    [Fact]
    public void ResetStartsANewRunningResult()
    {
        var result = new RunResultState();
        result.TryDeclareVictory();

        result.Reset();

        Assert.Equal(RunResultStatus.Running, result.Status);
        Assert.False(result.IsTerminal);
        Assert.True(result.TryDeclareDefeat());
    }
}
