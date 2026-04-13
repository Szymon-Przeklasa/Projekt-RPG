using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
    [Export] public PackedScene ProjectileScene;
    [Export] public WeaponStats Weapon;
    [Export] public int Speed = 600;

    [Export] public int MaxHealth = 100;
    public int Health { get; private set; }

    [Export] public float InvincibilityTime = 0.3f;
    private float _invincibilityTimer = 0f;
    private ProgressBar _hpBar;

    public float DamageMultiplier = 1f;
    public float CooldownMultiplier = 1f;
    public float AreaMultiplier = 1f;
    public float SpeedMultiplier = 1f;
    public float ProjectileSpeedMultiplier = 1f;

    public const int MAX_WEAPONS = 5;
    public const int MAX_PASSIVES = 5;
    public List<Weapon> Weapons = new();
    public List<PassiveData> Passives = new();
    public List<UpgradeData> AvailableUpgrades = new();

    public Marker2D ShootPoint;
    private ProgressBar xpBar;

    [Export] public bool DebugDrawEnemyLines = false;
    private Label _debugLabel;

    private bool _isDead = false;

    // ── Draw ─────────────────────────────────────────────────

    public override void _Draw()
    {
        if (!DebugDrawEnemyLines) return;
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Enemy enemy)
            {
                Vector2 localPos = ToLocal(enemy.GlobalPosition);
                DrawLine(Vector2.Zero, localPos, new Color(1f, 0.2f, 0.2f, 0.6f), 1f);
                DrawCircle(localPos, 4f, Colors.Red);
            }
        }
    }

    // ── Ready ────────────────────────────────────────────────

    public override void _Ready()
    {
        // Gracz: Always — żeby _PhysicsProcess działał podczas pauzy (LevelUpUI).
        // Ruch blokujemy ręcznie sprawdzając GetTree().Paused.
        ProcessMode = ProcessModeEnum.Always;

        Health = MaxHealth;
        SetupUpgrades();

        ShootPoint = GetNode<Marker2D>("ShootPoint");
        xpBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("CanvasLayer/XPBar");
        _hpBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("CanvasLayer/HPBar");
        UpdateHpBar();

        foreach (Weapon weapon in GetNode("Weapons").GetChildren())
        {
            weapon.Init(this);
            // Bronie: Inherit — zatrzymują się przy pauzie razem z resztą gry.
            weapon.ProcessMode = ProcessModeEnum.Inherit;
            Weapons.Add(weapon);
        }

        if (DebugDrawEnemyLines)
        {
            _debugLabel = new Label();
            _debugLabel.ZIndex = 20;
            _debugLabel.Position = new Vector2(20, -80);
            _debugLabel.AddThemeColorOverride("font_color", Colors.Cyan);
            AddChild(_debugLabel);
        }

        UpdateXpBar();
    }

    // ── HP ───────────────────────────────────────────────────

    public void TakeDamage(int damage)
    {
        if (_invincibilityTimer > 0f || _isDead) return;

        Health -= damage;
        _invincibilityTimer = InvincibilityTime;
        UpdateHpBar();
        FlashDamage();

        if (Health <= 0)
        {
            Health = 0;
            Die();
        }
    }

    public void Heal(int amount)
    {
        Health = Mathf.Min(Health + amount, MaxHealth);
        UpdateHpBar();
    }

    private void UpdateHpBar()
    {
        if (_hpBar == null) return;
        _hpBar.MaxValue = MaxHealth;
        _hpBar.Value = Health;
    }

    private void FlashDamage()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", new Color(1f, 0.2f, 0.2f, 1f), 0.05f);
        tween.TweenProperty(this, "modulate", new Color(1f, 1f, 1f, 1f), 0.15f);
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        // Wyłącz bronie gracza
        foreach (var weapon in Weapons)
            weapon.ProcessMode = ProcessModeEnum.Disabled;

        // Pauzuj całą grę — zatrzymuje przeciwników, spawner, XP orby
        GetTree().Paused = true;

        var deathScreen = GetTree().CurrentScene.GetNodeOrNull<CanvasLayer>("DeathScreen");
        if (deathScreen != null)
        {
            // DeathScreen musi mieć ProcessMode = Always żeby działał przy pauzie
            deathScreen.ProcessMode = ProcessModeEnum.Always;
            deathScreen.Call("ShowDeathScreen", Level, GetKillCount());
        }
        else
        {
            // Fallback: odpauzuj żeby timer SceneTree zadziałał, potem wróć do menu
            GetTree().Paused = false;
            GetTree().CreateTimer(1.5f).Timeout += () =>
                GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
        }
    }

    private int GetKillCount()
    {
        int total = 0;
        foreach (var pair in KillManager.Instance.GetAllKills())
            total += pair.Value;
        return total;
    }

    // ── Bronie / pasywki ─────────────────────────────────────

    public bool AddWeapon(PackedScene weaponScene)
    {
        if (Weapons.Count >= MAX_WEAPONS) return false;
        var weapon = weaponScene.Instantiate<Weapon>();
        GetNode("Weapons").AddChild(weapon);
        weapon.Init(this);
        weapon.ProcessMode = ProcessModeEnum.Inherit;
        Weapons.Add(weapon);
        return true;
    }

    public bool AddPassive(PassiveData passive)
    {
        if (!passive.CanUpgrade) return false;
        if (!Passives.Contains(passive))
        {
            if (Passives.Count >= MAX_PASSIVES) return false;
            Passives.Add(passive);
        }
        passive.Apply(this);
        return true;
    }

    public List<UpgradeData> GetUpgradeChoices(int count = 3)
    {
        List<UpgradeData> valid = new();
        foreach (var upgrade in AvailableUpgrades)
            if (upgrade.CanUpgrade) valid.Add(upgrade);
        Shuffle(valid);
        if (valid.Count > count) valid.RemoveRange(count, valid.Count - count);
        return valid;
    }

    public void Shuffle<T>(IList<T> list)
    {
        var rng = new RandomNumberGenerator();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.RandiRange(0, i);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public void RefreshAllWeapons()
    {
        foreach (var weapon in Weapons)
            weapon.RefreshStats();
    }

    // ── Setup ulepszeń ───────────────────────────────────────

    private void SetupUpgrades()
    {
        var spinach = new PassiveData { Name = "Spinach", Type = PassiveType.Spinach, MaxLevel = 5, BonusPerLevel = 0.1f };
        var pummarola = new PassiveData { Name = "Pummarola", Type = PassiveType.Pummarola, MaxLevel = 5, BonusPerLevel = 0.1f };
        var hollowHeart = new PassiveData { Name = "Hollow Heart", Type = PassiveType.HollowHeart, MaxLevel = 5, BonusPerLevel = 0.1f };
        var bracer = new PassiveData { Name = "Bracer", Type = PassiveType.Bracer, MaxLevel = 5, BonusPerLevel = 0.1f };
        var wings = new PassiveData { Name = "Wings", Type = PassiveType.Wings, MaxLevel = 5, BonusPerLevel = 0.1f };

        AvailableUpgrades.Add(new UpgradeData("Spinach", UpgradeType.Stat, (p) => AddPassive(spinach), 5));
        AvailableUpgrades.Add(new UpgradeData("Pummarola", UpgradeType.Stat, (p) => AddPassive(pummarola), 5));
        AvailableUpgrades.Add(new UpgradeData("Hollow Heart", UpgradeType.Stat, (p) => AddPassive(hollowHeart), 5));
        AvailableUpgrades.Add(new UpgradeData("Bracer", UpgradeType.Stat, (p) => AddPassive(bracer), 5));
        AvailableUpgrades.Add(new UpgradeData("Wings", UpgradeType.Stat, (p) => AddPassive(wings), 5));

        var lightning = GetNodeOrNull<Lightning>("Weapons/Lightning");
        if (lightning != null)
        {
            AvailableUpgrades.Add(new UpgradeData("Lightning: +5 DMG", UpgradeType.Weapon, (p) => { lightning.Stats.Damage += 5; }, 8));
            AvailableUpgrades.Add(new UpgradeData("Lightning: -0.2s cooldown", UpgradeType.Weapon, (p) => { lightning.Stats.Cooldown = Mathf.Max(0.3f, lightning.Stats.Cooldown - 0.2f); lightning.RefreshStats(); }, 5));
            AvailableUpgrades.Add(new UpgradeData("Lightning: +1 chains", UpgradeType.Weapon, (p) => { lightning.Stats.ProjectileCount += 1; }, 4));
            AvailableUpgrades.Add(new UpgradeData("Lightning: +150 range", UpgradeType.Weapon, (p) => { lightning.Stats.Range += 150f; }, 4));
        }

        var garlic = GetNodeOrNull<Garlic>("Weapons/Garlic");
        if (garlic != null)
        {
            AvailableUpgrades.Add(new UpgradeData("Garlic: +3 DMG", UpgradeType.Weapon, (p) => { garlic.Stats.Damage += 3; }, 8));
            AvailableUpgrades.Add(new UpgradeData("Garlic: +100 range", UpgradeType.Weapon, (p) => { garlic.Stats.Range += 100f; }, 5));
            AvailableUpgrades.Add(new UpgradeData("Garlic: -0.2s cooldown", UpgradeType.Weapon, (p) => { garlic.Stats.Cooldown = Mathf.Max(0.3f, garlic.Stats.Cooldown - 0.2f); garlic.RefreshStats(); }, 4));
        }

        var firewand = GetNodeOrNull<FireWand>("Weapons/FireWand");
        if (firewand != null)
        {
            AvailableUpgrades.Add(new UpgradeData("Fire Wand: +4 DMG", UpgradeType.Weapon, (p) => { firewand.Stats.Damage += 4; }, 8));
            AvailableUpgrades.Add(new UpgradeData("Fire Wand: +1 projectile", UpgradeType.Weapon, (p) => { firewand.Stats.ProjectileCount += 1; }, 4));
            AvailableUpgrades.Add(new UpgradeData("Fire Wand: +1 pierce", UpgradeType.Weapon, (p) => { firewand.Stats.Pierce += 1; }, 4));
            AvailableUpgrades.Add(new UpgradeData("Fire Wand: -0.15s cooldown", UpgradeType.Weapon, (p) => { firewand.Stats.Cooldown = Mathf.Max(0.1f, firewand.Stats.Cooldown - 0.15f); firewand.RefreshStats(); }, 5));
        }
    }

    // ── Wrogowie ─────────────────────────────────────────────

    public Node2D GetClosestEnemy(float range)
    {
        Node2D closest = null;
        float best = range;
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Node2D enemy)
            {
                float d = GlobalPosition.DistanceTo(enemy.GlobalPosition);
                if (d < best) { best = d; closest = enemy; }
            }
        }
        return closest;
    }

    // ── XP / Level ───────────────────────────────────────────

    public int Level = 1;
    public int Xp = 0;
    public int XpToLevel => Level * 40;

    public void GainXp(int amount)
    {
        if (_isDead) return;
        Xp += amount * 2;
        while (Xp >= XpToLevel)
        {
            Xp -= XpToLevel;
            LevelUp();
        }
        UpdateXpBar();
    }

    private void UpdateXpBar()
    {
        if (xpBar == null) return;
        xpBar.MaxValue = XpToLevel;
        xpBar.Value = Xp;
    }

    private void LevelUp()
    {
        Level++;
        var ui = GetTree().CurrentScene.GetNodeOrNull<LevelUpUI>("LevelUpUI");
        ui?.ShowUpgrades(this);
    }

    // ── Ruch ─────────────────────────────────────────────────

    public void GetInput()
    {
        // Blokuj ruch podczas pauzy (LevelUpUI) i po śmierci
        if (GetTree().Paused || _isDead)
        {
            Velocity = Vector2.Zero;
            return;
        }

        Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");
        Velocity = inputDirection * Speed * SpeedMultiplier;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_invincibilityTimer > 0f)
            _invincibilityTimer -= (float)delta;

        GetInput();
        MoveAndSlide();
    }

    public override void _Process(double delta)
    {
        if (!DebugDrawEnemyLines) return;

        var lines = new System.Text.StringBuilder();
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Enemy enemy)
                lines.AppendLine($"{enemy.Name}: {GlobalPosition.DistanceTo(enemy.GlobalPosition):F0}px");
        }
        if (_debugLabel != null) _debugLabel.Text = lines.ToString();
        QueueRedraw();
    }
}