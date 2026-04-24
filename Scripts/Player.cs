using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
	[Export] public PackedScene ProjectileScene;
	[Export] public WeaponStats Weapon;
	[Export] public int Speed = 210;
	[Export] public int MaxHealth = 100;
	[Export] public float InvincibilityTime = 0.28f;
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
		var magnetStats = new WeaponStats { Cooldown = 0.01f, Range = 75f };
		AddWeaponOfType<Magnet>(magnetStats);
		MarkWeaponUnlocked("Magnet");

		switch (SelectedStartWeaponIndex)
		{
			case 0:
				AddWeaponOfType<FireWand>(new WeaponStats { Damage = 14, Cooldown = 0.80f, Speed = 240f, Knockback = 260f, SpreadAngle = 6f, Range = 185f, Pierce = 2 });
				MarkWeaponUnlocked("Fire Wand");
				break;
			case 1:
				AddWeaponOfType<Lightning>(new WeaponStats { Cooldown = 1.4f, Damage = 18, Knockback = 120f, Range = 145f, ProjectileCount = 3 });
				MarkWeaponUnlocked("Lightning");
				break;
			case 2:
				AddWeaponOfType<Garlic>(new WeaponStats { Cooldown = 0.55f, Damage = 6, Knockback = 25f, Range = 82f });
				MarkWeaponUnlocked("Garlic");
				break;
			case 3:
				AddWeaponOfType<MagicMissile>(new WeaponStats { Cooldown = 0.85f, Damage = 13, Speed = 320f, Range = 235f, ProjectileCount = 2 });
				MarkWeaponUnlocked("Magic Missile");
				break;
			case 4:
				AddWeaponOfType<Axe>(new WeaponStats { Cooldown = 1.15f, Damage = 24, Speed = 320f, Knockback = 260f, Range = 240f, ProjectileCount = 1, Pierce = 3, SpreadAngle = 18f });
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
		_invincibilityTimer = InvincibilityTime;

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
		_currentHp.Text = Health + "/" + MaxHealth;
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
				p.AddWeaponOfType<FireWand>(new WeaponStats { Damage = 12, Cooldown = 0.85f, Speed = 240f, Knockback = 260f, SpreadAngle = 6f, Range = 185f, Pierce = 1 });
			})
			.AddLevel("Base Damage +3. Cooldown -0.05s.", p => { var w = FindWeapon<FireWand>(); if (w != null) { w.Stats.Damage += 3; w.Stats.Cooldown = Mathf.Max(0.2f, w.Stats.Cooldown - 0.05f); w.RefreshStats(); } })
			.AddLevel("+1 Projectile.", p => { var w = FindWeapon<FireWand>(); if (w != null) { w.Stats.ProjectileCount += 1; } })
			.AddLevel("+1 Pierce. Base Damage +3.", p => { var w = FindWeapon<FireWand>(); if (w != null) { w.Stats.Damage += 3; w.Stats.Pierce += 1; } })
			.AddLevel("Cooldown -0.08s. Base Damage +3.", p => { var w = FindWeapon<FireWand>(); if (w != null) { w.Stats.Damage += 3; w.Stats.Cooldown = Mathf.Max(0.2f, w.Stats.Cooldown - 0.08f); w.RefreshStats(); } })
			.AddLevel("+1 Projectile. Base Damage +2.", p => { var w = FindWeapon<FireWand>(); if (w != null) { w.Stats.Damage += 2; w.Stats.ProjectileCount += 1; } })
			.AddLevel("+1 Pierce. Cooldown -0.05s.", p => { var w = FindWeapon<FireWand>(); if (w != null) { w.Stats.Pierce += 1; w.Stats.Cooldown = Mathf.Max(0.2f, w.Stats.Cooldown - 0.05f); w.RefreshStats(); } })
			.AddLevel("Base Damage +6. +1 Projectile.", p => { var w = FindWeapon<FireWand>(); if (w != null) { w.Stats.Damage += 6; w.Stats.ProjectileCount += 1; } }));

		AvailableUpgrades.Add(new UpgradeData("Lightning", UpgradeType.Weapon)
			.AddLevel("UNLOCK: Razi najbliższego wroga, skacze po innych.", p =>
			{
				p.AddWeaponOfType<Lightning>(new WeaponStats { Cooldown = 1.7f, Damage = 16, Knockback = 120f, Range = 125f, ProjectileCount = 2 });
			})
			.AddLevel("Base Damage +4. +1 chain.", p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Damage += 4; w.Stats.ProjectileCount += 1; } })
			.AddLevel("Cooldown -0.12s.", p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Cooldown = Mathf.Max(0.4f, w.Stats.Cooldown - 0.12f); w.RefreshStats(); } })
			.AddLevel("Base Range +25. Base Damage +4.", p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Range += 25f; w.Stats.Damage += 4; } })
			.AddLevel("+1 chain.", p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.ProjectileCount += 1; } })
			.AddLevel("Cooldown -0.12s. Base Damage +4.", p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Damage += 4; w.Stats.Cooldown = Mathf.Max(0.4f, w.Stats.Cooldown - 0.12f); w.RefreshStats(); } })
			.AddLevel("Base Range +25.", p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Range += 25f; } })
			.AddLevel("Base Damage +8. +1 chain.", p => { var w = FindWeapon<Lightning>(); if (w != null) { w.Stats.Damage += 8; w.Stats.ProjectileCount += 1; } }));

		AvailableUpgrades.Add(new UpgradeData("Garlic", UpgradeType.Weapon)
			.AddLevel("UNLOCK: Zadaje obrażenia pobliskim wrogom.", p =>
			{
				p.AddWeaponOfType<Garlic>(new WeaponStats { Cooldown = 0.55f, Damage = 6, Knockback = 25f, Range = 82f });
			})
			.AddLevel("Base Area +25%. Base Damage +1.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range *= 1.25f; w.Stats.Damage += 1; } })
			.AddLevel("Cooldown -0.05s. Base Damage +1.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Damage += 1; w.Stats.Cooldown = Mathf.Max(0.25f, w.Stats.Cooldown - 0.05f); w.RefreshStats(); } })
			.AddLevel("Base Area +20%.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range *= 1.2f; } })
			.AddLevel("Base Damage +2.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Damage += 2; } })
			.AddLevel("Cooldown -0.05s. Base Area +20%.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range *= 1.2f; w.Stats.Cooldown = Mathf.Max(0.25f, w.Stats.Cooldown - 0.05f); w.RefreshStats(); } })
			.AddLevel("Base Damage +2.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Damage += 2; } })
			.AddLevel("Base Area +30%. Base Damage +2.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range *= 1.3f; w.Stats.Damage += 2; } }));

		AvailableUpgrades.Add(new UpgradeData("Magic Missile", UpgradeType.Weapon)
			.AddLevel("UNLOCK: Samonaprowadzający pocisk.", p =>
			{
				p.AddWeaponOfType<MagicMissile>(new WeaponStats { Cooldown = 1.0f, Damage = 11, Speed = 320f, Range = 235f, ProjectileCount = 1 });
			})
			.AddLevel("+1 Projectile.", p => { var w = FindWeapon<MagicMissile>(); if (w != null) { w.Stats.ProjectileCount += 1; } })
			.AddLevel("Base Damage +4.", p => { var w = FindWeapon<MagicMissile>(); if (w != null) { w.Stats.Damage += 4; } })
			.AddLevel("Cooldown -0.12s.", p => { var w = FindWeapon<MagicMissile>(); if (w != null) { w.Stats.Cooldown = Mathf.Max(0.2f, w.Stats.Cooldown - 0.12f); w.RefreshStats(); } })
			.AddLevel("+1 Projectile. Base Damage +3.", p => { var w = FindWeapon<MagicMissile>(); if (w != null) { w.Stats.ProjectileCount += 1; w.Stats.Damage += 3; } })
			.AddLevel("Projectile Speed +50. Base Damage +1.", p => { var w = FindWeapon<MagicMissile>(); if (w != null) { w.Stats.Speed += 50f; w.Stats.Damage += 1; } })
			.AddLevel("Cooldown -0.12s. Base Damage +3.", p => { var w = FindWeapon<MagicMissile>(); if (w != null) { w.Stats.Damage += 3; w.Stats.Cooldown = Mathf.Max(0.2f, w.Stats.Cooldown - 0.12f); w.RefreshStats(); } })
			.AddLevel("+1 Projectile. Base Damage +6.", p => { var w = FindWeapon<MagicMissile>(); if (w != null) { w.Stats.Damage += 6; w.Stats.ProjectileCount += 1; } }));

		AvailableUpgrades.Add(new UpgradeData("Axe", UpgradeType.Weapon)
			.AddLevel("UNLOCK: Topór z trajektorią łukową.", p =>
			{
				p.AddWeaponOfType<Axe>(new WeaponStats { Cooldown = 1.35f, Damage = 20, Speed = 320f, Knockback = 260f, Range = 240f, ProjectileCount = 1, Pierce = 2, SpreadAngle = 18f });
			})
			.AddLevel("Base Damage +4. +1 Pierce.", p => { var w = FindWeapon<Axe>(); if (w != null) { w.Stats.Damage += 4; w.Stats.Pierce += 1; } })
			.AddLevel("Cooldown -0.1s.", p => { var w = FindWeapon<Axe>(); if (w != null) { w.Stats.Cooldown = Mathf.Max(0.35f, w.Stats.Cooldown - 0.1f); w.RefreshStats(); } })
			.AddLevel("+1 Projectile.", p => { var w = FindWeapon<Axe>(); if (w != null) { w.Stats.ProjectileCount += 1; } })
			.AddLevel("Base Damage +4. Base Range +10%.", p => { var w = FindWeapon<Axe>(); if (w != null) { w.Stats.Damage += 4; w.Stats.Range *= 1.1f; } })
			.AddLevel("Cooldown -0.1s. +1 Pierce.", p => { var w = FindWeapon<Axe>(); if (w != null) { w.Stats.Pierce += 1; w.Stats.Cooldown = Mathf.Max(0.35f, w.Stats.Cooldown - 0.1f); w.RefreshStats(); } })
			.AddLevel("+1 Projectile. Base Damage +4.", p => { var w = FindWeapon<Axe>(); if (w != null) { w.Stats.Damage += 4; w.Stats.ProjectileCount += 1; } })
			.AddLevel("Base Damage +8. +1 Pierce.", p => { var w = FindWeapon<Axe>(); if (w != null) { w.Stats.Damage += 8; w.Stats.Pierce += 1; } }));

		AvailableUpgrades.Add(new UpgradeData("Magnet", UpgradeType.Weapon)
			.AddLevel("UNLOCK: Przyciąga pobliskie orby XP.", p =>
			{
				p.AddWeaponOfType<Magnet>(new WeaponStats { Cooldown = 0.01f, Range = 75f });
			})
			.AddLevel("Pull range +25%.", p => { var w = FindWeapon<Magnet>(); if (w != null) { w.Stats.Range *= 1.25f; } })
			.AddLevel("Pull speed +20%.", p => { var w = FindWeapon<Magnet>(); if (w != null) { w.PullSpeedBonus += 100f; } })
			.AddLevel("Pull range +25%.", p => { var w = FindWeapon<Magnet>(); if (w != null) { w.Stats.Range *= 1.25f; } })
			.AddLevel("Mega range + speed.", p => { var w = FindWeapon<Magnet>(); if (w != null) { w.Stats.Range = 1600f; w.PullSpeedBonus += 200f; } }));
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
	public int XpToLevel => Mathf.Max(10, Mathf.RoundToInt(13f * Mathf.Pow(Level, 1.08f)));

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
