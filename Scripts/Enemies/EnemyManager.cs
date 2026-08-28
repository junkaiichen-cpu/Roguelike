using Godot;
using System;
using System.Collections.Generic;

internal class EnemyManager
{
    private const float SpawnVisibilityMarginPixels = 48f;

    public float SpawnRate { get; private set; } = 1;

    public float SpawnDelay => 1f / SpawnRate;

    private readonly GameManager _gameManager;
    private readonly EnemySpawnConfiguration _spawnConfiguration;
    private readonly Dictionary<EnemyClass, PackedScene> _enemyPrefabs;
    private readonly List<Enemy> _enemies = new();
    public List<Enemy> Enemies => _enemies;

    // BONUSES
    private List<EnemyClass> _enemyClasses = new() { EnemyClass.Minion };
    private int _lifepointsBonus = 0;
    private uint _damageBonus = 0;
    private float _movespeedBonus = 0;

    public EnemyManager(GameManager gameManager, EnemySpawnConfiguration spawnConfiguration)
    {
        _gameManager = gameManager;
        _spawnConfiguration = spawnConfiguration ?? throw new System.ArgumentNullException(nameof(spawnConfiguration));

        _enemyPrefabs = new()
        {
            { EnemyClass.Minion, (PackedScene)GD.Load("res://Prefabs/Enemies/enemy_minion.tscn") },
            { EnemyClass.Warrior, (PackedScene)GD.Load("res://Prefabs/Enemies/enemy_warrior.tscn") },
            { EnemyClass.Archer, (PackedScene)GD.Load("res://Prefabs/Enemies/enemy_archer.tscn") },
            { EnemyClass.Mage, (PackedScene)GD.Load("res://Prefabs/Enemies/enemy_mage.tscn") },
            { EnemyClass.Boss, (PackedScene)GD.Load("res://Prefabs/Enemies/enemy_boss.tscn") },
        };
    }

    internal Enemy SpawnEnemy() => SpawnEnemy(_enemyClasses[GD.RandRange(0, _enemyClasses.Count - 1)]);

    internal Enemy SpawnEnemy(EnemyClass enemyClass)
    {
        return SpawnEnemy(_enemyPrefabs[enemyClass], enemyClass.ToString(), showSpawnWarning: true, EliteType.None);
    }

    internal Enemy SpawnElite()
    {
        EliteType eliteType = (EliteType)GD.RandRange(1, 5);
        return SpawnEnemy(_enemyPrefabs[EnemyClass.Minion], eliteType.ToString(), showSpawnWarning: true, eliteType);
    }

    internal Enemy SpawnBoss(BossDefinition definition)
    {
        if (definition == null || definition.EnemyScene == null)
        {
            GD.PushError("A boss definition requires an enemy scene.");
            return null;
        }

        return SpawnEnemy(definition.EnemyScene, definition.Id, showSpawnWarning: false, EliteType.None);
    }

    private Enemy SpawnEnemy(PackedScene enemyScene, string enemyName, bool showSpawnWarning, EliteType eliteType)
    {
        var enemy = enemyScene.Instantiate<Enemy>();
        enemy.Name = enemyName;
        enemy.ConfigureElite(eliteType);
        enemy.MaxHealth = GetSpawnHealth(enemy.MaxHealth);
        enemy.Damages += _damageBonus;
        enemy.MovementSpeed += _movespeedBonus;
        Vector3 spawnPosition = GetRandomPos(useNormalSafety: showSpawnWarning);
        enemy.Died += OnEnemyDied;
        _gameManager.GetNode("/root/MainScene").AddChild(enemy);
        enemy.GlobalPosition = spawnPosition;
        if (showSpawnWarning)
        {
            enemy.PlaySpawnFeedback();
        }
        _enemies.Add(enemy);
        enemy.Connect(Enemy.SignalName.OnEnemyHit, Callable.From<Enemy, int>(_gameManager.EnemyHit));

        return enemy;
    }

    internal Enemy SpawnBoss() => SpawnEnemy(_enemyPrefabs[EnemyClass.Boss], EnemyClass.Boss.ToString(), showSpawnWarning: false, EliteType.None);

    internal void ClearEnemies()
    {
        foreach (Enemy enemy in _enemies)
        {
            if (GodotObject.IsInstanceValid(enemy))
            {
                enemy.QueueFree();
            }
        }

        _enemies.Clear();
    }

    private void OnEnemyDied(Enemy enemy)
    {
        if (!_enemies.Remove(enemy)) return;

            uint experience = enemy.IsElite
                ? (uint)Math.Clamp((long)enemy.ExperienceReward * 20, 1L, uint.MaxValue)
                : enemy.ExperienceReward;
            _gameManager.SpawnExperiencePickup(enemy.GlobalPosition, experience);
            if (enemy.IsElite && GD.Randf() < 0.15f)
            {
                _gameManager.SpawnFaithSurge(enemy.GlobalPosition);
            }
    }

