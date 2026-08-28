using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class RunResultsView : Control
{
    private Label _resultLabel;
    private Label _summaryLabel;
    private Button _restartButton;
    private GameManager _gameManager;
    private Label _faithLabel;
    private VBoxContainer _shopContainer;
    private Button _shopToggle;
    private OptionButton _characterSelect;
    private readonly Dictionary<string, Button> _shopButtons = new();

    public event Action RestartRequested;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _resultLabel = GetNode<Label>("MarginContainer/VBoxContainer/Result");
        _summaryLabel = GetNode<Label>("MarginContainer/VBoxContainer/Summary");
        _restartButton = GetNode<Button>("MarginContainer/VBoxContainer/RestartButton");
        _restartButton.ProcessMode = ProcessModeEnum.Always;
        _restartButton.Pressed += RequestRestart;
        _gameManager = GetNode<GameManager>("/root/GameManager");
        CreateShopControls();
        CreateCharacterSelect();
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
        RefreshShop();
        Show();
    }

    private void CreateShopControls()
    {
        VBoxContainer parent = GetNode<VBoxContainer>("MarginContainer/VBoxContainer");
        _shopToggle = new Button { ProcessMode = ProcessModeEnum.Always };
        parent.AddChild(_shopToggle);
        _shopContainer = new VBoxContainer { Visible = false, ProcessMode = ProcessModeEnum.Always };
        parent.AddChild(_shopContainer);
        foreach ((string id, string name) in new[]
        {
            ("damage", "Damage"), ("max_health", "Max HP"), ("xp_gain", "XP Gain"),
            ("move_speed", "Move Speed"), ("pickup_range", "Pickup Range"), ("luck", "Luck"),
        })
        {
            Button button = new Button { ProcessMode = ProcessModeEnum.Always };
            button.Pressed += () => Purchase(id);
            _shopContainer.AddChild(button);
            _shopButtons[id] = button;
        }
        _shopToggle.Pressed += () => _shopContainer.Visible = !_shopContainer.Visible;
        _gameManager.MetaProgressionChanged += RefreshShop;
    }

    private void CreateCharacterSelect()
    {
        VBoxContainer parent = GetNode<VBoxContainer>("MarginContainer/VBoxContainer");
        _characterSelect = new OptionButton { ProcessMode = ProcessModeEnum.Always };
        parent.AddChild(_characterSelect);
        foreach (CharacterDefinition character in _gameManager.CharacterDefinitions)
        {
            _characterSelect.AddItem($"Next: {character.DisplayName}");
            _characterSelect.SetItemMetadata(_characterSelect.ItemCount - 1, character.CharacterId);
        }
        int selectedIndex = _gameManager.CharacterDefinitions
            .ToList().IndexOf(_gameManager.SelectedCharacter);
        if (selectedIndex >= 0) _characterSelect.Select(selectedIndex);
        _characterSelect.ItemSelected += index =>
            _gameManager.SelectCharacter(_characterSelect.GetItemMetadata((int)index).AsString());
    }

    private void RefreshShop()
    {
        if (_shopContainer == null || _gameManager?.MetaProgression == null) return;
        _shopToggle.Text = $"Meta Shop  |  Faith {_gameManager.FaithCurrency}";
        foreach ((string id, Button button) in _shopButtons)
        {
            int level = _gameManager.MetaProgression.GetUpgradeLevel(id);
            button.Text = $"{id} Lv {level}  |  {_gameManager.MetaProgression.GetUpgradeCost(id)} Faith";
            button.Disabled = !_gameManager.MetaProgression.IsUnlocked("shop")
                && _gameManager.FaithCurrency < _gameManager.MetaProgression.GetUpgradeCost(id);
        }
    }

    private void Purchase(string upgradeId)
    {
        if (_gameManager.PurchasePermanentUpgrade(upgradeId)) RefreshShop();
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
