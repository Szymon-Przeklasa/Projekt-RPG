using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Główna klasa gracza. Obsługuje ruch, statystyki, system walki,
/// zarządzanie ekwipunkiem (bronie/pasywki) oraz system ulepszeń.
/// </summary>
public partial class Player : CharacterBody2D
{
	// ── Eksportowane właściwości ──────────────────────────────
	[Export] public PackedScene ProjectileScene;
	[Export] public WeaponStats Weapon;
	[Export] public int Speed = 600;
	[Export] public int MaxHealth = 100;
	[Export] public float InvincibilityTime = 0.3f;
	[Export] public bool DebugDrawEnemyLines = false;

	// ── Statystyki i Stan ────────────────────────────────────
	public int Health { get; private set; }
	private float _invincibilityTimer = 0f;
	private ProgressBar _hpBar;
	private bool _isDead = false;
	public bool IsInLevelUp = false;

	// ── Mnożniki Statystyk ───────────────────────────────────
	public float DamageMultiplier = 1f;
	public float CooldownMultiplier = 1f;
	public float AreaMultiplier = 1f;
	public float SpeedMultiplier = 1f;
	public float ProjectileSpeedMultiplier = 1f;

	// ── Ekwipunek i Ulepszenia ───────────────────────────────
	public const int MAX_WEAPONS = 6;
	public const int MAX_PASSIVES = 6;
	public List<Weapon> Weapons = new();
	public List<PassiveData> Passives = new();
	public List<UpgradeData> AvailableUpgrades = new();

	// Sceny broni do odblokowywania przez level up
	[Export] public PackedScene LightningScene;
	[Export] public PackedScene GarlicScene;
	[Export] public PackedScene MagnetScene;
	[Export] public PackedScene MagicMissileScene;
	[Export] public PackedScene AxeScene;

	// ── Węzły ────────────────────────────────────────────────
	public Marker2D ShootPoint;
	private ProgressBar xpBar;
	private Label _debugLabel;
	private EquipmentUI _equipmentUI;

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
		ProcessMode = ProcessModeEnum.Always;
		Health = MaxHealth;

		ShootPoint = GetNode<Marker2D>("ShootPoint");
		xpBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("CanvasLayer/XPBar");
		_hpBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("CanvasLayer/HPBar");
		_equipmentUI = GetTree().CurrentScene.GetNodeOrNull<EquipmentUI>("CanvasLayer/EquipmentUI");

		UpdateHpBar();

		// Inicjalizuj tylko FireWand (pierwsze dziecko w Weapons)
		var weaponsNode = GetNode("Weapons");
		foreach (Node child in weaponsNode.GetChildren())
		{
			if (child is Weapon w)
			{
				w.Init(this);
				w.ProcessMode = ProcessModeEnum.Pausable;
				Weapons.Add(w);
				break; // tylko pierwsza broń startuje aktywna
			}
		}

		// Dezaktywuj pozostałe bronie w scenie (do odblokowania przez level up)
		bool first = true;
		foreach (Node child in weaponsNode.GetChildren())
		{
			if (child is Weapon w)
			{
				if (first) { first = false; continue; }
				w.ProcessMode = ProcessModeEnum.Disabled;
				w.QueueFree(); // usuń z węzła, będą dodawane dynamicznie
			}
		}

		SetupUpgrades();

		if (DebugDrawEnemyLines)
		{
			_debugLabel = new Label();
			_debugLabel.ZIndex = 20;
			_debugLabel.Position = new Vector2(20, -80);
			_debugLabel.AddThemeColorOverride("font_color", Colors.Cyan);
			AddChild(_debugLabel);
		}

