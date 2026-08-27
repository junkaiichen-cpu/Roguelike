using Godot;
using System;

public partial class RunResultsView : Control
{
    private Label _resultLabel;
    private Label _summaryLabel;
    private Button _restartButton;

    public event Action RestartRequested;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _resultLabel = GetNode<Label>("MarginContainer/VBoxContainer/Result");
        _summaryLabel = GetNode<Label>("MarginContainer/VBoxContainer/Summary");
        _restartButton = GetNode<Button>("MarginContainer/VBoxContainer/RestartButton");
        _restartButton.ProcessMode = ProcessModeEnum.Always;
        _restartButton.Pressed += RequestRestart;
        Hide();
    }

    public override void _ExitTree()
    {
        if (_restartButton != null)
        {
            _restartButton.Pressed -= RequestRestart;
        }

        base._ExitTree();
    }

    public void ShowResult(RunResultStatus result, uint level, double elapsedSeconds)
    {
        _resultLabel.Text = result == RunResultStatus.Victory ? "Victory" : "Defeat";
        _summaryLabel.Text = $"Level {level}  •  {FormatElapsedTime(elapsedSeconds)}";
        _restartButton.Disabled = false;
        Show();
    }

    private void RequestRestart()
    {
        _restartButton.Disabled = true;
        RestartRequested?.Invoke();
    }

    private static string FormatElapsedTime(double elapsedSeconds)
    {
        int wholeSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds));
        return $"{wholeSeconds / 60:00}:{wholeSeconds % 60:00}";
    }
}
