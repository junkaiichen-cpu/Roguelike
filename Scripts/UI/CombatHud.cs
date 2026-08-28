using Godot;
using System.Collections.Generic;

public partial class CombatHud : Control
{
    private readonly Dictionary<string, Label> _weaponRows = new();
    private Player _player;
    private Label _healthLabel;
    private Label _levelLabel;
    private GameManager _gameManager;
    private bool _isReady;
    private Player _pendingPlayer;

    public override void _Ready()
    {
        _healthLabel = GetNodeOrNull<Label>("PlayerStatus/Health");
        _levelLabel = GetNodeOrNull<Label>("PlayerStatus/Level");
        _gameManager = GetNodeOrNull<GameManager>("/root/GameManager");

        Control weaponBar = GetNodeOrNull<Control>("WeaponBar");
        foreach (Node child in weaponBar?.GetChildren() ?? [])
        {
            if (child is Label row && !string.IsNullOrEmpty(row.Name))
            {
                _weaponRows[row.Name] = row;
            }
        }

        _isReady = true;
        if (_pendingPlayer != null) BindPlayer(_pendingPlayer);
    }

    public void BindPlayer(Player player)
    {
        if (!_isReady)
        {
            _pendingPlayer = player;
            return;
        }

        if (_player == player) return;

        if (_player != null)
        {
            _player.ExperienceChanged -= Refresh;
            _player.HealthChanged -= Refresh;
            _player.LeveledUp -= OnPlayerLeveledUp;
            _player.Died -= OnPlayerDied;
            _player.BuildChanged -= Refresh;
        }

        _player = player;
        if (_player == null) return;

        _player.ExperienceChanged += Refresh;
        _player.HealthChanged += Refresh;
        _player.LeveledUp += OnPlayerLeveledUp;
        _player.Died += OnPlayerDied;
        _player.BuildChanged += Refresh;
        Refresh(_player);
    }

    public void Refresh(Player player)
    {
        if (player == null || player != _player) return;

        _healthLabel.Text = $"HP {player.CurrentHealth}/{player.MaxHealth}";
        _levelLabel.Text = $"LV {player.CurrentLevel}";
        SetWeapon("HolyLight", $"✦ {GetWeaponLevel("HolyLight")}", true);

        CrossAttack cross = player.GetNodeOrNull<CrossAttack>("CrossAttack");
        SetWeapon("Cross", $"✝ {GetWeaponLevel("Cross")}", cross?.IsUnlocked == true);
        LightningAttack lightning = player.GetNodeOrNull<LightningAttack>("LightningAttack");
        SetWeapon("Lightning", $"⚡ {GetWeaponLevel("Lightning")}", lightning?.IsUnlocked == true);
        BibleAttack bible = player.GetNodeOrNull<BibleAttack>("BibleAttack");
        SetWeapon("Bible", $"📖 {GetWeaponLevel("Bible")}", bible?.IsUnlocked == true);
        FloatingSphereAttack orb = player.GetNodeOrNull<FloatingSphereAttack>("FloatingSphere");
        SetWeapon("Orb", $"● {GetWeaponLevel("Orb")}", orb?.IsUnlocked == true);
        GroundFireAttack fire = player.GetNodeOrNull<GroundFireAttack>("GroundFire");
        SetWeapon("Fire", $"🔥 {GetWeaponLevel("Fire")}", fire?.IsUnlocked == true);
        SpiritWater spiritWater = player.GetNodeOrNull<SpiritWater>("SpiritWater");
        SetWeapon("SpiritWater", $"💧 {GetWeaponLevel("SpiritWater")}", spiritWater?.IsUnlocked == true);
        LifestealAttack lifesteal = player.GetNodeOrNull<LifestealAttack>("Lifesteal");
        SetWeapon("Lifesteal", $"♥ {GetWeaponLevel("Lifesteal")}", lifesteal?.IsUnlocked == true);
    }

    private int GetWeaponLevel(string id) => _gameManager?.GetWeaponLevel(id) ?? 1;

    private void SetWeapon(string id, string text, bool visible)
    {
        if (!_weaponRows.TryGetValue(id, out Label row)) return;
        row.Text = text;
        bool wasVisible = row.Visible;
        row.Visible = visible;
        if (visible && !wasVisible) PulseWeapon(row);
    }

    public void PulseUpgrade(TemporaryUpgradeDefinition upgrade)
    {
        if (upgrade == null) return;
        string id = upgrade.Effect switch
        {
            TemporaryUpgradeEffect.ProjectileDamage or TemporaryUpgradeEffect.ProjectileAttackSpeed
                or TemporaryUpgradeEffect.ProjectileSpeed or TemporaryUpgradeEffect.ProjectileCount
                or TemporaryUpgradeEffect.ProjectileSpread or TemporaryUpgradeEffect.ProjectileSize
                or TemporaryUpgradeEffect.ProjectileCountDouble or TemporaryUpgradeEffect.ProjectileDamagePercent
                or TemporaryUpgradeEffect.ProjectileAttackSpeedPercent => "HolyLight",
            TemporaryUpgradeEffect.CrossDamage or TemporaryUpgradeEffect.CrossSize or TemporaryUpgradeEffect.CrossCooldown
                or TemporaryUpgradeEffect.UnlockCross => "Cross",
            TemporaryUpgradeEffect.LightningDamage or TemporaryUpgradeEffect.LightningCount
                or TemporaryUpgradeEffect.LightningFrequency or TemporaryUpgradeEffect.LightningChainCount
                or TemporaryUpgradeEffect.UnlockLightning => "Lightning",
            TemporaryUpgradeEffect.BibleDamage or TemporaryUpgradeEffect.BibleCount
                or TemporaryUpgradeEffect.BibleOrbitSpeed or TemporaryUpgradeEffect.BibleRadius
                or TemporaryUpgradeEffect.UnlockBible => "Bible",
            TemporaryUpgradeEffect.OrbDamage or TemporaryUpgradeEffect.OrbCount
                or TemporaryUpgradeEffect.OrbSpeed or TemporaryUpgradeEffect.UnlockOrb => "Orb",
            TemporaryUpgradeEffect.FireDamage or TemporaryUpgradeEffect.FireArea
                or TemporaryUpgradeEffect.FireDuration or TemporaryUpgradeEffect.FireFrequency
                or TemporaryUpgradeEffect.UnlockFire => "Fire",
            TemporaryUpgradeEffect.SpiritWaterDamage or TemporaryUpgradeEffect.SpiritWaterDuration
                or TemporaryUpgradeEffect.SpiritWaterCooldown or TemporaryUpgradeEffect.UnlockSpiritWater => "SpiritWater",
            TemporaryUpgradeEffect.LifestealDamage or TemporaryUpgradeEffect.LifestealCooldown
                or TemporaryUpgradeEffect.UnlockLifesteal => "Lifesteal",
            _ => string.Empty,
        };
        if (!string.IsNullOrEmpty(id) && _weaponRows.TryGetValue(id, out Label row) && row.Visible)
        {
            PulseWeapon(row);
        }
    }

    private void PulseWeapon(Control row)
    {
        row.Scale = Vector2.One * 1.2f;
        CreateTween().TweenProperty(row, "scale", Vector2.One, 0.16f);
    }

    private void OnPlayerLeveledUp(Player player, uint levelsGained) => Refresh(player);

    private void OnPlayerDied(Player player) => Refresh(player);
}
