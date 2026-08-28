using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class FusionAttack : Node3D, ITemporaryUpgradeReceiver
{
    private FusionDefinition _definition;
    private GameManager _gameManager;
    private Area3D _area;
    private MeshInstance3D _visual;
    private Timer _timer;
    private readonly List<MeshInstance3D> _chainVisuals = new();
    private uint _damageBonus;
    private float _radiusBonus;
    private float _frequencyReduction;
    private int _level = 1;
    private int _sourceUpgradeContribution;

    public void Configure(FusionDefinition definition, int sourceLevel, int sourceUpgradeContribution)
    {
        _definition = definition;
        _level = Mathf.Max(1, sourceLevel);
        _sourceUpgradeContribution = sourceUpgradeContribution;
        _radiusBonus = sourceUpgradeContribution * 0.12f;
        _frequencyReduction = sourceUpgradeContribution * 0.04f;
    }

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
        _area = new Area3D { CollisionLayer = 0, CollisionMask = 2, Monitoring = true };
        _area.AddChild(new CollisionShape3D { Shape = new SphereShape3D { Radius = GetRadius() } });
        AddChild(_area);
        _visual = new MeshInstance3D
        {
            Visible = false,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            Mesh = new CylinderMesh { TopRadius = GetRadius(), BottomRadius = GetRadius(), Height = 0.12f },
            MaterialOverride = new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                AlbedoColor = GetColor(),
                EmissionEnabled = true,
                Emission = GetColor(),
                EmissionEnergyMultiplier = 2.5f,
            },
        };
        AddChild(_visual);
        if (_definition.ResultWeaponId == "Thunder Cross")
        {
            for (int index = 0; index < 4; index++)
            {
                MeshInstance3D chain = new MeshInstance3D
                {
                    Visible = false,
                    CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
                    Mesh = new CylinderMesh { TopRadius = 0.14f, BottomRadius = 0.24f, Height = 1f },
                    MaterialOverride = new StandardMaterial3D
                    {
                        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                        AlbedoColor = new Color(0.4f, 0.8f, 1f),
                        EmissionEnabled = true,
                        Emission = new Color(0.1f, 0.5f, 1f),
                        EmissionEnergyMultiplier = 3f,
                    },
                };
                AddChild(chain);
                _chainVisuals.Add(chain);
            }
        }
        _timer = new Timer { OneShot = false, WaitTime = GetCooldown() };
        _timer.Timeout += Trigger;
        AddChild(_timer);
        _timer.Start();
        Trigger();
    }

    private void Trigger()
    {
        if (_gameManager.Player == null || _gameManager.Player.IsDead) return;

        uint damage = _definition.ResultWeaponId switch
        {
            "Thunder Cross" => 22u + _damageBonus,
            "Living Spring" => 12u + _damageBonus,
            "Covenant Guard" => 14u + _damageBonus,
            _ => 18u + _damageBonus,
        };
        damage += (uint)Mathf.Max(0, (_level - 1) * 3 + _sourceUpgradeContribution);

        List<Enemy> areaTargets = _area.GetOverlappingBodies().OfType<Enemy>()
            .Where(enemy => !enemy.IsDead)
            .ToList();
        foreach (Enemy enemy in areaTargets)
        {
            enemy.TakeDamages(damage);
            if (_definition.ResultWeaponId == "Covenant Guard") _gameManager.Player.Heal(2);
        }

        if (_definition.ResultWeaponId == "Thunder Cross")
        {
            List<Enemy> chainTargets = _gameManager.GetLivingEnemies()
                .OrderBy(enemy => _gameManager.Player.GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition))
                .Take(Mathf.Min(4, 1 + _sourceUpgradeContribution / 2))
                .ToList();
            foreach (Enemy enemy in chainTargets)
            {
                enemy.TakeDamages(damage);
            }
            for (int index = 0; index < _chainVisuals.Count; index++)
            {
                MeshInstance3D chain = _chainVisuals[index];
                if (index + 1 >= chainTargets.Count)
                {
                    chain.Visible = false;
                    continue;
                }

                ShowChain(chain, chainTargets[index].GlobalPosition, chainTargets[index + 1].GlobalPosition);
                chain.Visible = true;
            }
            GetTree().CreateTimer(0.18f).Timeout += HideChains;
        }

        if (_definition.ResultWeaponId == "Radiant Gospel")
        {
            foreach (Enemy enemy in _gameManager.GetLivingEnemies()
                .OrderBy(enemy => _gameManager.Player.GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition))
                .Take(Mathf.Min(6, 3 + _sourceUpgradeContribution / 2)))
            {
                enemy.TakeDamages(damage);
            }
        }

        _visual.Visible = true;
        _visual.Scale = Vector3.One * 0.65f;
        Tween feedback = CreateTween();
        feedback.TweenProperty(_visual, "scale", Vector3.One * 1.2f, 0.08f);
        feedback.TweenProperty(_visual, "scale", Vector3.Zero, 0.14f);
        feedback.TweenCallback(Callable.From(() => _visual.Visible = false));
    }

    private void ShowChain(MeshInstance3D chain, Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        chain.GlobalPosition = (start + end) * 0.5f;
        chain.LookAt(end, Vector3.Up);
        chain.RotateObjectLocal(Vector3.Right, Mathf.Pi * 0.5f);
        chain.Scale = new Vector3(1f, 1f, direction.Length());
    }

    private void HideChains()
    {
        foreach (MeshInstance3D chain in _chainVisuals) chain.Visible = false;
    }

    private float GetRadius() => _definition?.ResultWeaponId switch
    {
        "Thunder Cross" => 4.5f + _radiusBonus,
        "Living Spring" => 6f + _radiusBonus,
        "Covenant Guard" => 4f + _radiusBonus,
        _ => 5f + _radiusBonus,
    };

    private Color GetColor() => _definition?.ResultWeaponId switch
    {
        "Thunder Cross" => new Color(0.45f, 0.8f, 1f),
        "Living Spring" => new Color(0.2f, 0.85f, 0.8f),
        "Covenant Guard" => new Color(0.85f, 0.2f, 0.35f),
        _ => new Color(1f, 0.75f, 0.25f),
    };

    private float GetCooldown() => Mathf.Max(0.35f, 1.25f - _frequencyReduction);

    public bool TryApplyTemporaryUpgrade(TemporaryUpgradeDefinition upgrade)
    {
        if (upgrade == null || upgrade.Amount <= 0 || !float.IsFinite(upgrade.Amount)) return false;

        switch (upgrade.Effect)
        {
            case TemporaryUpgradeEffect.FusionDamage:
                _damageBonus += (uint)Mathf.Max(1, Mathf.FloorToInt(upgrade.Amount));
                _level++;
                return true;
            case TemporaryUpgradeEffect.FusionArea:
                _radiusBonus += upgrade.Amount;
                _level++;
                if (_area?.GetChild(0) is CollisionShape3D shape && shape.Shape is SphereShape3D sphere)
                {
                    sphere.Radius = GetRadius();
                }
                if (_visual?.Mesh is CylinderMesh cylinder)
                {
                    cylinder.TopRadius = GetRadius();
                    cylinder.BottomRadius = GetRadius();
                }
                return true;
            case TemporaryUpgradeEffect.FusionFrequency:
                _frequencyReduction += upgrade.Amount;
                _level++;
                if (_timer != null) _timer.WaitTime = GetCooldown();
                return true;
            default:
                return false;
        }
    }
}