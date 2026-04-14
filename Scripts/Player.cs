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

	/// <summary>Scena pocisku używana przez niektóre bronie.</summary>
	[Export] public PackedScene ProjectileScene;
	/// <summary>Podstawowe statystyki startowej broni.</summary>
	[Export] public WeaponStats Weapon;
	/// <summary>Bazowa prędkość ruchu gracza.</summary>
	[Export] public int Speed = 600;
	/// <summary>Maksymalna liczba punktów życia gracza.</summary>
	[Export] public int MaxHealth = 100;
	/// <summary>Czas niewrażliwości po otrzymaniu obrażeń.</summary>
	[Export] public float InvincibilityTime = 0.3f;
	/// <summary>Flaga debugowania rysująca linie do przeciwników.</summary>
	[Export] public bool DebugDrawEnemyLines = false;

	// ── Statystyki i Stan ────────────────────────────────────

	/// <summary>Aktualna liczba punktów życia gracza.</summary>
	public int Health { get; private set; }
	/// <summary>Timer odliczający czas niewrażliwości.</summary>
	private float _invincibilityTimer = 0f;
	/// <summary>Referencja do paska zdrowia w UI.</summary>
	private ProgressBar _hpBar;
	/// <summary>Flaga określająca czy gracz nie żyje.</summary>
	private bool _isDead = false;
	/// <summary>Flaga okrśelająca czy gracz jest w LevelUp UI.</summary>
	public bool IsInLevelUp = false;

	// ── Mnożniki Statystyk ───────────────────────────────────

	/// <summary>Mnożnik zadawanych obrażeń.</summary>
	public float DamageMultiplier = 1f;
	/// <summary>Mnożnik szybkości przeładowania broni.</summary>
	public float CooldownMultiplier = 1f;
	/// <summary>Mnożnik obszaru działania broni.</summary>
	public float AreaMultiplier = 1f;
	/// <summary>Mnożnik prędkości ruchu gracza.</summary>
	public float SpeedMultiplier = 1f;
	/// <summary>Mnożnik prędkości wystrzeliwanych pocisków.</summary>
	public float ProjectileSpeedMultiplier = 1f;

	// ── Ekwipunek i Ulepszenia ───────────────────────────────

	/// <summary>Maksymalna liczba broni w ekwipunku.</summary>
	public const int MAX_WEAPONS = 5;
	/// <summary>Maksymalna liczba przedmiotów pasywnych.</summary>
	public const int MAX_PASSIVES = 5;
	/// <summary>Lista aktualnie posiadanych broni.</summary>
	public List<Weapon> Weapons = new();
	/// <summary>Lista aktualnie posiadanych przedmiotów pasywnych.</summary>
	public List<PassiveData> Passives = new();
	/// <summary>Lista wszystkich dostępnych ulepszeń w grze.</summary>
	public List<UpgradeData> AvailableUpgrades = new();

	// ── Węzły i UI pomocnicze ────────────────────────────────

	/// <summary>Punkt, z którego wystrzeliwane są pociski.</summary>
	public Marker2D ShootPoint;
	/// <summary>Referencja do paska doświadczenia w UI.</summary>
	private ProgressBar xpBar;
	/// <summary>Etykieta tekstowa do celów debugowania.</summary>
	private Label _debugLabel;

	// ── Draw ─────────────────────────────────────────────────

	/// <summary>
	/// Rysuje pomocnicze linie debugowania do aktywnych przeciwników w grupie.
	/// </summary>
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

	/// <summary>
	/// Inicjalizuje statystyki, ulepszenia, UI oraz przygotowuje listę startowych broni.
	/// </summary>
	public override void _Ready()
	{
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
			weapon.ProcessMode = ProcessModeEnum.Pausable;
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

	/// <summary>
	/// Zadaje obrażenia graczowi, uwzględniając niewrażliwość i efekty wizualne.
	/// </summary>
	/// <param name="damage">Liczba punktów obrażeń.</param>
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

	/// <summary>
	/// Przywraca graczowi określoną liczbę punktów życia.
	/// </summary>
	/// <param name="amount">Ilość punktów do uleczenia.</param>
	public void Heal(int amount)
	{
		Health = Mathf.Min(Health + amount, MaxHealth);
		SoundManager.Instance?.PlayHeal();
		UpdateHpBar();
	}

	/// <summary>
	/// Aktualizuje wartości wizualne na pasku zdrowia.
	/// </summary>
	private void UpdateHpBar()
	{
		if (_hpBar == null) return;
		_hpBar.MaxValue = MaxHealth;
		_hpBar.Value = Health;
	}

	/// <summary>
	/// Wykonuje efekt błyśnięcia kolorem podczas otrzymywania obrażeń.
	/// </summary>
	private void FlashDamage()
	{
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate", new Color(1f, 0.2f, 0.2f, 1f), 0.05f);
		tween.TweenProperty(this, "modulate", new Color(1f, 1f, 1f, 1f), 0.15f);
	}

	/// <summary>
	/// Obsługuje śmierć gracza, zatrzymuje grę i wyświetla ekran końcowy.
	/// </summary>
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

	/// <summary>
	/// Pobiera łączną liczbę zabójstw ze wszystkich typów przeciwników.
	/// </summary>
	/// <returns>Suma zabójstw.</returns>
	private int GetKillCount()
	{
		int total = 0;
		foreach (var pair in KillManager.Instance.GetAllKills())
			total += pair.Value;
		return total;
	}

	// ── Bronie / pasywki ─────────────────────────────────────

	/// <summary>
	/// Dodaje nową broń do ekwipunku gracza, jeśli jest miejsce.
	/// </summary>
	/// <param name="weaponScene">Scena nowej broni.</param>
	/// <returns>True jeśli broń została dodana, false jeśli ekwipunek jest pełny.</returns>
	public bool AddWeapon(PackedScene weaponScene)
	{
		if (Weapons.Count >= MAX_WEAPONS) return false;
		var weapon = weaponScene.Instantiate<Weapon>();
		GetNode("Weapons").AddChild(weapon);
		weapon.Init(this);
		weapon.ProcessMode = ProcessModeEnum.Pausable;
		Weapons.Add(weapon);
		return true;
	}

	/// <summary>
	/// Dodaje lub ulepsza przedmiot pasywny.
	/// </summary>
	/// <param name="passive">Dane przedmiotu pasywnego.</param>
	/// <returns>True jeśli operacja się udała.</returns>
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

	/// <summary>
	/// Losuje zestaw dostępnych ulepszeń dla ekranu awansu (Level Up).
	/// </summary>
	/// <param name="count">Liczba ulepszeń do wylosowania.</param>
	/// <returns>Lista wylosowanych danych ulepszeń.</returns>
	public List<UpgradeData> GetUpgradeChoices(int count = 3)
	{
		List<UpgradeData> valid = new();
		foreach (var upgrade in AvailableUpgrades)
			if (upgrade.CanUpgrade) valid.Add(upgrade);
		Shuffle(valid);
		if (valid.Count > count) valid.RemoveRange(count, valid.Count - count);
		return valid;
	}

	/// <summary>
	/// Przemiesza listę elementów przy użyciu generatora liczb losowych.
	/// </summary>
	public void Shuffle<T>(IList<T> list)
	{
		var rng = new RandomNumberGenerator();
		for (int i = list.Count - 1; i > 0; i--)
		{
			int j = rng.RandiRange(0, i);
			(list[i], list[j]) = (list[j], list[i]);
		}
	}

	/// <summary>
	/// Wymusza odświeżenie statystyk wszystkich posiadanych broni.
	/// </summary>
	public void RefreshAllWeapons()
	{
		foreach (var weapon in Weapons)
			weapon.RefreshStats();
	}

	// ── Setup ulepszeń ───────────────────────────────────────

	/// <summary>
	/// Definiuje bazową pulę ulepszeń pasywnych dostępnych w grze.
	/// </summary>

	private void SetupUpgrades()
	{
		// ── Pasywki ──────────────────────────────────────────

		var spinach = new PassiveData { Name = "Spinach", Type = PassiveType.Spinach, MaxLevel = 5, BonusPerLevel = 0.1f };
		AvailableUpgrades.Add(new UpgradeData("Spinach", UpgradeType.Passive)
			.AddLevel("Damage +10%.",          p => AddPassive(spinach))
			.AddLevel("Damage +10%.",          p => AddPassive(spinach))
			.AddLevel("Damage +10%.",          p => AddPassive(spinach))
			.AddLevel("Damage +10%.",          p => AddPassive(spinach))
			.AddLevel("Damage +10%.",          p => AddPassive(spinach)));

		var pummarola = new PassiveData { Name = "Pummarola", Type = PassiveType.Pummarola, MaxLevel = 5, BonusPerLevel = 0.1f };
		AvailableUpgrades.Add(new UpgradeData("Pummarola", UpgradeType.Passive)
			.AddLevel("Cooldown -10%.",        p => AddPassive(pummarola))
			.AddLevel("Cooldown -10%.",        p => AddPassive(pummarola))
			.AddLevel("Cooldown -10%.",        p => AddPassive(pummarola))
			.AddLevel("Cooldown -10%.",        p => AddPassive(pummarola))
			.AddLevel("Cooldown -10%.",        p => AddPassive(pummarola)));

		var hollowHeart = new PassiveData { Name = "Hollow Heart", Type = PassiveType.HollowHeart, MaxLevel = 5, BonusPerLevel = 0.1f };
		AvailableUpgrades.Add(new UpgradeData("Hollow Heart", UpgradeType.Passive)
			.AddLevel("Area +10%.",            p => AddPassive(hollowHeart))
			.AddLevel("Area +10%.",            p => AddPassive(hollowHeart))
			.AddLevel("Area +10%.",            p => AddPassive(hollowHeart))
			.AddLevel("Area +10%.",            p => AddPassive(hollowHeart))
			.AddLevel("Area +10%.",            p => AddPassive(hollowHeart)));

		var bracer = new PassiveData { Name = "Bracer", Type = PassiveType.Bracer, MaxLevel = 5, BonusPerLevel = 0.1f };
		AvailableUpgrades.Add(new UpgradeData("Bracer", UpgradeType.Passive)
			.AddLevel("Projectile Speed +10%.", p => AddPassive(bracer))
			.AddLevel("Projectile Speed +10%.", p => AddPassive(bracer))
			.AddLevel("Projectile Speed +10%.", p => AddPassive(bracer))
			.AddLevel("Projectile Speed +10%.", p => AddPassive(bracer))
			.AddLevel("Projectile Speed +10%.", p => AddPassive(bracer)));

		var wings = new PassiveData { Name = "Wings", Type = PassiveType.Wings, MaxLevel = 5, BonusPerLevel = 0.1f };
		AvailableUpgrades.Add(new UpgradeData("Wings", UpgradeType.Passive)
			.AddLevel("Move Speed +10%.",      p => AddPassive(wings))
			.AddLevel("Move Speed +10%.",      p => AddPassive(wings))
			.AddLevel("Move Speed +10%.",      p => AddPassive(wings))
			.AddLevel("Move Speed +10%.",      p => AddPassive(wings))
			.AddLevel("Move Speed +10%.",      p => AddPassive(wings)));

		// ── Bronie ───────────────────────────────────────────

		var lightning = GetNodeOrNull<Lightning>("Weapons/Lightning");
		if (lightning != null)
		{
			AvailableUpgrades.Add(new UpgradeData("Lightning", UpgradeType.Weapon)
				.AddLevel("Strikes the nearest enemy. Chains to others.", p => { /* Lv1 — bazowy unlock */ })
				.AddLevel("Base Damage +5. +1 chain.",                    p => { lightning.Stats.Damage += 5; lightning.Stats.ProjectileCount += 1; })
				.AddLevel("Cooldown -0.1s. Base Damage +5.",              p => { lightning.Stats.Damage += 5; lightning.Stats.Cooldown = Mathf.Max(0.3f, lightning.Stats.Cooldown - 0.1f); lightning.RefreshStats(); })
				.AddLevel("Base Range +40. +1 chain.",                    p => { lightning.Stats.Range += 40f; lightning.Stats.ProjectileCount += 1; })
				.AddLevel("Base Damage +5. Cooldown -0.1s.",              p => { lightning.Stats.Damage += 5; lightning.Stats.Cooldown = Mathf.Max(0.3f, lightning.Stats.Cooldown - 0.1f); lightning.RefreshStats(); })
				.AddLevel("Base Range +40. +1 chain.",                    p => { lightning.Stats.Range += 40f; lightning.Stats.ProjectileCount += 1; })
				.AddLevel("Base Damage +5. Cooldown -0.1s.",              p => { lightning.Stats.Damage += 5; lightning.Stats.Cooldown = Mathf.Max(0.3f, lightning.Stats.Cooldown - 0.1f); lightning.RefreshStats(); })
				.AddLevel("Base Damage +10. Base Range +40.",             p => { lightning.Stats.Damage += 10; lightning.Stats.Range += 40f; }));
		}

		var garlic = GetNodeOrNull<Garlic>("Weapons/Garlic");
		if (garlic != null)
		{
			AvailableUpgrades.Add(new UpgradeData("Garlic", UpgradeType.Weapon)
				.AddLevel("Damages nearby enemies.",                      p => { /* Lv1 — bazowy unlock */ })
				.AddLevel("Base Area +40%. Base Damage +2.",              p => { garlic.Stats.Range += garlic.Stats.Range * 0.4f; garlic.Stats.Damage += 2; })
				.AddLevel("Cooldown -0.1s. Base Damage +1.",              p => { garlic.Stats.Damage += 1; garlic.Stats.Cooldown = Mathf.Max(0.3f, garlic.Stats.Cooldown - 0.1f); garlic.RefreshStats(); })
				.AddLevel("Base Area +20%. Base Damage +1.",              p => { garlic.Stats.Range += garlic.Stats.Range * 0.2f; garlic.Stats.Damage += 1; })
				.AddLevel("Cooldown -0.1s. Base Damage +2.",              p => { garlic.Stats.Damage += 2; garlic.Stats.Cooldown = Mathf.Max(0.3f, garlic.Stats.Cooldown - 0.1f); garlic.RefreshStats(); })
				.AddLevel("Base Area +20%. Base Damage +1.",              p => { garlic.Stats.Range += garlic.Stats.Range * 0.2f; garlic.Stats.Damage += 1; })
				.AddLevel("Cooldown -0.1s. Base Damage +1.",              p => { garlic.Stats.Damage += 1; garlic.Stats.Cooldown = Mathf.Max(0.3f, garlic.Stats.Cooldown - 0.1f); garlic.RefreshStats(); })
				.AddLevel("Base Area +20%. Base Damage +2.",              p => { garlic.Stats.Range += garlic.Stats.Range * 0.2f; garlic.Stats.Damage += 2; }));
		}

		var firewand = GetNodeOrNull<FireWand>("Weapons/FireWand");
		if (firewand != null)
		{
			AvailableUpgrades.Add(new UpgradeData("Fire Wand", UpgradeType.Weapon)
				.AddLevel("Fires at the nearest enemy.",                  p => { /* Lv1 — bazowy unlock */ })
				.AddLevel("Base Damage +4. +1 Projectile.",               p => { firewand.Stats.Damage += 4; firewand.Stats.ProjectileCount += 1; })
				.AddLevel("Cooldown -0.15s. Base Damage +4.",             p => { firewand.Stats.Damage += 4; firewand.Stats.Cooldown = Mathf.Max(0.1f, firewand.Stats.Cooldown - 0.15f); firewand.RefreshStats(); })
				.AddLevel("+1 Pierce. Base Damage +4.",                   p => { firewand.Stats.Damage += 4; firewand.Stats.Pierce += 1; })
				.AddLevel("Cooldown -0.15s. +1 Projectile.",              p => { firewand.Stats.ProjectileCount += 1; firewand.Stats.Cooldown = Mathf.Max(0.1f, firewand.Stats.Cooldown - 0.15f); firewand.RefreshStats(); })
				.AddLevel("Base Damage +4. +1 Pierce.",                   p => { firewand.Stats.Damage += 4; firewand.Stats.Pierce += 1; })
				.AddLevel("Cooldown -0.15s. Base Damage +4.",             p => { firewand.Stats.Damage += 4; firewand.Stats.Cooldown = Mathf.Max(0.1f, firewand.Stats.Cooldown - 0.15f); firewand.RefreshStats(); })
				.AddLevel("Base Damage +8. +1 Projectile. +1 Pierce.",   p => { firewand.Stats.Damage += 8; firewand.Stats.ProjectileCount += 1; firewand.Stats.Pierce += 1; }));
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

		SoundManager.Instance?.PlayLevelUp();

		var ui = GetTree().CurrentScene.GetNodeOrNull<LevelUpUI>("LevelUpUI");
		ui?.ShowUpgrades(this);
		
		Heal(MaxHealth);
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
	public void SetWeaponsProcessMode(ProcessModeEnum mode)
	{
		foreach (var weapon in Weapons)
			weapon.ProcessMode = mode;
	}
}
