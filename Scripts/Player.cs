using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Główna klasa gracza. Zarządza zdrowiem, ruchem, systemem broni, pasywek i ulepszeń,
/// a także mechaniką doświadczenia i awansowania na poziomy.
/// Gracz jest identyfikowany w scenie przez grupę "player".
/// </summary>
public partial class Player : CharacterBody2D
{
	// ── Eksporty inspektora ───────────────────────────────────

	/// <summary>Scena bazowego pocisku (używana przez FireWand).</summary>
	[Export] public PackedScene ProjectileScene;

	/// <summary>Statystyki broni (legacy — statystyki aktywnej broni są zarządzane dynamicznie).</summary>
	[Export] public WeaponStats Weapon;

	/// <summary>Bazowa prędkość ruchu gracza (jednostki/s).</summary>
	[Export] public int Speed = 210;

	/// <summary>Maksymalna liczba punktów życia gracza.</summary>
	[Export] public int MaxHealth = 100;

	/// <summary>Czas nietykalności po otrzymaniu obrażeń (sekundy).</summary>
	[Export] public float InvincibilityTime = 0.28f;

	/// <summary>
	/// Gdy włączone, rysuje linie debugowania od gracza do każdego wroga.
	/// Dostępne tylko w trybie deweloperskim.
	/// </summary>
	[Export] public bool DebugDrawEnemyLines = false;

	/// <summary>Scena pocisku pioruna (<see cref="LightningBeam"/>), wymagana przez broń Lightning.</summary>
	[Export] public PackedScene LightningBeamScene;

	/// <summary>Scena pocisku Magic Missile (<see cref="MagicMissileProjectile"/>).</summary>
	[Export] public PackedScene MagicMissileProjectileScene;

	/// <summary>Scena pocisku topora (<see cref="AxeProjectile"/>).</summary>
	[Export] public PackedScene AxeProjectileScene;

	// ── Stan zdrowia i nietykalności ─────────────────────────

	/// <summary>Aktualna liczba punktów życia gracza.</summary>
	public int Health { get; private set; }

	/// <summary>Pozostały czas nietykalności w sekundach. 0 = gracz może być trafiony.</summary>
	private float _invincibilityTimer = 0f;

	/// <summary>Pasek postępu HP wyświetlany w UI.</summary>
	private ProgressBar _hpBar;

	/// <summary>Etykieta tekstowa wyświetlająca aktualne HP w formacie "HP/MaxHP".</summary>
	private Label _currentHp;

	/// <summary>Flaga martwego gracza — zapobiega wielokrotnemu wywołaniu <see cref="Die"/>.</summary>
	private bool _isDead = false;

	/// <summary>
	/// Flaga aktywna podczas wyświetlania ekranu wyboru ulepszenia.
	/// Blokuje otrzymywanie obrażeń podczas Level Up UI.
	/// </summary>
	public bool IsInLevelUp = false;

	// ── Mnożniki statystyk ────────────────────────────────────

	/// <summary>Mnożnik obrażeń ze wszystkich broni gracza. Modyfikowany przez pasywkę Spinach.</summary>
	public float DamageMultiplier = 1f;

	/// <summary>Mnożnik czasu odnowienia broni (wartości poniżej 1 skracają cooldown). Modyfikowany przez Pummarola.</summary>
	public float CooldownMultiplier = 1f;

	/// <summary>Mnożnik zasięgu/obszaru działania broni. Modyfikowany przez Hollow Heart.</summary>
	public float AreaMultiplier = 1f;

	/// <summary>Mnożnik prędkości ruchu gracza. Modyfikowany przez pasywkę Wings.</summary>
	public float SpeedMultiplier = 1f;

	/// <summary>Mnożnik prędkości pocisków. Modyfikowany przez pasywkę Bracer.</summary>
	public float ProjectileSpeedMultiplier = 1f;

	// ── Limity ekwipunku ──────────────────────────────────────

	/// <summary>Maksymalna liczba broni, które gracz może posiadać jednocześnie.</summary>
	public const int MAX_WEAPONS = 6;

	/// <summary>Maksymalna liczba pasywek, które gracz może posiadać jednocześnie.</summary>
	public const int MAX_PASSIVES = 6;

