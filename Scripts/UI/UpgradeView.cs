using Godot;
using System;
using System.Collections.Generic;

public partial class UpgradeView : Control
{
    private PackedScene _choicePanel;
    private HBoxContainer _actionBar;
    private Choice _lastHoveredChoice;

    public event Action<Choice> OnChoose;
    public event Action RerollRequested;
    public event Action SkipRequested;
    public event Action<Choice> BanishRequested;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _choicePanel = (PackedScene)GD.Load("res://Prefabs/UI/powerup_block.tscn");
        ProcessMode = ProcessModeEnum.Always;
        Clear();
        _actionBar = new HBoxContainer { Name = "UpgradeActions", ProcessMode = ProcessModeEnum.Always };
        AddChild(_actionBar);
        AddAction("Reroll", () => RerollRequested?.Invoke());
        AddAction("Skip", () => SkipRequested?.Invoke());
        AddAction("Banish", () => BanishRequested?.Invoke(GetSelectedChoice()));
    }

    internal void Clear()
    {
        foreach (var child in GetChildren())
        {
            if (child == _actionBar) continue;
            RemoveChild(child);
            child.QueueFree();
        }
    }

    private void AddAction(string text, Action callback)
    {
        Button button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(100, 38),
            ProcessMode = ProcessModeEnum.Always,
        };
        button.Pressed += callback;
        _actionBar.AddChild(button);
    }

    private Choice GetSelectedChoice()
    {
        return _lastHoveredChoice;
    }

    internal void SetChoices(List<Choice> choices)
    {
        Clear();

        foreach (var choice in choices)
        {
            var panel = _choicePanel.Instantiate<Button>();
            panel.ProcessMode = ProcessModeEnum.Always;
            panel.Pressed += () =>
            {
                panel.Disabled = true;
                OnChoose?.Invoke(choice);
            };
            panel.SetMeta("choice_id", choice.Upgrade.Id);
            panel.MouseEntered += () => _lastHoveredChoice = choice;
            AddChild(panel);

            var name = panel.GetNode<Label>("MarginContainer/VBoxContainer/VBoxPlayer/Name");
            name.Text = choice.Upgrade.DisplayName;
            var description = panel.GetNode<Label>("MarginContainer/VBoxContainer/VBoxPlayer/Description");
            description.Text = choice.Upgrade.Description;

            panel.GetNode<Control>("MarginContainer/VBoxContainer/Separator").Visible = false;
            panel.GetNode<Control>("MarginContainer/VBoxContainer/VBoxEnemy").Visible = false;
        }
    }

    internal void DisplayChoicePicked(int choice)
    {
        var children = GetChildren();
        for (int i = children.Count - 1; i >= 0; --i)
        {
            if (children[i] == _actionBar) continue;
            if (choice - 1 == i) continue;
            RemoveChild(children[i]);
        }
    }
}
