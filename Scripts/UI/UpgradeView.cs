using Godot;
using System;
using System.Collections.Generic;

public partial class UpgradeView : Control
{
    private PackedScene _choicePanel;

    public event Action<Choice> OnChoose;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _choicePanel = (PackedScene)GD.Load("res://Prefabs/UI/powerup_block.tscn");
        ProcessMode = ProcessModeEnum.Always;
        Clear();
    }

    internal void Clear()
    {
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }
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
            if (choice - 1 == i) continue;
            RemoveChild(children[i]);
        }
    }
}
