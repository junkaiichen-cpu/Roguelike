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
                _player.BuildChanged -= OnPlayerBuildChanged;
            }

            _player = value;

            if (_player != null)
            {
                _player.Died += OnPlayerDied;
                _player.ExperienceChanged += OnPlayerExperienceChanged;
                _player.LeveledUp += OnPlayerLeveledUp;
                _player.BuildChanged += OnPlayerBuildChanged;
                EnsureWeaponStates();
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
    private readonly Dictionary<string, WeaponRuntimeState> _weaponStates = new();
    private MetaProgressionState _metaProgressionState;
    private readonly Dictionary<string, PassiveRuntimeState> _passiveStates = new();
        private readonly List<EventDefinition> _eventDefinitions = new();
        private readonly EventRuntimeState _eventState = new();
        private float _eventCooldown = 45f;
    private readonly List<FusionDefinition> _fusionDefinitions = new();
    private readonly Dictionary<string, FusionRuntimeState> _fusionStates = new();
    private readonly HashSet<ExperiencePickup> _experiencePickups = new();
    private PackedScene _experiencePickupPrefab;
    private PackedScene _faithSurgePickupPrefab;
    private Player _vacuumPlayer;
    private double _vacuumRemaining;

    private Label _gameTimeLabel;
    private CombatHud _combatHud;
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
        _faithSurgePickupPrefab = GD.Load<PackedScene>("res://Prefabs/Progression/faith_surge_pickup.tscn");
        LoadTemporaryUpgrades();
        LoadPassiveDefinitions();
        LoadFusionDefinitions();
        _metaProgressionState = MetaProgressionSave.Load();
        EnsureWeaponStates();
        UpdatePlayerProgressDisplay();
        StartRun();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_isRunOver || _runPressureState.Status != RunLifecycleStatus.Active) return;
        _eventCooldown -= (float)delta;
        _eventState.Advance((float)delta);
        if (_eventCooldown <= 0 && !_eventState.IsActive)
        {
            TriggerNextEvent();
            _eventCooldown = 45f;
        }

        if (_vacuumRemaining > 0)
        {
            _vacuumRemaining -= delta;
            if (_vacuumPlayer != null && !_vacuumPlayer.IsDead)
            {
                foreach (ExperiencePickup pickup in _experiencePickups.ToArray())
                {
                    if (GodotObject.IsInstanceValid(pickup))
                    {
                        pickup.StartVacuum(_vacuumPlayer);
                    }
                }
            }
        }

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

        if (Player == null || _isVotePhase) return;

        SpawnPressure spawnPressure = _runPressureState.GetCurrentSpawnPressure();
        _enemySpawnTimeLeft -= delta;
        if (_enemySpawnTimeLeft > 0) return;
        _enemySpawnTimeLeft = spawnPressure.SpawnIntervalSeconds;

        if (_enemyManager.Enemies.Count < spawnPressure.MaxActiveEnemies)
        {
            float eliteChance = GameTime switch
            {
                < 60 => 0f,
                < 180 => 0.1f,
                < 270 => 0.18f,
                _ => 0.25f,
            };
            if (GD.Randf() < eliteChance)
            {
                _enemyManager.SpawnElite();
            }

            int burstSize = GameTime switch
            {
                < 60 => 1,
                < 180 => 2,
                < 270 => 3,
                _ => 4,
            };
            int availableSlots = Mathf.Max(0, spawnPressure.MaxActiveEnemies - _enemyManager.Enemies.Count);
            for (int index = 0; index < Mathf.Min(burstSize, availableSlots); index++)
            {
                _enemyManager.SpawnEnemy();
            }
        }
    }


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

    private void LoadPassiveDefinitions()
    {
        string[] paths =
        {
            "res://Upgrades/passive_damage.tres",
            "res://Upgrades/passive_max_health.tres",
            "res://Upgrades/passive_move_speed.tres",
            "res://Upgrades/passive_cooldown.tres",
            "res://Upgrades/passive_area.tres",
            "res://Upgrades/passive_projectile_size.tres",
            "res://Upgrades/passive_xp_gain.tres",
            "res://Upgrades/passive_pickup_range.tres",
        };

        foreach (string path in paths)
        {
            PassiveDefinition passive = GD.Load<PassiveDefinition>(path);
            if (passive == null || string.IsNullOrWhiteSpace(passive.Id)) continue;
            if (!_passiveStates.ContainsKey(passive.Id)) _passiveStates.Add(passive.Id, new PassiveRuntimeState(passive.Id));
            _temporaryUpgrades.Add(new TemporaryUpgradeDefinition
            {
                Id = passive.Id,
                DisplayName = passive.DisplayName,
                Description = passive.Description,
                Effect = ToTemporaryEffect(passive.Effect),
                Amount = passive.Amount,
                MaxApplications = passive.MaxApplications,
            });
            _temporaryUpgradeApplications.Add(passive.Id, 0);
        }
    }

    private static TemporaryUpgradeEffect ToTemporaryEffect(PassiveEffect effect) => effect switch
    {
        PassiveEffect.Damage => TemporaryUpgradeEffect.PassiveDamage,
        PassiveEffect.MaxHealth => TemporaryUpgradeEffect.PassiveMaxHealth,
        PassiveEffect.MoveSpeed => TemporaryUpgradeEffect.PassiveMoveSpeed,
        PassiveEffect.Cooldown => TemporaryUpgradeEffect.PassiveCooldown,
        PassiveEffect.Area => TemporaryUpgradeEffect.PassiveArea,
        PassiveEffect.ProjectileSize => TemporaryUpgradeEffect.PassiveProjectileSize,
        PassiveEffect.ExperienceGain => TemporaryUpgradeEffect.PassiveExperienceGain,
        _ => TemporaryUpgradeEffect.PassivePickupRange,
    };

    private void LoadEventDefinitions()
    {
        string[] paths =
        {
            "res://Events/xp_rain.tres",
            "res://Events/elite_horde.tres",
            "res://Events/faith_surge.tres",
            "res://Events/holy_ground.tres",
            "res://Events/angel_blessing.tres",
            "res://Events/demon_wave.tres",
        };

        foreach (string path in paths)
        {
            EventDefinition definition = GD.Load<EventDefinition>(path);
            if (definition != null) _eventDefinitions.Add(definition);
        }
    }

    private void TriggerNextEvent()
    {
        if (_eventDefinitions.Count == 0 || Player == null) return;
        EventDefinition eventDefinition = _eventDefinitions[GD.RandRange(0, _eventDefinitions.Count - 1)];
        _eventState.Start(eventDefinition.DurationSeconds);

        switch (eventDefinition.Type)
        {
            case RunEventType.XpRain:
                for (int index = 0; index < Mathf.Min(8, eventDefinition.Intensity * 4); index++)
                {
                    SpawnExperiencePickup(GetRandomPosAroundPlayer(6f), 2);
                }
                break;
            case RunEventType.EliteHorde:
                for (int index = 0; index < Mathf.Min(4, eventDefinition.Intensity + 1); index++) _enemyManager.SpawnElite();
                break;
            case RunEventType.FaithSurge:
                ActivateExperienceVacuum(Player);
                break;
            case RunEventType.HolyGround:
                Player.Heal((uint)Mathf.Max(1, eventDefinition.Intensity * 10));
                break;
            case RunEventType.AngelBlessing:
                Player.Heal((uint)Mathf.Max(1, eventDefinition.Intensity * 20));
                Player.CollectExperience((uint)Mathf.Max(1, eventDefinition.Intensity * 3));
                break;
            case RunEventType.DemonWave:
                for (int index = 0; index < Mathf.Min(6, eventDefinition.Intensity * 3); index++) _enemyManager.SpawnEnemy();
                break;
        }
    }

    private void LoadFusionDefinitions()
    {
        string[] paths =
        {
            "res://Upgrades/development_fusion_holy_light_bible.tres",
            "res://Upgrades/development_fusion_fire_spirit_water.tres",
            "res://Upgrades/development_fusion_orb_lifesteal.tres",
            "res://Upgrades/development_fusion_cross_lightning.tres",
        };

        foreach (string path in paths)
        {
            FusionDefinition definition = GD.Load<FusionDefinition>(path);
            if (definition != null) _fusionDefinitions.Add(definition);
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

        foreach (ExperiencePickup existingPickup in _experiencePickups.ToArray())
        {
            if (!GodotObject.IsInstanceValid(existingPickup)
                || existingPickup.GlobalPosition.DistanceTo(position) > 2f)
            {
                continue;
            }

            existingPickup.AddExperience(experienceValue);
            return;
        }

        ExperiencePickup pickup = _experiencePickupPrefab.Instantiate<ExperiencePickup>();
        pickup.ExperienceValue = experienceValue;
        pickup.PickupRadius = Player?.TotalPickupRadius ?? pickup.PickupRadius;
        GetNode<Node3D>("/root/MainScene").AddChild(pickup);
        _experiencePickups.Add(pickup);
        pickup.Finished += OnExperiencePickupFinished;
        pickup.GlobalPosition = position;
        if (_vacuumRemaining > 0 && _vacuumPlayer != null)
        {
            pickup.StartVacuum(_vacuumPlayer);
        }
    }

    internal void SpawnFaithSurge(Vector3 position)
    {
        if (_isRunOver || _faithSurgePickupPrefab == null) return;
        FaithSurgePickup pickup = _faithSurgePickupPrefab.Instantiate<FaithSurgePickup>();
        GetNode<Node3D>("/root/MainScene").AddChild(pickup);
        pickup.GlobalPosition = position + new Vector3(0, 0.8f, 0);
    }

    internal void ActivateExperienceVacuum(Player player)
    {
        _vacuumPlayer = player;
        _vacuumRemaining = 0.8d;
        foreach (ExperiencePickup pickup in _experiencePickups.ToArray())
        {
            if (GodotObject.IsInstanceValid(pickup))
            {
                pickup.StartVacuum(player);
            }
        }
    }

    private void OnExperiencePickupFinished(ExperiencePickup pickup)
    {
        _experiencePickups.Remove(pickup);
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
            .Where(IsUpgradeAvailable)
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
            }
            return;
        }

        _isVotePhase = true;

        _currentVotes = eligibleUpgrades.Select(upgrade => new Choice(upgrade)).ToList();
        _upgradeView.SetChoices(_currentVotes);
    }

    private bool IsUpgradeAvailable(TemporaryUpgradeDefinition upgrade)
    {
        if (upgrade.Effect is TemporaryUpgradeEffect.UnlockCross
            or TemporaryUpgradeEffect.UnlockLightning
            or TemporaryUpgradeEffect.UnlockBible
            or TemporaryUpgradeEffect.UnlockOrb
            or TemporaryUpgradeEffect.UnlockFire
            or TemporaryUpgradeEffect.UnlockSpiritWater
            or TemporaryUpgradeEffect.UnlockLifesteal)
        {
            return false;
        }

        if (upgrade.Effect is not (TemporaryUpgradeEffect.CrossDamage
            or TemporaryUpgradeEffect.CrossSize
            or TemporaryUpgradeEffect.CrossCooldown
            or TemporaryUpgradeEffect.LightningDamage
            or TemporaryUpgradeEffect.LightningCount
            or TemporaryUpgradeEffect.LightningFrequency
            or TemporaryUpgradeEffect.LightningChainCount
            or TemporaryUpgradeEffect.BibleDamage
            or TemporaryUpgradeEffect.BibleCount
            or TemporaryUpgradeEffect.BibleOrbitSpeed
            or TemporaryUpgradeEffect.BibleRadius
            or TemporaryUpgradeEffect.OrbDamage
            or TemporaryUpgradeEffect.OrbCount
            or TemporaryUpgradeEffect.OrbSpeed
            or TemporaryUpgradeEffect.FireDamage
            or TemporaryUpgradeEffect.FireArea
            or TemporaryUpgradeEffect.FireDuration
            or TemporaryUpgradeEffect.FireFrequency
            or TemporaryUpgradeEffect.SpiritWaterDamage
            or TemporaryUpgradeEffect.SpiritWaterDuration
            or TemporaryUpgradeEffect.SpiritWaterCooldown
            or TemporaryUpgradeEffect.LifestealDamage
            or TemporaryUpgradeEffect.LifestealCooldown
            or TemporaryUpgradeEffect.FusionDamage
            or TemporaryUpgradeEffect.FusionArea
            or TemporaryUpgradeEffect.FusionFrequency))
        {
            return true;
        }

        if (upgrade.Effect is TemporaryUpgradeEffect.CrossDamage
            or TemporaryUpgradeEffect.CrossSize
            or TemporaryUpgradeEffect.CrossCooldown)
        {
            return IsWeaponUnlocked("Cross");
        }

        if (upgrade.Effect is TemporaryUpgradeEffect.LightningDamage
            or TemporaryUpgradeEffect.LightningCount
            or TemporaryUpgradeEffect.LightningFrequency
            or TemporaryUpgradeEffect.LightningChainCount)
        {
            return IsWeaponUnlocked("Lightning");
        }

        if (upgrade.Effect is TemporaryUpgradeEffect.FusionDamage
            or TemporaryUpgradeEffect.FusionArea
            or TemporaryUpgradeEffect.FusionFrequency)
        {
            return _fusionStates.Values.Any(state => state.Active);
        }

        BibleAttack bibleAttack = Player.GetNodeOrNull<BibleAttack>("BibleAttack");
        if (upgrade.Effect is TemporaryUpgradeEffect.BibleDamage
            or TemporaryUpgradeEffect.BibleCount
            or TemporaryUpgradeEffect.BibleOrbitSpeed
            or TemporaryUpgradeEffect.BibleRadius)
        {
            return IsWeaponUnlocked("Bible");
        }

        if (upgrade.Effect is TemporaryUpgradeEffect.OrbDamage
            or TemporaryUpgradeEffect.OrbCount
            or TemporaryUpgradeEffect.OrbSpeed)
        {
            return IsWeaponUnlocked("Orb");
        }

        GroundFireAttack fire = Player.GetNodeOrNull<GroundFireAttack>("GroundFire");
        if (upgrade.Effect is TemporaryUpgradeEffect.FireDamage
            or TemporaryUpgradeEffect.FireArea
            or TemporaryUpgradeEffect.FireDuration
            or TemporaryUpgradeEffect.FireFrequency)
        {
            return IsWeaponUnlocked("Fire");
        }

        SpiritWater spiritWater = Player.GetNodeOrNull<SpiritWater>("SpiritWater");
        if (upgrade.Effect is TemporaryUpgradeEffect.SpiritWaterDamage
            or TemporaryUpgradeEffect.SpiritWaterDuration
            or TemporaryUpgradeEffect.SpiritWaterCooldown)
        {
            return IsWeaponUnlocked("SpiritWater");
        }

        return IsWeaponUnlocked("Lifesteal");
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

    }

    private bool TryApplyTemporaryUpgrade(TemporaryUpgradeDefinition upgrade)
    {
        if (upgrade == null || !_temporaryUpgradeApplications.ContainsKey(upgrade.Id)) return false;

        if (Player is ITemporaryUpgradeReceiver playerReceiver
            && playerReceiver.TryApplyTemporaryUpgrade(upgrade))
        {
            _temporaryUpgradeApplications[upgrade.Id]++;
            RecordWeaponUpgrade(upgrade);
            RecordFusionUpgrade(upgrade);
            RecordPassiveUpgrade(upgrade);
            TryActivateAvailableFusions();
            _combatHud?.Refresh(Player);
            _combatHud?.PulseUpgrade(upgrade);
            return true;
        }

        foreach (Node child in Player.GetChildren())
        {
            if (child is not ITemporaryUpgradeReceiver receiver || !receiver.TryApplyTemporaryUpgrade(upgrade)) continue;

            _temporaryUpgradeApplications[upgrade.Id]++;
            RecordWeaponUpgrade(upgrade);
            RecordFusionUpgrade(upgrade);
            RecordPassiveUpgrade(upgrade);
            TryActivateAvailableFusions();
            _combatHud?.Refresh(Player);
            _combatHud?.PulseUpgrade(upgrade);
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
        ClearCombatEntities();
        _metaProgressionState.AddFaith(50);
        MetaProgressionSave.Save(_metaProgressionState);
        RunCompleted?.Invoke();
        GetTree().CreateTimer(0.2f, true, false, true).Timeout += FinishRun;
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

    private void ClearCombatEntities()
    {
        _enemyManager.ClearEnemies();
        foreach (Node child in GetNode<Node3D>("/root/MainScene").GetChildren())
        {
            if (child is Enemy or ExperiencePickup or FaithSurgePickup or RigidBody3D)
            {
                child.QueueFree();
            }
        }

        if (Player == null) return;
        foreach (Node child in Player.GetChildren()) StopCombatNode(child);
    }

    private static void StopCombatNode(Node node)
    {
        node.SetProcess(false);
        node.SetPhysicsProcess(false);
        if (node is Timer timer) timer.Stop();
        foreach (Node child in node.GetChildren()) StopCombatNode(child);
    }

    private void BindHud()
    {
        _gameTimeLabel = GetNodeOrNull<Label>("/root/MainScene/HUD/GameTime");
        _playerXpBar = GetNodeOrNull<ProgressBar>("/root/MainScene/HUD/PlayerXPBar");
        _combatHud = GetNodeOrNull<CombatHud>("/root/MainScene/HUD");
        _combatHud?.BindPlayer(_player);

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

        _weaponStates.Clear();
        _passiveStates.Clear();
        _fusionStates.Clear();
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

    private bool IsWeaponUnlocked(string weaponId)
    {
        return _weaponStates.TryGetValue(weaponId, out WeaponRuntimeState state) && state.IsUnlocked;
    }

    public int GetPassiveLevel(string passiveId)
    {
        return _passiveStates.TryGetValue(passiveId, out PassiveRuntimeState state) ? state.Level : 0;
    }

    private void OnPlayerBuildChanged(Player player)
    {
        EnsureWeaponStates();
        TryActivateAvailableFusions();
        _combatHud?.Refresh(player);
    }

    private void EnsureWeaponStates()
    {
        string[] weaponIds = { "HolyLight", "Cross", "Lightning", "Bible", "Orb", "Fire", "SpiritWater", "Lifesteal" };
        foreach (string weaponId in weaponIds)
        {
            if (!_weaponStates.ContainsKey(weaponId)) _weaponStates.Add(weaponId, new WeaponRuntimeState(weaponId));
        }

        foreach (FusionDefinition fusion in _fusionDefinitions)
        {
            if (!_fusionStates.ContainsKey(fusion.Id))
            {
                _fusionStates.Add(fusion.Id, new FusionRuntimeState(fusion.Id, fusion.ResultWeaponId));
            }
        }

        if (Player == null) return;
        _weaponStates["HolyLight"].Unlock();
        if (Player.GetNodeOrNull<CrossAttack>("CrossAttack")?.IsUnlocked == true) _weaponStates["Cross"].Unlock();
        if (Player.GetNodeOrNull<LightningAttack>("LightningAttack")?.IsUnlocked == true) _weaponStates["Lightning"].Unlock();
        if (Player.GetNodeOrNull<BibleAttack>("BibleAttack")?.IsUnlocked == true) _weaponStates["Bible"].Unlock();
        if (Player.GetNodeOrNull<FloatingSphereAttack>("FloatingSphere")?.IsUnlocked == true) _weaponStates["Orb"].Unlock();
        if (Player.GetNodeOrNull<GroundFireAttack>("GroundFire")?.IsUnlocked == true) _weaponStates["Fire"].Unlock();
        if (Player.GetNodeOrNull<SpiritWater>("SpiritWater")?.IsUnlocked == true) _weaponStates["SpiritWater"].Unlock();
        if (Player.GetNodeOrNull<LifestealAttack>("Lifesteal")?.IsUnlocked == true) _weaponStates["Lifesteal"].Unlock();
    }

    private void RecordWeaponUpgrade(TemporaryUpgradeDefinition upgrade)
    {
        string weaponId = GetWeaponIdForUpgrade(upgrade?.Effect);
        if (!string.IsNullOrEmpty(weaponId) && _weaponStates.TryGetValue(weaponId, out WeaponRuntimeState state))
        {
            state.RecordUpgrade();
        }
    }

    private void RecordFusionUpgrade(TemporaryUpgradeDefinition upgrade)
    {
        if (upgrade?.Effect is not (TemporaryUpgradeEffect.FusionDamage
            or TemporaryUpgradeEffect.FusionArea
            or TemporaryUpgradeEffect.FusionFrequency)) return;

        foreach (FusionRuntimeState state in _fusionStates.Values)
        {
            if (!state.Active) continue;
            state.RecordUpgrade();
            break;
        }
    }

    private void RecordPassiveUpgrade(TemporaryUpgradeDefinition upgrade)
    {
        if (upgrade == null || !_passiveStates.TryGetValue(upgrade.Id, out PassiveRuntimeState state)) return;
        state.Apply(upgrade.Amount);
    }

    private void TryActivateAvailableFusions()
    {
        foreach (FusionDefinition fusion in _fusionDefinitions)
        {
            if (_fusionStates[fusion.Id].Active || !CanFuse(fusion)) continue;

            int sourceLevel = _weaponStates[fusion.WeaponAId].Level + _weaponStates[fusion.WeaponBId].Level;
            int sourceContribution = (int)_weaponStates[fusion.WeaponAId].UpgradeApplications
                + (int)_weaponStates[fusion.WeaponBId].UpgradeApplications;
            _weaponStates[fusion.WeaponAId].Deactivate();
            _weaponStates[fusion.WeaponBId].Deactivate();
            _fusionStates[fusion.Id].Activate();
            _fusionStates[fusion.Id].ActivateWithLevel(sourceLevel);
            StopWeaponNode(fusion.WeaponAId);
            StopWeaponNode(fusion.WeaponBId);

            FusionAttack attack = new FusionAttack();
            attack.Configure(fusion, sourceLevel, sourceContribution);
            Player.AddChild(attack);
            ShowEvolutionFeedback(fusion.DisplayName);
        }
    }

    private void StopWeaponNode(string weaponId)
    {
        string nodePath = weaponId switch
        {
            "HolyLight" => "Shooting",
            "Cross" => "CrossAttack",
            "Lightning" => "LightningAttack",
            "Bible" => "BibleAttack",
            "Orb" => "FloatingSphere",
            "Fire" => "GroundFire",
            "SpiritWater" => "SpiritWater",
            "Lifesteal" => "Lifesteal",
            _ => string.Empty,
        };
        Node node = Player?.GetNodeOrNull<Node>(nodePath);
        if (node != null) StopCombatNode(node);
    }

    private void ShowEvolutionFeedback(string name)
    {
        Label feedback = new Label
        {
            Text = $"EVOLUTION\n{name}",
            ProcessMode = ProcessModeEnum.Always,
            ZIndex = 8,
            Modulate = new Color(1f, 0.85f, 0.25f, 0f),
        };
        feedback.Position = new Vector2(440, 260);
        feedback.AddThemeFontSizeOverride("font_size", 42);
        GetNode<Control>("/root/MainScene/HUD").AddChild(feedback);
        Tween tween = feedback.CreateTween();
        tween.TweenProperty(feedback, "modulate", Colors.White, 0.1f);
        tween.TweenInterval(0.8f);
        tween.TweenProperty(feedback, "modulate", new Color(1f, 1f, 1f, 0f), 0.2f);
        tween.TweenCallback(Callable.From(feedback.QueueFree));

        ColorRect flash = GetNodeOrNull<ColorRect>("/root/MainScene/HUD/PlayerDamageFlash");
        if (flash != null)
        {
            Color originalColor = flash.Color;
            flash.Color = new Color(1f, 0.78f, 0.2f, 0.35f);
            Tween flashTween = flash.CreateTween();
            flashTween.TweenProperty(flash, "color", originalColor, 0.35f);
        }
    }

    private static string GetWeaponIdForUpgrade(TemporaryUpgradeEffect? effect) => effect switch
    {
        TemporaryUpgradeEffect.ProjectileDamage or TemporaryUpgradeEffect.ProjectileAttackSpeed
            or TemporaryUpgradeEffect.ProjectileSpeed or TemporaryUpgradeEffect.ProjectileCount
            or TemporaryUpgradeEffect.ProjectileSpread or TemporaryUpgradeEffect.ProjectileSize
            or TemporaryUpgradeEffect.ProjectileCountDouble or TemporaryUpgradeEffect.ProjectileDamagePercent
            or TemporaryUpgradeEffect.ProjectileAttackSpeedPercent => "HolyLight",
        TemporaryUpgradeEffect.CrossDamage or TemporaryUpgradeEffect.CrossSize or TemporaryUpgradeEffect.CrossCooldown => "Cross",
        TemporaryUpgradeEffect.LightningDamage or TemporaryUpgradeEffect.LightningCount
            or TemporaryUpgradeEffect.LightningFrequency or TemporaryUpgradeEffect.LightningChainCount => "Lightning",
        TemporaryUpgradeEffect.BibleDamage or TemporaryUpgradeEffect.BibleCount
            or TemporaryUpgradeEffect.BibleOrbitSpeed or TemporaryUpgradeEffect.BibleRadius => "Bible",
        TemporaryUpgradeEffect.OrbDamage or TemporaryUpgradeEffect.OrbCount or TemporaryUpgradeEffect.OrbSpeed => "Orb",
        TemporaryUpgradeEffect.FireDamage or TemporaryUpgradeEffect.FireArea
            or TemporaryUpgradeEffect.FireDuration or TemporaryUpgradeEffect.FireFrequency => "Fire",
        TemporaryUpgradeEffect.SpiritWaterDamage or TemporaryUpgradeEffect.SpiritWaterDuration
            or TemporaryUpgradeEffect.SpiritWaterCooldown => "SpiritWater",
        TemporaryUpgradeEffect.LifestealDamage or TemporaryUpgradeEffect.LifestealCooldown => "Lifesteal",
        _ => string.Empty,
    };

    public Vector3 GetRandomPosAroundPlayer(float range) => Player.Position + range * new Vector3(
            (float)GD.RandRange(-1f, 1f),
            0,
            (float)GD.RandRange(-1f, 1f)
            ).Normalized();

    internal Enemy GetNearestEnemy() => _enemyManager.Enemies
        .OrderBy(enemy => (Player.Position - enemy.Position).Length())
        .FirstOrDefault();

    internal IEnumerable<Enemy> GetLivingEnemies() => _enemyManager.Enemies
        .Where(enemy => GodotObject.IsInstanceValid(enemy) && !enemy.IsDead);

    internal int GetMaxEnemyLifepoints(int level) => Mathf.Clamp(
        Mathf.RoundToInt(Math.Log10(level * 10)),
        1,
        2);

    internal int GetMaxEnemyLifepoints() => GetMaxEnemyLifepoints((int)(Player?.CurrentLevel ?? 1));

    public int GetWeaponLevel(string weaponId)
    {
        return _weaponStates.TryGetValue(weaponId, out WeaponRuntimeState state) && state.IsActive
            ? state.Level
            : 0;
    }

    public int FaithCurrency => _metaProgressionState?.FaithCurrency ?? 0;

    public bool CanFuse(FusionDefinition definition)
    {
        if (definition == null) return false;
        return GetWeaponLevel(definition.WeaponAId) >= definition.WeaponARequiredLevel
            && GetWeaponLevel(definition.WeaponBId) >= definition.WeaponBRequiredLevel;
    }

    public IReadOnlyList<FusionDefinition> GetAvailableFusions()
    {
        return _fusionDefinitions.Where(CanFuse).ToArray();
    }

    public int GetFusionLevel(string fusionId)
    {
        return _fusionStates.TryGetValue(fusionId, out FusionRuntimeState state) && state.Active
            ? state.Level
            : 0;
    }

    private bool IsUpgradeForWeapon(string upgradeId, string weaponId)
    {
        TemporaryUpgradeDefinition upgrade = _temporaryUpgrades.FirstOrDefault(item => item.Id == upgradeId);
        if (upgrade == null) return false;

        return weaponId switch
        {
            "HolyLight" => upgrade.Effect is TemporaryUpgradeEffect.ProjectileDamage
                or TemporaryUpgradeEffect.ProjectileAttackSpeed or TemporaryUpgradeEffect.ProjectileSpeed
                or TemporaryUpgradeEffect.ProjectileCount or TemporaryUpgradeEffect.ProjectileSpread
                or TemporaryUpgradeEffect.ProjectileSize or TemporaryUpgradeEffect.ProjectileCountDouble
                or TemporaryUpgradeEffect.ProjectileDamagePercent or TemporaryUpgradeEffect.ProjectileAttackSpeedPercent,
            "Cross" => upgrade.Effect is TemporaryUpgradeEffect.CrossDamage or TemporaryUpgradeEffect.CrossSize
                or TemporaryUpgradeEffect.CrossCooldown,
            "Lightning" => upgrade.Effect is TemporaryUpgradeEffect.LightningDamage or TemporaryUpgradeEffect.LightningCount
                or TemporaryUpgradeEffect.LightningFrequency or TemporaryUpgradeEffect.LightningChainCount,
            "Bible" => upgrade.Effect is TemporaryUpgradeEffect.BibleDamage or TemporaryUpgradeEffect.BibleCount
                or TemporaryUpgradeEffect.BibleOrbitSpeed or TemporaryUpgradeEffect.BibleRadius,
            "Orb" => upgrade.Effect is TemporaryUpgradeEffect.OrbDamage or TemporaryUpgradeEffect.OrbCount
                or TemporaryUpgradeEffect.OrbSpeed,
            "Fire" => upgrade.Effect is TemporaryUpgradeEffect.FireDamage or TemporaryUpgradeEffect.FireArea
                or TemporaryUpgradeEffect.FireDuration or TemporaryUpgradeEffect.FireFrequency,
            "SpiritWater" => upgrade.Effect is TemporaryUpgradeEffect.SpiritWaterDamage
                or TemporaryUpgradeEffect.SpiritWaterDuration or TemporaryUpgradeEffect.SpiritWaterCooldown,
            "Lifesteal" => upgrade.Effect is TemporaryUpgradeEffect.LifestealDamage
                or TemporaryUpgradeEffect.LifestealCooldown,
            _ => false,
        };
    }
}
