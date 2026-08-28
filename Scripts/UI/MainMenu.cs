using Godot;
using System;
using System.Collections.Generic;

public partial class MainMenu : Control
{
    private GameManager _gameManager;
    private VBoxContainer _content;
    private Label _title;
    private Label _status;
    private readonly List<Control> _dynamicControls = new();

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
        BuildHome();
    }

    private void BuildHome()
    {
        ClearDynamicControls();
        _content = new VBoxContainer
        {
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -220,
            OffsetTop = -220,
            OffsetRight = 220,
            OffsetBottom = 220,
            Alignment = BoxContainer.AlignmentMode.Center,
        };
        AddChild(_content);
        _dynamicControls.Add(_content);

        _title = new Label
        {
            Text = "FAITH FIGHT",
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        _title.AddThemeFontSizeOverride("font_size", 42);
        _content.AddChild(_title);

        _status = new Label
        {
            Text = $"Faith {_gameManager.FaithCurrency}",
            HorizontalAlignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        _status.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.25f));
        _content.AddChild(_status);

        AddMenuButton("PLAY", StartGame);
        AddMenuButton("CHARACTERS", BuildCharacters);
        AddMenuButton("SHOP", BuildShop);
        AddMenuButton("SETTINGS", ShowSettings);
    }

    private void AddMenuButton(string text, Action callback)
    {
        Button button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(0, 56),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            ProcessMode = ProcessModeEnum.Always,
        };
        button.AddThemeFontSizeOverride("font_size", 22);
        button.Pressed += callback;
        _content.AddChild(button);
    }

    private void BuildCharacters()
    {
        ClearDynamicControls();
        VBoxContainer panel = CreatePanel("CHARACTERS");
        foreach (CharacterDefinition character in _gameManager.CharacterDefinitions)
        {
            Button button = new Button
            {
                Text = $"{character.DisplayName}\n{character.Description}",
                CustomMinimumSize = new Vector2(0, 82),
                ProcessMode = ProcessModeEnum.Always,
            };
            button.Pressed += () =>
            {
                _gameManager.SelectCharacter(character.CharacterId);
                BuildCharacters();
            };
            panel.AddChild(button);
        }
        AddBackButton(panel);
    }

    private void BuildShop()
    {
        ClearDynamicControls();
        VBoxContainer panel = CreatePanel($"SHOP  |  Faith {_gameManager.FaithCurrency}");
        foreach ((string id, string name) in new[]
        {
            ("damage", "Damage"), ("max_health", "Max HP"), ("xp_gain", "XP Gain"),
            ("move_speed", "Move Speed"), ("pickup_range", "Pickup Range"), ("luck", "Luck"),
        })
        {
            Button button = new Button
            {
                Text = $"{name}  Lv {_gameManager.MetaProgression.GetUpgradeLevel(id)}  |  {_gameManager.MetaProgression.GetUpgradeCost(id)} Faith",
                CustomMinimumSize = new Vector2(0, 48),
                ProcessMode = ProcessModeEnum.Always,
            };
            button.Pressed += () =>
            {
                _gameManager.PurchasePermanentUpgrade(id);
                BuildShop();
            };
            panel.AddChild(button);
        }
        AddBackButton(panel);
    }

    private VBoxContainer CreatePanel(string heading)
    {
        VBoxContainer panel = new VBoxContainer
        {
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -260,
            OffsetTop = -280,
            OffsetRight = 260,
            OffsetBottom = 280,
            Alignment = BoxContainer.AlignmentMode.Center,
        };
        AddChild(panel);
        _dynamicControls.Add(panel);
        Label label = new Label { Text = heading, HorizontalAlignment = HorizontalAlignment.Center };
        label.AddThemeFontSizeOverride("font_size", 30);
        panel.AddChild(label);
        return panel;
    }

    private void AddBackButton(VBoxContainer panel)
    {
        Button back = new Button { Text = "BACK", CustomMinimumSize = new Vector2(0, 52), ProcessMode = ProcessModeEnum.Always };
        back.Pressed += BuildHome;
        panel.AddChild(back);
    }

    private void ShowSettings()
    {
        ClearDynamicControls();
        VBoxContainer panel = CreatePanel("SETTINGS");
        panel.AddChild(new Label
        {
            Text = "PC controls: WASD / Arrow Keys\nAndroid touch controls require SDK/device validation.",
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        });
        AddBackButton(panel);
    }

    private void StartGame()
    {
        GetTree().ChangeSceneToFile("res://Scenes/main_scene.tscn");
    }

    private void ClearDynamicControls()
    {
        foreach (Control control in _dynamicControls)
        {
            if (GodotObject.IsInstanceValid(control)) control.QueueFree();
        }
        _dynamicControls.Clear();
    }
}