	// ── Kolekcje broni i ulepszeń ─────────────────────────────

	/// <summary>Lista aktywnych broni posiadanych przez gracza.</summary>
	public List<Weapon> Weapons = new();

	/// <summary>Lista aktywnych pasywek posiadanych przez gracza.</summary>
	public List<PassiveData> Passives = new();

	/// <summary>
	/// Lista wszystkich możliwych ulepszeń (bronie i pasywki).
	/// Każda pozycja śledzi aktualny poziom i może być aplikowana przez <see cref="LevelUpUI"/>.
	/// </summary>
	public List<UpgradeData> AvailableUpgrades = new();

	// ── Węzły sceny ───────────────────────────────────────────

	/// <summary>Marker2D wyznaczający punkt spawnu pocisków (np. koniec różdżki).</summary>
	public Marker2D ShootPoint;

	/// <summary>Pasek postępu XP wyświetlany w UI.</summary>
	private ProgressBar xpBar;

	/// <summary>Etykieta debugowania pokazująca odległości do wrogów (aktywna przy <see cref="DebugDrawEnemyLines"/>).</summary>
	private Label _debugLabel;

	/// <summary>Panel ekwipunku wyświetlający posiadane bronie i pasywki.</summary>
	private EquipmentUI _equipmentUI;

	// ── Statyczny wybór startowej broni ──────────────────────

	/// <summary>
	/// Indeks startowej broni wybrany przez gracza w <see cref="WeaponSelectUI"/>.
	/// Utrzymywany jako statyczny, aby przetrwał zmianę sceny.
	/// </summary>
	public static int SelectedStartWeaponIndex = 0;

	// ── Rysowanie debugowania ─────────────────────────────────

	/// <summary>
	/// Rysuje linie debugowania od gracza do każdego wroga (jeśli włączone przez <see cref="DebugDrawEnemyLines"/>).
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

	/// <summary>
	/// Inicjalizacja gracza po dodaniu do sceny.
	/// Pobiera węzły UI, inicjalizuje zdrowie, czyści predefiniowane bronie ze sceny (.tscn),
	/// konfiguruje ulepszenia i dodaje broń startową.
	/// </summary>
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

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

	// ── Zarządzanie bronią ────────────────────────────────────

	/// <summary>
	/// Dynamicznie dodaje broń podanego typu do ekwipunku gracza.
	/// Zwraca <c>false</c> jeśli gracz osiągnął limit broni lub już posiada broń tego typu.
	/// Automatycznie ustawia sceny pocisków, inicjalizuje broń i odświeża UI.
	/// </summary>
	/// <typeparam name="T">Typ broni dziedziczący po <see cref="Weapon"/>.</typeparam>
	/// <param name="stats">Opcjonalne statystyki startowe; jeśli <c>null</c>, broń używa własnych domyślnych.</param>
	/// <returns><c>true</c> jeśli broń została dodana; <c>false</c> w przeciwnym razie.</returns>
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

	/// <summary>
	/// Przypisuje odpowiednie sceny pocisków do broni wymagających PackedScene.
	/// Obsługuje FireWand, Lightning, MagicMissile i Axe.
	/// </summary>
	/// <param name="weapon">Broń, której sceny pocisków mają być ustawione.</param>
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

	/// <summary>
	/// Dodaje startowe bronie gracza na podstawie <see cref="SelectedStartWeaponIndex"/>.
	/// Magnet jest zawsze dodawany jako darmowy starter.
	/// Wybrana broń startowa (Fire Wand, Lightning, Garlic, Magic Missile lub Axe)
	/// jest inicjalizowana z predefiniowanymi statystykami i oznaczana jako odblokowana.
	/// </summary>
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
				AddWeaponOfType<Garlic>(new WeaponStats { Cooldown = 0.55f, Damage = 6, Knockback = 25f, Range = 50f });
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

	/// <summary>
	/// Ustawia poziom broni o podanej nazwie w <see cref="AvailableUpgrades"/> na 1,
	/// oznaczając ją jako odblokowaną (posiadaną przez gracza).
	/// </summary>
	/// <param name="name">Nazwa broni zgodna z <see cref="UpgradeData.Name"/>.</param>
	private void MarkWeaponUnlocked(string name)
	{
		foreach (var upg in AvailableUpgrades)
			if (upg.Name == name && upg.Type == UpgradeType.Weapon)
			{
				upg.Level = 1;
				break;
			}
	}

