using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public record Choice(TemporaryUpgradeDefinition Upgrade);

public partial class GameManager : Node
{
    private const int UpgradeChoiceCount = 3;

    private Player _player;
    private bool _isRunOver;
    private RunPressureState _runPressureState;
    private RunResultState _runResultState = new();
    private StagePressureConfiguration _stagePressureConfiguration;
    private BossDefinition _completionBossDefinition;
    private bool _bossSpawned;
    private Enemy _activeBoss;

    public Player Player
    {
        get => _player;
        set
        {
            if (_player == value) return;

            if (_player != null)
            {
                _player.Died -= OnPlayerDied;
                _player.ExperienceChanged -= OnPlayerExperienceChanged;
                _player.LeveledUp -= OnPlayerLeveledUp;
            }

            _player = value;

            if (_player != null)
            {
                _player.Died += OnPlayerDied;
                _player.ExperienceChanged += OnPlayerExperienceChanged;
                _player.LeveledUp += OnPlayerLeveledUp;
                BindHud();
                UpdatePlayerProgressDisplay();
                StartRun();
            }
        }
    }

    private double _enemySpawnTimeLeft;

    private EnemyManager _enemyManager;

    private ProgressBar _playerXpBar;

    private bool _isVotePhase;
    private bool _isApplyingUpgrade;
    private uint _pendingLevelUpChoices;
    private UpgradeView _upgradeView;
    private List<Choice> _currentVotes;
    private readonly List<TemporaryUpgradeDefinition> _temporaryUpgrades = new();
    private readonly Dictionary<string, uint> _temporaryUpgradeApplications = new();
    private PackedScene _experiencePickupPrefab;

    private Label _gameTimeLabel;
    private RunResultsView _runResultsView;
    public double GameTime => _runPressureState?.ElapsedSeconds ?? 0;

    public RunLifecycleStatus RunStatus => _runPressureState?.Status ?? RunLifecycleStatus.NotStarted;

    public RunResultStatus RunResult => _runResultState.Status;

    public int CurrentStageNumber => _runPressureState?.CurrentStageNumber ?? 0;

    public event Action<Enemy, int> OnEnemyHit;

    public event Action<int> StageChanged;

    public event Action RunCompleted;

    public override void _Ready()
    {
        base._Ready();

        ResetRunState();
        _enemyManager = new(this, _stagePressureConfiguration.ToEnemySpawnConfiguration());

        ProcessMode = ProcessModeEnum.Always;

        BindHud();

        _experiencePickupPrefab = GD.Load<PackedScene>("res://Prefabs/Progression/experience_pickup.tscn");
        LoadTemporaryUpgrades();
        UpdatePlayerProgressDisplay();
        StartRun();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_isRunOver || _isVotePhase || _runPressureState.Status != RunLifecycleStatus.Active) return;

        RunPressureAdvanceResult progress = _runPressureState.Advance(delta);
        if (_gameTimeLabel != null)
        {
            _gameTimeLabel.Text = $"{Mathf.FloorToInt(GameTime / 60):00}:{Mathf.FloorToInt(GameTime % 60):00}";
        }

        if (progress.StageTransitions > 0)
        {
            _enemySpawnTimeLeft = Math.Min(
                _enemySpawnTimeLeft,
                _runPressureState.GetCurrentSpawnPressure().SpawnIntervalSeconds);
            StageChanged?.Invoke(_runPressureState.CurrentStageNumber);
        }

        if (progress.RunCompleted)
        {
            BeginCompletionBossEncounter();
            return;
        }

        if (Player == null) return;

        SpawnPressure spawnPressure = _runPressureState.GetCurrentSpawnPressure();
        _enemySpawnTimeLeft -= delta;
        if (_enemySpawnTimeLeft > 0) return;
        _enemySpawnTimeLeft = spawnPressure.SpawnIntervalSeconds;

