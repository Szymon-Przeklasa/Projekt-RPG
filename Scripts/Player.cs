using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
	[Export] public PackedScene ProjectileScene;
	[Export] public WeaponStats Weapon;
	[Export] public int Speed = 600;
	[Export] public int MaxHealth = 100;
	[Export] public float InvincibilityTime = 0.5f;
	[Export] public bool DebugDrawEnemyLines = false;

	// Sceny pocisków — jedyne PackedScene których bronie potrzebują
	[Export] public PackedScene LightningBeamScene;
	[Export] public PackedScene MagicMissileProjectileScene;
	[Export] public PackedScene AxeProjectileScene;

	public int Health { get; private set; }
	private float _invincibilityTimer = 0f;
	private ProgressBar _hpBar;
	private Label _currentHp;
	private bool _isDead = false;
	public bool IsInLevelUp = false;

	public float DamageMultiplier = 1f;
	public float CooldownMultiplier = 1f;
	public float AreaMultiplier = 1f;
	public float SpeedMultiplier = 1f;
	public float ProjectileSpeedMultiplier = 1f;

	public const int MAX_WEAPONS = 6;
	public const int MAX_PASSIVES = 6;
	public List<Weapon> Weapons = new();
	public List<PassiveData> Passives = new();
	public List<UpgradeData> AvailableUpgrades = new();

	public Marker2D ShootPoint;
	private ProgressBar xpBar;
	private Label _debugLabel;
	private EquipmentUI _equipmentUI;

	public static int SelectedStartWeaponIndex = 0;

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

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		// Gracz: layer 1 (mapa), mask 1 (tylko mapa, nie wrogowie)
		//CollisionLayer = 1;
		//CollisionMask = 1;

		Health = MaxHealth;
		ShootPoint = GetNode<Marker2D>("ShootPoint");
		xpBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("CanvasLayer/XPBar");
		_hpBar = GetTree().CurrentScene.GetNodeOrNull<ProgressBar>("CanvasLayer/HPBar");
		_currentHp = GetTree().CurrentScene.GetNodeOrNull<Label>("CanvasLayer/HPBar/Label");
		_equipmentUI = GetTree().CurrentScene.GetNodeOrNull<EquipmentUI>("CanvasLayer/EquipmentUI");

		UpdateHpBar();

		// Usuń predefiniowane bronie z .tscn — zarządzamy dynamicznie
		var weaponsNode = GetNode("Weapons");
		foreach (Node child in weaponsNode.GetChildren())
			child.QueueFree();

		SetupUpgrades();
		AddStartingWeapon();

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

	// ── Dodawanie broni przez typ (skryptowo, bez PackedScene) ──

	public bool AddWeaponOfType<T>(WeaponStats stats = null) where T : Weapon, new()
	{
		if (Weapons.Count >= MAX_WEAPONS) return false;

		foreach (var existing in Weapons)
			if (existing is T) return false;

		var weapon = new T();
		if (stats != null)
			weapon.Stats = stats;

		SetWeaponProjectileScenes(weapon);

		GetNode("Weapons").AddChild(weapon);
		weapon.Init(this);
		weapon.ProcessMode = ProcessModeEnum.Pausable;
		Weapons.Add(weapon);
		RefreshEquipmentUI();
		return true;
	}

	private void SetWeaponProjectileScenes(Weapon weapon)
	{
		switch (weapon)
		{
			case FireWand fw:
				fw.ProjectileScene = ProjectileScene;
				break;
			case Lightning lt:
				lt.ProjectileScene = LightningBeamScene;
				break;
			case MagicMissile mm:
				if (MagicMissileProjectileScene != null)
					mm.ProjectileScene = MagicMissileProjectileScene;
				break;
			case Axe axe:
				if (AxeProjectileScene != null)
					axe.ProjectileScene = AxeProjectileScene;
				break;
		}
	}

	private void AddStartingWeapon()
	{
		// Magnet zawsze jako darmowy starter
		var magnetStats = new WeaponStats { Cooldown = 0.01f, Range = 40f };
		AddWeaponOfType<Magnet>(magnetStats);
		MarkWeaponUnlocked("Magnet");

		switch (SelectedStartWeaponIndex)
		{
			case 0:
				AddWeaponOfType<FireWand>(new WeaponStats { Damage = 10, Cooldown = 1f, Speed = 200f, Knockback = 250f, SpreadAngle = 10f, Range = 150f });
				MarkWeaponUnlocked("Fire Wand");
				break;
			case 1:
				AddWeaponOfType<Lightning>(new WeaponStats { Cooldown = 2f, Damage = 20, Knockback = 150f, Range = 100f, ProjectileCount = 3 });
				MarkWeaponUnlocked("Lightning");
				break;
			case 2:
				AddWeaponOfType<Garlic>(new WeaponStats { Cooldown = 0.4f, Damage = 4, Knockback = 40f, Range = 80f });
				MarkWeaponUnlocked("Garlic");
				break;
			case 3:
				AddWeaponOfType<MagicMissile>(new WeaponStats { Cooldown = 1.2f, Damage = 15, Speed = 350f, Range = 500f, ProjectileCount = 1 });
				MarkWeaponUnlocked("Magic Missile");
				break;
			case 4:
				AddWeaponOfType<Axe>(new WeaponStats { Cooldown = 1.5f, Damage = 25, Speed = 400f, Knockback = 300f, Range = 200f, ProjectileCount = 1 });
				MarkWeaponUnlocked("Axe");
				break;
			default:
				goto case 0;
		}
	}

	private void MarkWeaponUnlocked(string name)
	{
		foreach (var upg in AvailableUpgrades)
			if (upg.Name == name && upg.Type == UpgradeType.Weapon)
			{
				upg.Level = 1;
				break;
			}
	}

	public void TakeDamage(int damage)
	{
		if (IsInLevelUp) return;
		if (_invincibilityTimer > 0f || _isDead) return;

		Health -= damage;
		_invincibilityTimer = 0.1f;

		SoundManager.Instance?.PlayHurt();
		UpdateHpBar();
		FlashDamage();

		if (Health <= 0) { Health = 0; Die(); }
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
		_currentHp.Text = Health + "/100";
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

	public void Shuffle<T>(IList<T> list)
	{
		var rng = new RandomNumberGenerator();
		for (int i = list.Count - 1; i > 0; i--)
		{
			int j = rng.RandiRange(0, i);
			(list[i], list[j]) = (list[j], list[i]);
		}
	}

	private void SetupUpgrades()
	{
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

		AvailableUpgrades.Add(new UpgradeData("Fire Wand", UpgradeType.Weapon)
			.AddLevel("UNLOCK: Strzela w najbliższego wroga.", p =>
			{
				p.AddWeaponOfType<FireWand>(new WeaponStats { Damage = 10, Cooldown = 1f, Speed = 200f, Knockback = 250f, SpreadAngle = 10f, Range = 150f });
			})
			.AddLevel("Base Damage +4. +1 Projectile.", p => { var w = FindWeapon<FireWand>(); if (w != null) { w.Stats.Damage += 4; w.Stats.ProjectileCount += 1; } })
			.AddLevel("Cooldown -0.15s. Base Damage +4.", p => { var w = FindWeapon<FireWand>(); if (w != null) { w.Stats.Damage += 4; w.Stats.Cooldown = Mathf.Max(0.1f, w.Stats.Cooldown - 0.15f); w.RefreshStats(); } })
			.AddLevel("+1 Pierce. Base Damage +4.", p => { var w = FindWeapon<FireWand>(); if (w != null) { w.Stats.Damage += 4; w.Stats.Pierce += 1; } })
			.AddLevel("Cooldown -0.15s. +1 Projectile.", p => { var w = FindWeapon<FireWand>(); if (w != null) { w.Stats.ProjectileCount += 1; w.Stats.Cooldown = Mathf.Max(0.1f, w.Stats.Cooldown - 0.15f); w.RefreshStats(); } })
			.AddLevel("Base Damage +4. +1 Pierce.", p => { var w = FindWeapon<FireWand>(); if (w != null) { w.Stats.Damage += 4; w.Stats.Pierce += 1; } })
			.AddLevel("Cooldown -0.15s. Base Damage +4.", p => { var w = FindWeapon<FireWand>(); if (w != null) { w.Stats.Damage += 4; w.Stats.Cooldown = Mathf.Max(0.1f, w.Stats.Cooldown - 0.15f); w.RefreshStats(); } })
			.AddLevel("Base Damage +8. +1 Projectile. +1 Pierce.", p => { var w = FindWeapon<FireWand>(); if (w != null) { w.Stats.Damage += 8; w.Stats.ProjectileCount += 1; w.Stats.Pierce += 1; } }));

		AvailableUpgrades.Add(new UpgradeData("Lightning", UpgradeType.Weapon)
			.AddLevel("UNLOCK: Razi najbliższego wroga, skacze po innych.", p =>
			{
				p.AddWeaponOfType<Lightning>(new WeaponStats { Cooldown = 2f, Damage = 20, Knockback = 150f, Range = 100f, ProjectileCount = 3 });
			})
			.AddLevel("Base Damage +5. +1 chain.", p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Damage += 5; w.Stats.ProjectileCount += 1; } })
			.AddLevel("Cooldown -0.1s. Base Damage +5.", p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Damage += 5; w.Stats.Cooldown = Mathf.Max(0.3f, w.Stats.Cooldown - 0.1f); w.RefreshStats(); } })
			.AddLevel("Base Range +40. +1 chain.", p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Range += 40f; w.Stats.ProjectileCount += 1; } })
			.AddLevel("Base Damage +5. Cooldown -0.1s.", p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Damage += 5; w.Stats.Cooldown = Mathf.Max(0.3f, w.Stats.Cooldown - 0.1f); w.RefreshStats(); } })
			.AddLevel("Base Range +40. +1 chain.", p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Range += 40f; w.Stats.ProjectileCount += 1; } })
			.AddLevel("Base Damage +5. Cooldown -0.1s.", p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Damage += 5; w.Stats.Cooldown = Mathf.Max(0.3f, w.Stats.Cooldown - 0.1f); w.RefreshStats(); } })
			.AddLevel("Base Damage +10. Base Range +40.", p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Damage += 10; w.Stats.Range += 40f; } }));

		AvailableUpgrades.Add(new UpgradeData("Garlic", UpgradeType.Weapon)
			.AddLevel("UNLOCK: Zadaje obrażenia pobliskim wrogom.", p =>
			{
				p.AddWeaponOfType<Garlic>(new WeaponStats { Cooldown = 0.4f, Damage = 4, Knockback = 40f, Range = 80f });
			})
			.AddLevel("Base Area +40%. Base Damage +2.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range *= 1.4f; w.Stats.Damage += 2; } })
			.AddLevel("Cooldown -0.1s. Base Damage +1.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Damage += 1; w.Stats.Cooldown = Mathf.Max(0.3f, w.Stats.Cooldown - 0.1f); w.RefreshStats(); } })
			.AddLevel("Base Area +20%. Base Damage +1.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range *= 1.2f; w.Stats.Damage += 1; } })
			.AddLevel("Cooldown -0.1s. Base Damage +2.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Damage += 2; w.Stats.Cooldown = Mathf.Max(0.3f, w.Stats.Cooldown - 0.1f); w.RefreshStats(); } })
			.AddLevel("Base Area +20%. Base Damage +1.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range *= 1.2f; w.Stats.Damage += 1; } })
			.AddLevel("Cooldown -0.1s. Base Damage +1.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Damage += 1; w.Stats.Cooldown = Mathf.Max(0.3f, w.Stats.Cooldown - 0.1f); w.RefreshStats(); } })
			.AddLevel("Base Area +20%. Base Damage +2.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range *= 1.2f; w.Stats.Damage += 2; } }));

		AvailableUpgrades.Add(new UpgradeData("Magic Missile", UpgradeType.Weapon)
			.AddLevel("UNLOCK: Samonaprowadzający pocisk.", p =>
			{
				p.AddWeaponOfType<MagicMissile>(new WeaponStats { Cooldown = 1.2f, Damage = 15, Speed = 350f, Range = 500f, ProjectileCount = 1 });
			})
			.AddLevel("Base Damage +5. +1 Projectile.", p => { var w = FindWeapon<MagicMissile>(); if (w != null) { w.Stats.Damage += 5; w.Stats.ProjectileCount += 1; } })
			.AddLevel("Cooldown -0.15s. Base Damage +5.", p => { var w = FindWeapon<MagicMissile>(); if (w != null) { w.Stats.Damage += 5; w.Stats.Cooldown = Mathf.Max(0.2f, w.Stats.Cooldown - 0.15f); w.RefreshStats(); } })
			.AddLevel("+1 Projectile.", p => { var w = FindWeapon<MagicMissile>(); if (w != null) { w.Stats.ProjectileCount += 1; } })
			.AddLevel("Base Damage +5. Cooldown -0.15s.", p => { var w = FindWeapon<MagicMissile>(); if (w != null) { w.Stats.Damage += 5; w.Stats.Cooldown = Mathf.Max(0.2f, w.Stats.Cooldown - 0.15f); w.RefreshStats(); } })
			.AddLevel("+1 Projectile. Base Damage +5.", p => { var w = FindWeapon<MagicMissile>(); if (w != null) { w.Stats.Damage += 5; w.Stats.ProjectileCount += 1; } })
			.AddLevel("Cooldown -0.15s.", p => { var w = FindWeapon<MagicMissile>(); if (w != null) { w.Stats.Cooldown = Mathf.Max(0.2f, w.Stats.Cooldown - 0.15f); w.RefreshStats(); } })
			.AddLevel("Base Damage +10. +1 Projectile.", p => { var w = FindWeapon<MagicMissile>(); if (w != null) { w.Stats.Damage += 10; w.Stats.ProjectileCount += 1; } }));

		AvailableUpgrades.Add(new UpgradeData("Axe", UpgradeType.Weapon)
			.AddLevel("UNLOCK: Topór z trajektorią łukową.", p =>
			{
				p.AddWeaponOfType<Axe>(new WeaponStats { Cooldown = 1.5f, Damage = 25, Speed = 400f, Knockback = 300f, Range = 200f, ProjectileCount = 1 });
			})
			.AddLevel("Base Damage +6. +1 Projectile.", p => { var w = FindWeapon<Axe>(); if (w != null) { w.Stats.Damage += 6; w.Stats.ProjectileCount += 1; } })
			.AddLevel("Cooldown -0.2s. Base Damage +6.", p => { var w = FindWeapon<Axe>(); if (w != null) { w.Stats.Damage += 6; w.Stats.Cooldown = Mathf.Max(0.3f, w.Stats.Cooldown - 0.2f); w.RefreshStats(); } })
			.AddLevel("+1 Pierce. Base Damage +6.", p => { var w = FindWeapon<Axe>(); if (w != null) { w.Stats.Damage += 6; w.Stats.Pierce += 1; } })
			.AddLevel("Cooldown -0.2s. +1 Projectile.", p => { var w = FindWeapon<Axe>(); if (w != null) { w.Stats.ProjectileCount += 1; w.Stats.Cooldown = Mathf.Max(0.3f, w.Stats.Cooldown - 0.2f); w.RefreshStats(); } })
			.AddLevel("Base Damage +6. +1 Pierce.", p => { var w = FindWeapon<Axe>(); if (w != null) { w.Stats.Damage += 6; w.Stats.Pierce += 1; } })
			.AddLevel("Cooldown -0.2s. Base Damage +6.", p => { var w = FindWeapon<Axe>(); if (w != null) { w.Stats.Damage += 6; w.Stats.Cooldown = Mathf.Max(0.3f, w.Stats.Cooldown - 0.2f); w.RefreshStats(); } })
			.AddLevel("Base Damage +12. +1 Projectile. +1 Pierce.", p => { var w = FindWeapon<Axe>(); if (w != null) { w.Stats.Damage += 12; w.Stats.ProjectileCount += 1; w.Stats.Pierce += 1; } }));

		AvailableUpgrades.Add(new UpgradeData("Magnet", UpgradeType.Weapon)
			.AddLevel("UNLOCK: Przyciąga pobliskie orby XP.", p =>
			{
				p.AddWeaponOfType<Magnet>(new WeaponStats { Cooldown = 0.01f, Range = 40f });
			})
			.AddLevel("Pull range +20%.", p => { var w = FindWeapon<Magnet>(); if (w != null) { w.Stats.Range *= 1.2f; } })
			.AddLevel("Pull range +20%.", p => { var w = FindWeapon<Magnet>(); if (w != null) { w.Stats.Range *= 1.2f; } })
			.AddLevel("Pull range +30%.", p => { var w = FindWeapon<Magnet>(); if (w != null) { w.Stats.Range *= 1.3f; } })
			.AddLevel("Mega range: cały ekran.", p => { var w = FindWeapon<Magnet>(); if (w != null) { w.Stats.Range = 2000f; } }));
	}

	private T FindWeapon<T>() where T : Weapon
	{
		foreach (var w in Weapons)
			if (w is T found) return found;
		return null;
	}

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

	public int Level = 1;
	public int Xp = 0;
	public int XpToLevel => Mathf.Max(10, (int)(15 * Mathf.Pow(Level, 1.15f)));

	public void GainXp(int amount)
	{
		if (_isDead) return;
		Xp += amount;
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
		Heal(MaxHealth / 4);
	}

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