	// ── Zdrowie i obrażenia ───────────────────────────────────

	/// <summary>
	/// Aplikuje obrażenia na gracza.
	/// Nie działa podczas Level Up UI, nietykalności ani po śmierci gracza.
	/// Obrażenia są podwajane (balansowanie trudności), po czym uruchamia
	/// efekt wizualny i sprawdza warunek śmierci.
	/// </summary>
	/// <param name="damage">Bazowe obrażenia do zadania (przed podwojeniem).</param>
	public void TakeDamage(int damage)
	{
		if (IsInLevelUp) return;
		if (_invincibilityTimer > 0f || _isDead) return;

		damage *= 2;

		Health -= damage;
		_invincibilityTimer = InvincibilityTime;

		SoundManager.Instance?.PlayHurt();
		UpdateHpBar();
		FlashDamage();

		if (Health <= 0) { Health = 0; Die(); }
	}

	/// <summary>
	/// Leczy gracza o podaną liczbę punktów życia (nie przekraczając <see cref="MaxHealth"/>).
	/// Odgrywa dźwięk leczenia i aktualizuje pasek HP.
	/// </summary>
	/// <param name="amount">Liczba punktów życia do przywrócenia.</param>
	public void Heal(int amount)
	{
		Health = Mathf.Min(Health + amount, MaxHealth);
		SoundManager.Instance?.PlayHeal();
		UpdateHpBar();
	}

	/// <summary>
	/// Aktualizuje pasek HP i etykietę tekstową UI na podstawie aktualnego zdrowia.
	/// </summary>
	private void UpdateHpBar()
	{
		if (_hpBar == null) return;
		_hpBar.MaxValue = MaxHealth;
		_hpBar.Value = Health;
		_currentHp.Text = Health + "/" + MaxHealth;
	}

	/// <summary>
	/// Krótki efekt wizualny (czerwony błysk) po otrzymaniu obrażeń.
	/// Używa Tweena do animacji koloru modulate.
	/// </summary>
	private void FlashDamage()
	{
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate", new Color(1f, 0.2f, 0.2f, 1f), 0.05f);
		tween.TweenProperty(this, "modulate", new Color(1f, 1f, 1f, 1f), 0.15f);
	}

	/// <summary>
	/// Obsługuje śmierć gracza.
	/// Wyłącza wszystkie bronie, pauzuje grę i wywołuje ekran śmierci.
	/// Jeśli ekran śmierci nie istnieje w scenie, powraca do menu po 1,5 sekundy.
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
	/// Zwraca całkowitą liczbę zabójstw ze wszystkich typów wrogów (z <see cref="KillManager"/>).
	/// </summary>
	/// <returns>Suma zabójstw ze wszystkich typów wrogów.</returns>
	private int GetKillCount()
	{
		int total = 0;
		foreach (var pair in KillManager.Instance.GetAllKills())
			total += pair.Value;
		return total;
	}

	// ── Pasywki ───────────────────────────────────────────────

	/// <summary>
	/// Dodaje lub ulepsza pasywkę gracza.
	/// Sprawdza limit pasywek (<see cref="MAX_PASSIVES"/>), aplikuje efekt pasywki
	/// i odświeża UI ekwipunku.
	/// </summary>
	/// <param name="passive">Dane pasywki do dodania lub ulepszenia.</param>
	/// <returns><c>true</c> jeśli pasywka została zastosowana; <c>false</c> jeśli nie było to możliwe.</returns>
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

	/// <summary>
	/// Wywołuje <see cref="Weapon.RefreshStats"/> na wszystkich broniach gracza.
	/// Powinno być wywoływane po zmianie mnożników statystyk (np. po kupieniu pasywki).
	/// </summary>
	public void RefreshAllWeapons()
	{
		foreach (var weapon in Weapons)
			weapon.RefreshStats();
	}

