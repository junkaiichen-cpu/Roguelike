using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class LightningAttack : Node3D, ITemporaryUpgradeReceiver
{
    [Export]
    public uint BaseDamage { get; set; } = 14;

    [Export]
    public float CooldownSeconds { get; set; } = 3f;

    private uint _damageBonus;
    private int _countBonus;
    private float _frequencyReduction;
    private bool _unlocked;
    private GameManager _gameManager;
    private Timer _timer;
    private MeshInstance3D _strikeVisual;
    private readonly List<MeshInstance3D> _chainVisuals = new();
    private int _chainCountBonus;

    public int TotalCount => 1 + _countBonus;
    public int TotalChainCount => Mathf.Min(5, 2 + _chainCountBonus);

    public bool IsUnlocked => _unlocked;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
        _strikeVisual = new MeshInstance3D
        {
            Visible = false,
            Mesh = new CylinderMesh
            {
                TopRadius = 0.55f,
                BottomRadius = 0.9f,
                Height = 4.5f,
            },
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            MaterialOverride = new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                AlbedoColor = new Color(0.55f, 0.8f, 1f),
                EmissionEnabled = true,
                Emission = new Color(0.3f, 0.65f, 1f),
                EmissionEnergyMultiplier = 3f,
            },
        };
        AddChild(_strikeVisual);
        for (int index = 0; index < 4; index++)
        {
            MeshInstance3D chain = new()
            {
                Visible = false,
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
                Mesh = new CylinderMesh { TopRadius = 0.12f, BottomRadius = 0.2f, Height = 1f },
                MaterialOverride = new StandardMaterial3D
                {
                    ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                    AlbedoColor = new Color(0.35f, 0.75f, 1f),
                    EmissionEnabled = true,
                    Emission = new Color(0.2f, 0.55f, 1f),
                    EmissionEnergyMultiplier = 3f,
                },
            };
            AddChild(chain);
            _chainVisuals.Add(chain);
        }

        _timer = new Timer { OneShot = true, WaitTime = CooldownSeconds };
        _timer.Timeout += Strike;
        AddChild(_timer);
    }

    private void Strike()
    {
        if (!_unlocked) return;

        _timer.Start(Mathf.Max(0.25f, CooldownSeconds - _frequencyReduction));
        var targets = _gameManager.GetLivingEnemies()
            .OrderBy(enemy => _gameManager.Player.GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition))
            .Take(Mathf.Max(TotalCount, TotalChainCount))
            .ToList();
        if (targets.Count == 0) return;

        foreach (Enemy enemy in targets.Take(TotalCount))
        {
            enemy.TakeDamages(BaseDamage + _damageBonus);
        }

        _strikeVisual.GlobalPosition = targets[0].GlobalPosition + new Vector3(0, 0.9f, 0);
        _strikeVisual.Visible = true;
        _strikeVisual.Scale = Vector3.One * 0.3f;
        Tween strikeTween = CreateTween();
        strikeTween.TweenProperty(_strikeVisual, "scale", Vector3.One, 0.07f);
        strikeTween.TweenProperty(_strikeVisual, "scale", Vector3.Zero, 0.1f);
        strikeTween.TweenCallback(Callable.From(() => _strikeVisual.Visible = false));

        for (int index = 0; index < _chainVisuals.Count; index++)
        {
            MeshInstance3D chain = _chainVisuals[index];
            if (index + 1 >= Mathf.Min(TotalChainCount, targets.Count))
            {
                chain.Visible = false;
                continue;
            }

            ShowChain(chain, targets[index].GlobalPosition, targets[index + 1].GlobalPosition);
            chain.Visible = true;
        }

        GetTree().CreateTimer(0.16).Timeout += HideChains;
    }

    public bool TryApplyTemporaryUpgrade(TemporaryUpgradeDefinition upgrade)
    {
        if (upgrade == null || upgrade.Amount <= 0 || !float.IsFinite(upgrade.Amount)) return false;

        switch (upgrade.Effect)
        {
            case TemporaryUpgradeEffect.UnlockLightning:
                _unlocked = true;
                _timer.Start();
                return true;
            case TemporaryUpgradeEffect.LightningDamage:
                if (!_unlocked) return false;
                _damageBonus += (uint)Mathf.Max(1, Mathf.FloorToInt(upgrade.Amount));
                return true;
            case TemporaryUpgradeEffect.LightningCount:
                if (!_unlocked) return false;
                _countBonus = Mathf.Min(3, _countBonus + Mathf.Max(1, Mathf.FloorToInt(upgrade.Amount)));
                return true;
            case TemporaryUpgradeEffect.LightningFrequency:
                if (!_unlocked) return false;
                _frequencyReduction += upgrade.Amount;
                return true;
            case TemporaryUpgradeEffect.LightningChainCount:
                if (!_unlocked) return false;
                _chainCountBonus = Mathf.Min(3, _chainCountBonus + Mathf.Max(1, Mathf.FloorToInt(upgrade.Amount)));
                return true;
            default:
                return false;
        }
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
}