		UpdateXpBar();
		RefreshEquipmentUI();
	}

	// ── HP ───────────────────────────────────────────────────
	public void TakeDamage(int damage)
	{
		if (IsInLevelUp) return;
		if (_invincibilityTimer > 0f || _isDead) return;

		Health -= damage;
		_invincibilityTimer = InvincibilityTime;

		SoundManager.Instance?.PlayHurt();
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
		SoundManager.Instance?.PlayHeal();
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

		foreach (var weapon in Weapons)
			weapon.ProcessMode = ProcessModeEnum.Disabled;

		GetTree().Paused = true;

		var deathScreen = GetTree().CurrentScene.GetNodeOrNull<CanvasLayer>("DeathScreen");
		if (deathScreen != null)
		{
			deathScreen.ProcessMode = ProcessModeEnum.Always;
			deathScreen.Call("ShowDeathScreen", Level, GetKillCount());
		}
		else
		{
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
		weapon.ProcessMode = ProcessModeEnum.Pausable;
		Weapons.Add(weapon);
		RefreshEquipmentUI();
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
		RefreshEquipmentUI();
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

	public void RefreshEquipmentUI()
	{
		if (_equipmentUI == null)
			_equipmentUI = GetTree().CurrentScene.GetNodeOrNull<EquipmentUI>("CanvasLayer/EquipmentUI");
		_equipmentUI?.Refresh(this);
	}

	// ── Setup ulepszeń ───────────────────────────────────────
	private void SetupUpgrades()
	{
		// ── Pasywki ──────────────────────────────────────────
		var spinach = new PassiveData { Name = "Spinach", Type = PassiveType.Spinach, MaxLevel = 5, BonusPerLevel = 0.1f };
		AvailableUpgrades.Add(new UpgradeData("Spinach", UpgradeType.Passive)
			.AddLevel("Damage +10%.", p => AddPassive(spinach))
			.AddLevel("Damage +10%.", p => AddPassive(spinach))
			.AddLevel("Damage +10%.", p => AddPassive(spinach))
			.AddLevel("Damage +10%.", p => AddPassive(spinach))
			.AddLevel("Damage +10%.", p => AddPassive(spinach)));

		var pummarola = new PassiveData { Name = "Pummarola", Type = PassiveType.Pummarola, MaxLevel = 5, BonusPerLevel = 0.1f };
		AvailableUpgrades.Add(new UpgradeData("Pummarola", UpgradeType.Passive)
			.AddLevel("Cooldown -10%.", p => AddPassive(pummarola))
			.AddLevel("Cooldown -10%.", p => AddPassive(pummarola))
			.AddLevel("Cooldown -10%.", p => AddPassive(pummarola))
			.AddLevel("Cooldown -10%.", p => AddPassive(pummarola))
			.AddLevel("Cooldown -10%.", p => AddPassive(pummarola)));

		var hollowHeart = new PassiveData { Name = "Hollow Heart", Type = PassiveType.HollowHeart, MaxLevel = 5, BonusPerLevel = 0.1f };
		AvailableUpgrades.Add(new UpgradeData("Hollow Heart", UpgradeType.Passive)
			.AddLevel("Area +10%.", p => AddPassive(hollowHeart))
			.AddLevel("Area +10%.", p => AddPassive(hollowHeart))
			.AddLevel("Area +10%.", p => AddPassive(hollowHeart))
			.AddLevel("Area +10%.", p => AddPassive(hollowHeart))
			.AddLevel("Area +10%.", p => AddPassive(hollowHeart)));

		var bracer = new PassiveData { Name = "Bracer", Type = PassiveType.Bracer, MaxLevel = 5, BonusPerLevel = 0.1f };
		AvailableUpgrades.Add(new UpgradeData("Bracer", UpgradeType.Passive)
			.AddLevel("Projectile Speed +10%.", p => AddPassive(bracer))
			.AddLevel("Projectile Speed +10%.", p => AddPassive(bracer))
			.AddLevel("Projectile Speed +10%.", p => AddPassive(bracer))
			.AddLevel("Projectile Speed +10%.", p => AddPassive(bracer))
			.AddLevel("Projectile Speed +10%.", p => AddPassive(bracer)));

		var wings = new PassiveData { Name = "Wings", Type = PassiveType.Wings, MaxLevel = 5, BonusPerLevel = 0.1f };
		AvailableUpgrades.Add(new UpgradeData("Wings", UpgradeType.Passive)
			.AddLevel("Move Speed +10%.", p => AddPassive(wings))
			.AddLevel("Move Speed +10%.", p => AddPassive(wings))
			.AddLevel("Move Speed +10%.", p => AddPassive(wings))
			.AddLevel("Move Speed +10%.", p => AddPassive(wings))
			.AddLevel("Move Speed +10%.", p => AddPassive(wings)));

		// ── Bronie (odblokowanie + ulepszenia) ───────────────

		// FireWand — gracz startuje z tą bronią, level 1 już "odblokowany"
		var firewand = GetNodeOrNull<FireWand>("Weapons/FireWand");
		if (firewand != null)
		{
			var fwUpgrade = new UpgradeData("Fire Wand", UpgradeType.Weapon);
			fwUpgrade.Level = 1; // już odblokowana
			fwUpgrade
				.AddLevel("Fires at the nearest enemy.",                p => { /* starter */ })
				.AddLevel("Base Damage +4. +1 Projectile.",             p => { firewand.Stats.Damage += 4; firewand.Stats.ProjectileCount += 1; })
				.AddLevel("Cooldown -0.15s. Base Damage +4.",           p => { firewand.Stats.Damage += 4; firewand.Stats.Cooldown = Mathf.Max(0.1f, firewand.Stats.Cooldown - 0.15f); firewand.RefreshStats(); })
				.AddLevel("+1 Pierce. Base Damage +4.",                 p => { firewand.Stats.Damage += 4; firewand.Stats.Pierce += 1; })
				.AddLevel("Cooldown -0.15s. +1 Projectile.",            p => { firewand.Stats.ProjectileCount += 1; firewand.Stats.Cooldown = Mathf.Max(0.1f, firewand.Stats.Cooldown - 0.15f); firewand.RefreshStats(); })
				.AddLevel("Base Damage +4. +1 Pierce.",                 p => { firewand.Stats.Damage += 4; firewand.Stats.Pierce += 1; })
				.AddLevel("Cooldown -0.15s. Base Damage +4.",           p => { firewand.Stats.Damage += 4; firewand.Stats.Cooldown = Mathf.Max(0.1f, firewand.Stats.Cooldown - 0.15f); firewand.RefreshStats(); })
				.AddLevel("Base Damage +8. +1 Projectile. +1 Pierce.", p => { firewand.Stats.Damage += 8; firewand.Stats.ProjectileCount += 1; firewand.Stats.Pierce += 1; });
			AvailableUpgrades.Add(fwUpgrade);
		}

		// Lightning — odblokowywane przez level up
		AvailableUpgrades.Add(new UpgradeData("Lightning", UpgradeType.Weapon)
			.AddLevel("UNLOCK: Strikes nearest enemy, chains to others.", p =>
			{
				if (LightningScene != null) p.AddWeapon(LightningScene);
			})
			.AddLevel("Base Damage +5. +1 chain.",                        p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Damage += 5; w.Stats.ProjectileCount += 1; } })
			.AddLevel("Cooldown -0.1s. Base Damage +5.",                  p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Damage += 5; w.Stats.Cooldown = Mathf.Max(0.3f, w.Stats.Cooldown - 0.1f); w.RefreshStats(); } })
			.AddLevel("Base Range +40. +1 chain.",                        p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Range += 40f; w.Stats.ProjectileCount += 1; } })
			.AddLevel("Base Damage +5. Cooldown -0.1s.",                  p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Damage += 5; w.Stats.Cooldown = Mathf.Max(0.3f, w.Stats.Cooldown - 0.1f); w.RefreshStats(); } })
			.AddLevel("Base Range +40. +1 chain.",                        p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Range += 40f; w.Stats.ProjectileCount += 1; } })
			.AddLevel("Base Damage +5. Cooldown -0.1s.",                  p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Damage += 5; w.Stats.Cooldown = Mathf.Max(0.3f, w.Stats.Cooldown - 0.1f); w.RefreshStats(); } })
			.AddLevel("Base Damage +10. Base Range +40.",                 p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Damage += 10; w.Stats.Range += 40f; } }));

		// Garlic — odblokowywane przez level up
		AvailableUpgrades.Add(new UpgradeData("Garlic", UpgradeType.Weapon)
			.AddLevel("UNLOCK: Damages nearby enemies.",                  p =>
			{
				if (GarlicScene != null) p.AddWeapon(GarlicScene);
			})
			.AddLevel("Base Area +40%. Base Damage +2.",                  p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range += w.Stats.Range * 0.4f; w.Stats.Damage += 2; } })
			.AddLevel("Cooldown -0.1s. Base Damage +1.",                  p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Damage += 1; w.Stats.Cooldown = Mathf.Max(0.3f, w.Stats.Cooldown - 0.1f); w.RefreshStats(); } })
			.AddLevel("Base Area +20%. Base Damage +1.",                  p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range += w.Stats.Range * 0.2f; w.Stats.Damage += 1; } })
			.AddLevel("Cooldown -0.1s. Base Damage +2.",                  p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Damage += 2; w.Stats.Cooldown = Mathf.Max(0.3f, w.Stats.Cooldown - 0.1f); w.RefreshStats(); } })
			.AddLevel("Base Area +20%. Base Damage +1.",                  p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range += w.Stats.Range * 0.2f; w.Stats.Damage += 1; } })
			.AddLevel("Cooldown -0.1s. Base Damage +1.",                  p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Damage += 1; w.Stats.Cooldown = Mathf.Max(0.3f, w.Stats.Cooldown - 0.1f); w.RefreshStats(); } })
			.AddLevel("Base Area +20%. Base Damage +2.",                  p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range += w.Stats.Range * 0.2f; w.Stats.Damage += 2; } }));

		// Magnet — odblokowywane przez level up
		AvailableUpgrades.Add(new UpgradeData("Magnet", UpgradeType.Weapon)
			.AddLevel("UNLOCK: Attracts nearby XP orbs.",                p =>
			{
				if (MagnetScene != null) p.AddWeapon(MagnetScene);
			})
			.AddLevel("Pull range +20%. Pull speed +50.",                 p => { var w = FindWeapon<Magnet>(); if (w != null) { w.Stats.Range *= 1.2f; } })
			.AddLevel("Pull range +20%. Pull speed +50.",                 p => { var w = FindWeapon<Magnet>(); if (w != null) { w.Stats.Range *= 1.2f; } })
			.AddLevel("Pull range +30%. Double pull speed.",              p => { var w = FindWeapon<Magnet>(); if (w != null) { w.Stats.Range *= 1.3f; } })
			.AddLevel("Mega range: collect from entire screen.",          p => { var w = FindWeapon<Magnet>(); if (w != null) { w.Stats.Range = 2000f; } }));
	}

	/// <summary>Pomocnicza metoda szukająca broni danego typu wśród posiadanych.</summary>
	private T FindWeapon<T>() where T : Weapon
	{
		foreach (var w in Weapons)
			if (w is T found) return found;
		return null;
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
		SoundManager.Instance?.PlayLevelUp();
		var ui = GetTree().CurrentScene.GetNodeOrNull<LevelUpUI>("LevelUpUI");
		ui?.ShowUpgrades(this);
		Heal(MaxHealth / 4); // Lecz 25% HP zamiast 100% przy level up
	}

	// ── Ruch ─────────────────────────────────────────────────
	public void GetInput()
	{
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

	public void SetWeaponsProcessMode(ProcessModeEnum mode)
	{
		foreach (var weapon in Weapons)
			weapon.ProcessMode = mode;
	}
}