    private Vector3 GetRandomPos(bool useNormalSafety)
    {
        Vector3 playerPosition = _gameManager.Player.GlobalPosition;
        Camera3D camera = _gameManager.Player.GetNodeOrNull<Camera3D>("Camera3D");
        bool canValidateVisibility = camera != null && camera.IsInsideTree() && camera.GetViewport() != null;

        float minimumDistance = useNormalSafety
            ? _spawnConfiguration.NormalMinimumDistance
            : _spawnConfiguration.MinimumDistance;
        int maximumAttempts = useNormalSafety
            ? _spawnConfiguration.NormalMaximumAttempts
            : _spawnConfiguration.MaximumAttempts;

        for (int attempt = 0; attempt < maximumAttempts; attempt++)
        {
            float angle = GD.Randf() * Mathf.Tau;
            float distance = Mathf.Lerp(
                minimumDistance,
                _spawnConfiguration.MaximumDistance,
                GD.Randf());
            Vector3 candidate = playerPosition + new Vector3(
                Mathf.Cos(angle) * distance,
                0,
                Mathf.Sin(angle) * distance);

                if (!EnemySpawnPlacement.IsWithinSpawnBand(
                    ToNumericsVector3(playerPosition),
                    ToNumericsVector3(candidate),
                    _spawnConfiguration,
                    minimumDistance,
                    _spawnConfiguration.Bounds))
            {
                continue;
            }

            if (!canValidateVisibility
                || !IsCameraVisible(camera, candidate, useNormalSafety ? 96f : SpawnVisibilityMarginPixels)
                || attempt >= 3)
            {
                return candidate;
            }
        }

        Vector3 fallback = ToGodotVector3(EnemySpawnPlacement.GetFallbackPosition(
            ToNumericsVector3(playerPosition),
            _spawnConfiguration,
            minimumDistance,
            _spawnConfiguration.Bounds));
        return fallback;
    }

    private static bool IsCameraVisible(Camera3D camera, Vector3 worldPosition, float marginPixels)
    {
        if (camera == null || !camera.IsInsideTree() || camera.GetViewport() == null)
        {
            return false;
        }

        if (camera.IsPositionBehind(worldPosition))
        {
            return false;
        }

        Rect2 visibleRect = camera.GetViewport().GetVisibleRect().Grow(marginPixels);
        return visibleRect.HasPoint(camera.UnprojectPosition(worldPosition));
    }

    private static System.Numerics.Vector3 ToNumericsVector3(Vector3 vector) => new(vector.X, vector.Y, vector.Z);

    private static Vector3 ToGodotVector3(System.Numerics.Vector3 vector) => new(vector.X, vector.Y, vector.Z);

    private uint GetSpawnHealth(uint baseHealth)
    {
        long scaledHealth = (long)baseHealth * _gameManager.GetMaxEnemyLifepoints() + _lifepointsBonus;
        return (uint)System.Math.Clamp(scaledHealth, 1L, (long)uint.MaxValue);
    }

    internal void Upgrade(EnemyPowerup enemyPowerup)
    {
        switch (enemyPowerup.Type)
        {
            case EnemyPowerupType.UnlockClassWarrior:
                _enemyClasses.Add(EnemyClass.Warrior);
                break;
            case EnemyPowerupType.UnlockClassMage:
                _enemyClasses.Add(EnemyClass.Mage);
                break;
            case EnemyPowerupType.UnlockClassArcher:
                _enemyClasses.Add(EnemyClass.Archer);
                break;
            case EnemyPowerupType.BossSpawn:
                SpawnBoss();
                break;
            case EnemyPowerupType.Lifepoints:
                _lifepointsBonus += (int)((StatEnemyPowerup)enemyPowerup).Value;
                break;
            case EnemyPowerupType.Damages:
                _damageBonus += (uint)((StatEnemyPowerup)enemyPowerup).Value;
                break;
            case EnemyPowerupType.Movespeed:
                _movespeedBonus += ((StatEnemyPowerup)enemyPowerup).Value;
                break;
            case EnemyPowerupType.SpawnRate:
                SpawnRate += ((StatEnemyPowerup)enemyPowerup).Value;
                break;
            default:
                GD.PrintErr($"{enemyPowerup.Type} is not handled");
                break;
        }
    }

    internal double GetFinalValue(EnemyPowerup enemyPowerup) => enemyPowerup.Type switch
    {
        EnemyPowerupType.Lifepoints => _lifepointsBonus + (int)((StatEnemyPowerup)enemyPowerup).Value,
        EnemyPowerupType.Damages => _damageBonus + (uint)((StatEnemyPowerup)enemyPowerup).Value,
        EnemyPowerupType.Movespeed => (double)(_movespeedBonus + ((StatEnemyPowerup)enemyPowerup).Value),
        EnemyPowerupType.SpawnRate => (double)(1f / (SpawnRate + ((StatEnemyPowerup)enemyPowerup).Value)),
        _ => default,
    };
}