	/// <summary>
	/// Odświeża panel ekwipunku UI (<see cref="EquipmentUI"/>) odzwierciedlając aktualny stan gracza.
	/// Jeśli referencja do UI jest <c>null</c>, próbuje ją pobrać ponownie ze sceny.
	/// </summary>
	public void RefreshEquipmentUI()
	{
		if (_equipmentUI == null)
			_equipmentUI = GetTree().CurrentScene.GetNodeOrNull<EquipmentUI>("CanvasLayer/EquipmentUI");
		_equipmentUI?.Refresh(this);
	}

	/// <summary>
	/// Miesza listę w miejscu algorytmem Fisher-Yates.
	/// Używana do losowania kolejności ulepszeń w <see cref="LevelUpUI"/>.
	/// </summary>
	/// <typeparam name="T">Typ elementów listy.</typeparam>
	/// <param name="list">Lista do przetasowania.</param>
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
	/// Konfiguruje wszystkie dostępne ulepszenia (bronie i pasywki) dla systemu Level Up.
	/// Każde ulepszenie jest definiowane za pomocą fluent API <see cref="UpgradeData.AddLevel"/>
	/// z opisem i efektem dla każdego poziomu.
	/// </summary>
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

		var hollowHeart = new PassiveData { Name = "Hollow Heart", Type = PassiveType.HollowHeart, MaxLevel = 5, BonusPerLevel = 0.05f };
		AvailableUpgrades.Add(new UpgradeData("Hollow Heart", UpgradeType.Passive)
			.AddLevel("Area +5%.", p => AddPassive(hollowHeart))
			.AddLevel("Area +5%.", p => AddPassive(hollowHeart))
			.AddLevel("Area +5%.", p => AddPassive(hollowHeart))
			.AddLevel("Area +5%.", p => AddPassive(hollowHeart))
			.AddLevel("Area +5%.", p => AddPassive(hollowHeart)));

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
				p.AddWeaponOfType<Garlic>(new WeaponStats { Cooldown = 0.55f, Damage = 6, Knockback = 25f, Range = 50f });
			})
			.AddLevel("Base Area +10%. Base Damage +1.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range *= 1.10f; w.Stats.Damage += 1; } })
			.AddLevel("Cooldown -0.05s. Base Damage +1.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Damage += 1; w.Stats.Cooldown = Mathf.Max(0.25f, w.Stats.Cooldown - 0.05f); w.RefreshStats(); } })
			.AddLevel("Base Area +5%. Base Damage +1.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range *= 1.05f; w.Stats.Damage += 1; } })
			.AddLevel("Base Damage +2.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Damage += 2; } })
			.AddLevel("Cooldown -0.05s. Base Area +5%.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range *= 1.05f; w.Stats.Cooldown = Mathf.Max(0.25f, w.Stats.Cooldown - 0.05f); w.RefreshStats(); } })
			.AddLevel("Base Damage +2.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Damage += 2; } })
			.AddLevel("Base Area +10%. Base Damage +2.", p => { var w = FindWeapon<Garlic>(); if (w != null) { w.Stats.Range *= 1.10f; w.Stats.Damage += 2; } }));

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
			.AddLevel("Pull range +15%.", p => { var w = FindWeapon<Magnet>(); if (w != null) { w.Stats.Range *= 1.15f; } })
			.AddLevel("Pull speed +20%.", p => { var w = FindWeapon<Magnet>(); if (w != null) { w.PullSpeedBonus += 100f; } })
			.AddLevel("Pull range +15%.", p => { var w = FindWeapon<Magnet>(); if (w != null) { w.Stats.Range *= 1.15f; } })
			.AddLevel("Pull range +30%. Pull speed +20%.", p => { var w = FindWeapon<Magnet>(); if (w != null) { w.Stats.Range *= 1.30f; w.PullSpeedBonus += 100f; } }));
	}

	/// <summary>
	/// Zwraca pierwszą broń podanego typu z listy <see cref="Weapons"/> gracza,
	/// lub <c>null</c> jeśli gracz jej nie posiada.
	/// </summary>
	/// <typeparam name="T">Typ broni dziedziczący po <see cref="Weapon"/>.</typeparam>
	/// <returns>Instancja broni lub <c>null</c>.</returns>
	private T FindWeapon<T>() where T : Weapon
	{
		foreach (var w in Weapons)
			if (w is T found) return found;
		return null;
	}

	// ── Wyszukiwanie wrogów ───────────────────────────────────

	/// <summary>
	/// Zwraca najbliższego wroga w podanym zasięgu od pozycji gracza.
	/// </summary>
	/// <param name="range">Maksymalny zasięg poszukiwania (jednostki świata).</param>
	/// <returns>Najbliższy <see cref="Node2D"/> wroga lub <c>null</c>, jeśli brak celów w zasięgu.</returns>
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

	// ── Doświadczenie i poziomy ───────────────────────────────

	/// <summary>Aktualny poziom gracza. Zaczyna od 1.</summary>
	public int Level = 1;

	/// <summary>Aktualna liczba punktów doświadczenia w bieżącym poziomie.</summary>
	public int Xp = 0;

	/// <summary>
	/// Liczba punktów XP wymagana do awansu na następny poziom.
	/// Skaluje się nieliniowo z poziomem — <c>13 * Level^1.08</c>, minimum 10.
	/// </summary>
	public int XpToLevel => Mathf.Max(10, Mathf.RoundToInt(13f * Mathf.Pow(Level, 1.08f)));

	/// <summary>
	/// Dodaje punkty doświadczenia i obsługuje wielokrotne awanse poziomu w jednym wywołaniu.
	/// Wywołuje <see cref="LevelUp"/> dla każdego progu przekroczonego jednorazowo.
	/// </summary>
	/// <param name="amount">Liczba punktów XP do dodania.</param>
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

	/// <summary>
	/// Aktualizuje pasek XP w UI do aktualnych wartości <see cref="Xp"/> i <see cref="XpToLevel"/>.
	/// </summary>
	private void UpdateXpBar()
	{
		if (xpBar == null) return;
		xpBar.MaxValue = XpToLevel;
		xpBar.Value = Xp;
	}

	/// <summary>
	/// Obsługuje awans na kolejny poziom: zwiększa <see cref="Level"/>,
	/// odtwarza dźwięk, wyświetla <see cref="LevelUpUI"/> i leczy gracza o 25% maksymalnego HP.
	/// </summary>
	private void LevelUp()
	{
		Level++;
		SoundManager.Instance?.PlayLevelUp();
		var ui = GetTree().CurrentScene.GetNodeOrNull<LevelUpUI>("LevelUpUI");
		ui?.ShowUpgrades(this);
		Heal(MaxHealth / 4);
	}

	// ── Ruch ─────────────────────────────────────────────────

	/// <summary>
	/// Pobiera wejście gracza i aktualizuje wektor <see cref="CharacterBody2D.Velocity"/>.
	/// Gdy gra jest pauzowana lub gracz jest martwy, prędkość jest zerowana.
	/// Prędkość skalowana jest przez <see cref="Speed"/> i <see cref="SpeedMultiplier"/>.
	/// </summary>
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

	/// <summary>
	/// Aktualizacja fizyki: zmniejsza timer nietykalności, pobiera wejście gracza i wywołuje MoveAndSlide.
	/// </summary>
	/// <param name="delta">Czas od poprzedniej klatki fizyki (sekundy).</param>
	public override void _PhysicsProcess(double delta)
	{
		if (_invincibilityTimer > 0f)
			_invincibilityTimer -= (float)delta;

		GetInput();
		MoveAndSlide();
	}

	/// <summary>
	/// Aktualizacja logiki co klatkę: odświeża linie debugowania i wymusza przerysowanie
	/// (tylko gdy <see cref="DebugDrawEnemyLines"/> jest włączone).
	/// </summary>
	/// <param name="delta">Czas od poprzedniej klatki (sekundy).</param>
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

	/// <summary>
	/// Zmienia tryb procesowania (<see cref="Node.ProcessModeEnum"/>) dla wszystkich broni gracza.
	/// Używane do zatrzymywania broni podczas pauzy lub Level Up UI.
	/// </summary>
	/// <param name="mode">Docelowy tryb procesowania.</param>
	public void SetWeaponsProcessMode(ProcessModeEnum mode)
	{
		foreach (var weapon in Weapons)
			weapon.ProcessMode = mode;
	}
}