        if (_enemyManager.Enemies.Count < spawnPressure.MaxActiveEnemies)
        {
            _enemyManager.SpawnEnemy();
        }
    }

    //public override void _PhysicsProcess(double delta)
    //{
    //    base._PhysicsProcess(delta);

    //    _enemyManager._PhysicsProcess(delta);
    //}

    private void LoadTemporaryUpgrades()
    {
        TemporaryUpgradeCatalog catalog = GD.Load<TemporaryUpgradeCatalog>(
            "res://Upgrades/development_temporary_upgrade_catalog.tres");
        if (catalog == null)
        {
            GD.PushError("The development temporary upgrade catalog could not be loaded.");
            return;
        }

        foreach (TemporaryUpgradeDefinition upgrade in catalog.Upgrades)
        {
            if (upgrade == null || string.IsNullOrWhiteSpace(upgrade.Id) || upgrade.MaxApplications == 0)
            {
                GD.PushError("Temporary upgrade catalog contains an invalid definition.");
                continue;
            }

            _temporaryUpgrades.Add(upgrade);
            _temporaryUpgradeApplications.Add(upgrade.Id, 0);
        }
    }

    private void ResetRunState()
    {
        _stagePressureConfiguration = GD.Load<StagePressureConfiguration>(
            "res://Stages/development_run_pressure.tres");
        if (_stagePressureConfiguration == null)
        {
            throw new InvalidOperationException("The development run pressure configuration could not be loaded.");
        }

        _completionBossDefinition = _stagePressureConfiguration.CompletionBoss;
        if (_completionBossDefinition == null || _completionBossDefinition.EnemyScene == null)
        {
            throw new InvalidOperationException("The development run pressure configuration requires a completion boss.");
        }

        _runPressureState = new RunPressureState(_stagePressureConfiguration.ToRuntimeConfiguration());
    }

    private void StartRun()
    {
        if (_runPressureState == null || Player == null || _runResultState.IsTerminal) return;

        _runPressureState.Start();
        _enemySpawnTimeLeft = _runPressureState.GetCurrentSpawnPressure().SpawnIntervalSeconds;
    }

    internal void EnemyHit(Enemy enemy, int damages)
    {
        OnEnemyHit?.Invoke(enemy, damages);
    }

    internal void SpawnExperiencePickup(Vector3 position, uint experienceValue)
    {
        if (_isRunOver || experienceValue == 0) return;
        if (_experiencePickupPrefab == null)
        {
            GD.PushError("The experience pickup scene could not be loaded.");
            return;
        }

        ExperiencePickup pickup = _experiencePickupPrefab.Instantiate<ExperiencePickup>();
        pickup.ExperienceValue = experienceValue;
        GetNode<Node3D>("/root/MainScene").AddChild(pickup);
        pickup.GlobalPosition = position;
    }

    private void OnPlayerExperienceChanged(Player player)
    {
        UpdatePlayerProgressDisplay();
    }

    private void OnPlayerLeveledUp(Player player, uint levelsGained)
    {
        if (_isRunOver || levelsGained == 0) return;

        _pendingLevelUpChoices += levelsGained;
        if (!_isVotePhase)
        {
            DisplayNextUpgradeChoices();
        }
    }

    private void DisplayNextUpgradeChoices()
    {
        List<TemporaryUpgradeDefinition> eligibleUpgrades = _temporaryUpgrades
            .Where(upgrade => _temporaryUpgradeApplications[upgrade.Id] < upgrade.MaxApplications)
            .OrderBy(_ => GD.Randf())
            .Take(UpgradeChoiceCount)
            .ToList();

        if (eligibleUpgrades.Count == 0)
        {
            GD.PushWarning("No temporary upgrades remain; resolving pending level-up choices without an upgrade.");
            _pendingLevelUpChoices = 0;
            _isVotePhase = false;
            if (!_isRunOver)
            {
                _runPressureState.Resume();
                GetTree().Paused = false;
            }
            return;
        }

        _isVotePhase = true;
        _runPressureState.Pause();
        GetTree().Paused = true;

        _currentVotes = eligibleUpgrades.Select(upgrade => new Choice(upgrade)).ToList();
        _upgradeView.SetChoices(_currentVotes);
    }

    private void OnChoose(Choice choice)
    {
        if (_isRunOver || !_isVotePhase || _isApplyingUpgrade) return;

        _isApplyingUpgrade = true;
        if (!TryApplyTemporaryUpgrade(choice.Upgrade))
        {
            _isApplyingUpgrade = false;
            return;
        }

        _upgradeView.DisplayChoicePicked(_currentVotes.IndexOf(choice) + 1);
        _upgradeView.Clear();

        _pendingLevelUpChoices--;
        _isVotePhase = false;
        _isApplyingUpgrade = false;

        if (_pendingLevelUpChoices > 0)
        {
            DisplayNextUpgradeChoices();
            return;
        }

        _runPressureState.Resume();
        GetTree().Paused = false;
    }

    private bool TryApplyTemporaryUpgrade(TemporaryUpgradeDefinition upgrade)
    {
        if (upgrade == null || !_temporaryUpgradeApplications.ContainsKey(upgrade.Id)) return false;

        foreach (Node child in Player.GetChildren())
        {
            if (child is not ITemporaryUpgradeReceiver receiver || !receiver.TryApplyTemporaryUpgrade(upgrade)) continue;

            _temporaryUpgradeApplications[upgrade.Id]++;
            return true;
        }

        return false;
    }

    private void OnPlayerDied(Player player)
    {
        if (_isRunOver || !_runResultState.TryDeclareDefeat()) return;

        _activeBoss = null;
        _isRunOver = true;
        _runPressureState.Stop();
        FinishRun();
    }

    private void BeginCompletionBossEncounter()
    {
        if (_isRunOver || _bossSpawned) return;

        _activeBoss = _enemyManager.SpawnBoss(_completionBossDefinition);
        if (_activeBoss == null)
        {
            throw new InvalidOperationException("The configured completion boss could not be spawned.");
        }

        _bossSpawned = true;
        _activeBoss.Died += OnBossDied;
    }

    private void OnBossDied(Enemy boss)
    {
        if (_activeBoss != boss) return;

        _activeBoss = null;
        if (!_runResultState.TryDeclareVictory()) return;

        _isRunOver = true;
        RunCompleted?.Invoke();
        FinishRun();
    }

    private void FinishRun()
    {
        _isRunOver = true;
        _pendingLevelUpChoices = 0;
        _isVotePhase = false;
        _upgradeView?.Clear();
        _runResultsView?.ShowResult(RunResult, Player?.CurrentLevel ?? 0, GameTime);
        GetTree().Paused = true;
    }

    private void BindHud()
    {
        _gameTimeLabel = GetNodeOrNull<Label>("/root/MainScene/HUD/GameTime");
        _playerXpBar = GetNodeOrNull<ProgressBar>("/root/MainScene/HUD/PlayerXPBar");

        UpgradeView nextUpgradeView = GetNodeOrNull<UpgradeView>("/root/MainScene/HUD/UpgradeContainer");
        if (_upgradeView != nextUpgradeView)
        {
            if (_upgradeView != null)
            {
                _upgradeView.OnChoose -= OnChoose;
            }

            _upgradeView = nextUpgradeView;
            if (_upgradeView != null)
            {
                _upgradeView.OnChoose += OnChoose;
            }
        }

        RunResultsView nextResultsView = GetNodeOrNull<RunResultsView>("/root/MainScene/HUD/RunResultsContainer");
        if (_runResultsView != nextResultsView)
        {
            if (_runResultsView != null)
            {
                _runResultsView.RestartRequested -= RestartRun;
            }

            _runResultsView = nextResultsView;
            if (_runResultsView != null)
            {
                _runResultsView.RestartRequested += RestartRun;
            }
        }
    }

    private void RestartRun()
    {
        if (!_runResultState.IsTerminal) return;

        _activeBoss = null;
        _bossSpawned = false;
        _isRunOver = false;
        _isVotePhase = false;
        _isApplyingUpgrade = false;
        _pendingLevelUpChoices = 0;
        _currentVotes?.Clear();
        _enemySpawnTimeLeft = 0;
        _runResultState.Reset();
        ResetRunState();

        foreach (string upgradeId in _temporaryUpgradeApplications.Keys.ToArray())
        {
            _temporaryUpgradeApplications[upgradeId] = 0;
        }

        _enemyManager.ClearEnemies();
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
    }

    private void UpdatePlayerProgressDisplay()
    {
        if (_player == null || _playerXpBar == null) return;

        _playerXpBar.MaxValue = _player.ExperienceRequiredForNextLevel;
        _playerXpBar.Value = _player.CurrentExperience;
    }

    public Vector3 GetRandomPosAroundPlayer(float range) => Player.Position + range * new Vector3(
            (float)GD.RandRange(-1f, 1f),
            0,
            (float)GD.RandRange(-1f, 1f)
            ).Normalized();

    internal Enemy GetNearestEnemy() => _enemyManager.Enemies
        .OrderBy(enemy => (Player.Position - enemy.Position).Length())
        .FirstOrDefault();

    internal int GetMaxEnemyLifepoints(int level) => Mathf.RoundToInt(Math.Log10(level * 10));

    internal int GetMaxEnemyLifepoints() => GetMaxEnemyLifepoints((int)(Player?.CurrentLevel ?? 1));
}
