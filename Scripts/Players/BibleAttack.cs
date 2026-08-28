using Godot;
using System.Collections.Generic;

public partial class BibleAttack : Node3D, ITemporaryUpgradeReceiver
{
    private const string BibleScenePath = "res://addons/kaykit_skeleton_pack/assets/spellbook.gltf.glb";

    [Export]
    public uint BaseDamage { get; set; } = 7;

    [Export]
    public int BaseCount { get; set; } = 1;

    [Export]
    public float OrbitRadius { get; set; } = 3.2f;

    [Export]
    public float OrbitSpeedDegrees { get; set; } = 110f;

    [Export]
    public float HitCooldownSeconds { get; set; } = 0.65f;

    private readonly List<OrbitBook> _books = new();
    private uint _damageBonus;
    private int _countBonus;
    private float _orbitSpeedBonus;
    private float _radiusBonus;
    private bool _unlocked;
    private float _orbitAngle;

    public bool IsUnlocked => _unlocked;

    public int TotalCount => Mathf.Min(4, BaseCount + _countBonus);

    public float TotalRadius => OrbitRadius + _radiusBonus;

    public override void _Ready()
    {
        RebuildBooks();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_unlocked) return;

        _orbitAngle = Mathf.PosMod(_orbitAngle + (OrbitSpeedDegrees + _orbitSpeedBonus) * (float)delta, 360f);
        for (int index = 0; index < _books.Count; index++)
        {
            OrbitBook book = _books[index];
            float angle = Mathf.DegToRad(_orbitAngle + index * 360f / _books.Count);
            book.Root.Position = new Vector3(
                Mathf.Cos(angle) * TotalRadius,
                1.0f,
                Mathf.Sin(angle) * TotalRadius);
            book.Update(delta, BaseDamage + _damageBonus, HitCooldownSeconds);
        }
    }

    private void RebuildBooks()
    {
        foreach (OrbitBook book in _books)
        {
            book.Root.QueueFree();
        }

        _books.Clear();
        if (!_unlocked) return;

        PackedScene bibleScene = GD.Load<PackedScene>(BibleScenePath);
        for (int index = 0; index < TotalCount; index++)
        {
            Node3D root = new Node3D();
            AddChild(root);

            Area3D area = new Area3D
            {
                CollisionLayer = 0,
                CollisionMask = 2,
                Monitoring = true,
            };
            area.AddChild(new CollisionShape3D
            {
                Shape = new SphereShape3D { Radius = 0.8f },
            });
            root.AddChild(area);

            Node3D visual = bibleScene?.Instantiate<Node3D>();
            if (visual == null)
            {
                visual = new MeshInstance3D
                {
                    Mesh = new BoxMesh { Size = new Vector3(0.7f, 0.12f, 1.0f) },
                    MaterialOverride = new StandardMaterial3D
                    {
                        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                        AlbedoColor = new Color(0.9f, 0.75f, 0.3f),
                    },
                };
            }

            visual.Scale = Vector3.One * 1.25f;
            DisableShadows(visual);
            root.AddChild(visual);
            _books.Add(new OrbitBook(root, area, visual));
        }
    }

    private static void DisableShadows(Node node)
    {
        if (node is GeometryInstance3D geometry)
        {
            geometry.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        }

        foreach (Node child in node.GetChildren())
        {
            DisableShadows(child);
        }
    }

    public bool TryApplyTemporaryUpgrade(TemporaryUpgradeDefinition upgrade)
    {
        if (upgrade == null || upgrade.Amount <= 0 || !float.IsFinite(upgrade.Amount)) return false;

        switch (upgrade.Effect)
        {
            case TemporaryUpgradeEffect.UnlockBible:
                _unlocked = true;
                RebuildBooks();
                return true;
            case TemporaryUpgradeEffect.BibleDamage:
                if (!_unlocked) return false;
                _damageBonus += (uint)Mathf.Max(1, Mathf.FloorToInt(upgrade.Amount));
                return true;
            case TemporaryUpgradeEffect.BibleCount:
                if (!_unlocked || TotalCount >= 4) return false;
                _countBonus = Mathf.Min(4 - BaseCount, _countBonus + Mathf.Max(1, Mathf.FloorToInt(upgrade.Amount)));
                RebuildBooks();
                return true;
            case TemporaryUpgradeEffect.BibleOrbitSpeed:
                if (!_unlocked) return false;
                _orbitSpeedBonus += upgrade.Amount;
                return true;
            case TemporaryUpgradeEffect.BibleRadius:
                if (!_unlocked) return false;
                _radiusBonus += upgrade.Amount;
                return true;
            default:
                return false;
        }
    }

    private sealed class OrbitBook
    {
        private readonly Area3D _area;
        private readonly Node3D _visual;
        private readonly Dictionary<Enemy, double> _cooldowns = new();
        private double _popRemaining;

        public OrbitBook(Node3D root, Area3D area, Node3D visual)
        {
            Root = root;
            _area = area;
            _visual = visual;
        }

        public Node3D Root { get; }

        public void Update(double delta, uint damage, float hitCooldown)
        {
            var expired = new List<Enemy>();
            foreach (Enemy enemy in _cooldowns.Keys)
            {
                _cooldowns[enemy] -= delta;
                if (_cooldowns[enemy] <= 0) expired.Add(enemy);
            }

            foreach (Enemy enemy in expired) _cooldowns.Remove(enemy);
            foreach (Node3D body in _area.GetOverlappingBodies())
            {
                if (body is not Enemy enemy || enemy.IsDead || _cooldowns.ContainsKey(enemy)) continue;

                enemy.TakeDamages(damage);
                _cooldowns[enemy] = hitCooldown;
                _popRemaining = 0.14d;
            }

            _popRemaining = System.Math.Max(0, _popRemaining - delta);
            float pop = 1f + 0.15f * (float)(_popRemaining / 0.14d);
            _visual.Scale = Vector3.One * (1.25f * pop);
        }
    }
}