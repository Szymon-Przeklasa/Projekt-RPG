using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

/// <summary>
/// Klasa reprezentująca gracza w grze.
/// Dziedziczy po CharacterBody2D.
/// Zarządza broniami, ulepszeniami, doświadczeniem i poziomami gracza.
/// </summary>
public partial class Player : CharacterBody2D
{
	/// <summary>
	/// Scena pocisku używana przez broń gracza.
	/// </summary>
	[Export] public PackedScene ProjectileScene;

	/// <summary>
	/// Statystyki aktualnie wybranej broni.
	/// </summary>
	[Export] public WeaponStats Weapon;

	/// <summary>
	/// Prędkość poruszania się gracza.
	/// </summary>
	[Export] public int Speed = 600;



	/// <summary>
	/// Lista dostępnych ulepszeń dla gracza.
	/// </summary>
	public List<UpgradeData> AvailableUpgrades = new();

	/// <summary>
	/// Globalny mnożnik obrażeń używany przez bronie.
	/// </summary>
	public float DamageMultiplier = 1f;

	/// <summary>
	/// Globalny mnożnik czasu odnowienia umiejętności.
	/// </summary>
	public float CooldownMultiplier = 1f;

	/// <summary>
	/// Globalny mnożnik zasięgu efektów obszarowych.
	/// </summary>
	public float AreaMultiplier = 1f;

	/// <summary>
	/// Globalny mnożnik prędkości.
	/// </summary>
	public float SpeedMultiplier = 1f;

	/// <summary>
	/// Maksymalna liczba broni gracza.
	/// </summary>
	public const int MAX_WEAPONS = 5;

	/// <summary>
	/// Maksymalna liczba pasywnych ulepszeń gracza.
	/// </summary>
	public const int MAX_PASSIVES = 5;

	/// <summary>
	/// Lista aktualnych broni gracza.
	/// </summary>
	public List<Weapon> Weapons = new();

	/// <summary>
	/// Lista pasywnych ulepszeń gracza.
	/// </summary>
	public List<UpgradeData> PassiveUpgrades = new();

	/// <summary>
	/// Punkt, z którego gracz strzela pociskami.
	/// </summary>
	public Marker2D ShootPoint;
	
	private ProgressBar xpBar;

	[Export] public bool DebugDrawEnemyLines = true;
	

	public override void _Draw()
	{
		if (!DebugDrawEnemyLines) return;

		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Enemy enemy)
			{
				// Pozycja względem gracza (Draw działa w lokalnych współrzędnych)
				Vector2 localPos = ToLocal(enemy.GlobalPosition);
				float dist = GlobalPosition.DistanceTo(enemy.GlobalPosition);

				// Linia do przeciwnika
				DrawLine(Vector2.Zero, localPos, new Color(1f, 0.2f, 0.2f, 0.6f), 1f);

				// Punkt na przeciwniku
				DrawCircle(localPos, 4f, Colors.Red);

				// Tekst z odległością — rysujemy przez Label nad graczem
				// (DrawString wymaga Font, więc użyjemy osobnego node'a)
			}
		}
	}

	/// <summary>
	/// Metoda wywoływana po załadowaniu sceny.
	/// Inicjalizuje bronie i dostępne ulepszenia.
	/// </summary>
	private Label _debugLabel;
	public override void _Ready()
	{
		SetupUpgrades();
		GD.Print("Upgrades count: ", AvailableUpgrades.Count);

		ShootPoint = GetNode<Marker2D>("ShootPoint");
		
		xpBar = GetTree().CurrentScene.GetNode<ProgressBar>("CanvasLayer/XPBar");

		foreach (Weapon weapon in GetNode("Weapons").GetChildren())
		{
			weapon.Init(this);
			Weapons.Add(weapon);
		}

				// Debug label node — tworzony raz
		

			// Na końcu _Ready():
		_debugLabel = new Label();
		_debugLabel.ZIndex = 20;
		_debugLabel.Position = new Vector2(20, -80);
		_debugLabel.AddThemeColorOverride("font_color", Colors.Cyan);
		_debugLabel.AddThemeFontSizeOverride("font_size", 10);
		AddChild(_debugLabel);
		
		UpdateXpBar();
	}

	/// <summary>
	/// Dodaje nową broń do gracza.
	/// </summary>
	/// <param name="weaponScene">Scena broni do dodania.</param>
	/// <returns>True, jeśli broń została dodana, false jeśli osiągnięto limit.</returns>
	public bool AddWeapon(PackedScene weaponScene)
	{
		if (Weapons.Count >= MAX_WEAPONS)
			return false;

		var weapon = weaponScene.Instantiate<Weapon>();
		GetNode("Weapons").AddChild(weapon);
		weapon.Init(this);
		Weapons.Add(weapon);

		return true;
	}

	/// <summary>
	/// Dodaje pasywne ulepszenie do gracza.
	/// </summary>
	/// <param name="upgrade">Ulepszenie do dodania.</param>
	/// <returns>True, jeśli ulepszenie zostało dodane, false jeśli osiągnięto limit.</returns>
	public bool AddPassive(UpgradeData upgrade)
	{
		if (PassiveUpgrades.Count >= MAX_PASSIVES)
			return false;

		PassiveUpgrades.Add(upgrade);
		upgrade.Apply(this);

		return true;
	}

	/// <summary>
	/// Zwraca losową listę dostępnych ulepszeń do wyboru.
	/// </summary>
	/// <param name="count">Liczba ulepszeń do zwrócenia (domyślnie 3).</param>
	/// <returns>Lista dostępnych ulepszeń gracza.</returns>
	public List<UpgradeData> GetUpgradeChoices(int count = 3)
	{
		List<UpgradeData> valid = new();
		foreach (var upgrade in AvailableUpgrades)
		{
			if (upgrade.CanUpgrade)
				valid.Add(upgrade);
		}

		Shuffle(valid);

		if (valid.Count > count)
			valid.RemoveRange(count, valid.Count - count);

		return valid;
	}

	/// <summary>
	/// Tasuje elementy w liście w miejscu.
	/// </summary>
	/// <typeparam name="T">Typ elementów w liście.</typeparam>
	/// <param name="list">Lista do tasowania.</param>
	public void Shuffle<T>(IList<T> list)
	{
		RandomNumberGenerator rng = new RandomNumberGenerator();
		for (int i = list.Count - 1; i > 0; i--)
		{
			int j = rng.RandiRange(0, i);
			(list[i], list[j]) = (list[j], list[i]);
		}
	}

	// Nowe pole — mnożnik prędkości pocisków
	public float ProjectileSpeedMultiplier = 1f;

	// Lista pasywnych (zmień typ z UpgradeData na PassiveData)
	public List<PassiveData> Passives = new();

	/// <summary>
	/// Dodaje pasywną umiejętność.
	/// </summary>
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
	/// Odświeża timery i prędkości wszystkich broni po zmianie statystyk gracza.
	/// </summary>
	public void RefreshAllWeapons()
	{
		foreach (var weapon in Weapons)
			weapon.RefreshStats();
	}

	/// <summary>
	/// Inicjalizuje dostępne ulepszenia gracza.
	/// Dodaje ulepszenia dla konkretnych broni oraz globalne statystyki.
	/// </summary>
	private void SetupUpgrades()
	{
		// ── PASYWY ──────────────────────────────────────────────
		var spinach = new PassiveData
		{
			Name = "Spinach",
			Description = "+10% damage",
			Type = PassiveType.Spinach,
			MaxLevel = 5,
			BonusPerLevel = 0.1f
		};
		var pummarola = new PassiveData
		{
			Name = "Pummarola",
			Description = "-10% cooldown",
			Type = PassiveType.Pummarola,
			MaxLevel = 5,
			BonusPerLevel = 0.1f
		};
		var hollowHeart = new PassiveData
		{
			Name = "Hollow Heart",
			Description = "+10% area range",
			Type = PassiveType.HollowHeart,
			MaxLevel = 5,
			BonusPerLevel = 0.1f
		};
		var bracer = new PassiveData
		{
			Name = "Bracer",
			Description = "+10% projectile speed",
			Type = PassiveType.Bracer,
			MaxLevel = 5,
			BonusPerLevel = 0.1f
		};
		var wings = new PassiveData
		{
			Name = "Wings",
			Description = "+10% move speed",
			Type = PassiveType.Wings,
			MaxLevel = 5,
			BonusPerLevel = 0.1f
		};

		// Pasywne jako ulepszenia
		AvailableUpgrades.Add(new UpgradeData("Spinach", UpgradeType.Stat,
			(p) => AddPassive(spinach), 5));
		AvailableUpgrades.Add(new UpgradeData("Pummarola", UpgradeType.Stat,
			(p) => AddPassive(pummarola), 5));
		AvailableUpgrades.Add(new UpgradeData("Hollow Heart", UpgradeType.Stat,
			(p) => AddPassive(hollowHeart), 5));
		AvailableUpgrades.Add(new UpgradeData("Bracer", UpgradeType.Stat,
			(p) => AddPassive(bracer), 5));
		AvailableUpgrades.Add(new UpgradeData("Wings", UpgradeType.Stat,
			(p) => AddPassive(wings), 5));

		// ── ULEPSZENIA BRONI ─────────────────────────────────────
		var lightning = GetNodeOrNull<Lightning>("Weapons/Lightning");
		if (lightning != null)
		{
			AvailableUpgrades.Add(new UpgradeData("Lightning: +5 DMG", UpgradeType.Weapon,
				(p) => { lightning.Stats.Damage += 5; }, 8));
			AvailableUpgrades.Add(new UpgradeData("Lightning: -0.2s cooldown", UpgradeType.Weapon,
				(p) => { lightning.Stats.Cooldown = Mathf.Max(0.3f, lightning.Stats.Cooldown - 0.2f); lightning.RefreshStats(); }, 5));
			AvailableUpgrades.Add(new UpgradeData("Lightning: +1 chains", UpgradeType.Weapon,
				(p) => { lightning.Stats.ProjectileCount += 1; }, 4));
			AvailableUpgrades.Add(new UpgradeData("Lightning: +150 range", UpgradeType.Weapon,
				(p) => { lightning.Stats.Range += 150f; }, 4));
		}

		var garlic = GetNodeOrNull<Garlic>("Weapons/Garlic");
		if (garlic != null)
		{
			AvailableUpgrades.Add(new UpgradeData("Garlic: +3 DMG", UpgradeType.Weapon,
				(p) => { garlic.Stats.Damage += 3; }, 8));
			AvailableUpgrades.Add(new UpgradeData("Garlic: +100 range", UpgradeType.Weapon,
				(p) => { garlic.Stats.Range += 100f; }, 5));
			AvailableUpgrades.Add(new UpgradeData("Garlic: -0.2s cooldown", UpgradeType.Weapon,
				(p) => { garlic.Stats.Cooldown = Mathf.Max(0.3f, garlic.Stats.Cooldown - 0.2f); garlic.RefreshStats(); }, 4));
		}

		var firewand = GetNodeOrNull<FireWand>("Weapons/FireWand");
		if (firewand != null)
		{
			AvailableUpgrades.Add(new UpgradeData("Fire Wand: +4 DMG", UpgradeType.Weapon,
				(p) => { firewand.Stats.Damage += 4; }, 8));
			AvailableUpgrades.Add(new UpgradeData("Fire Wand: +1 projectile", UpgradeType.Weapon,
				(p) => { firewand.Stats.ProjectileCount += 1; }, 4));
			AvailableUpgrades.Add(new UpgradeData("Fire Wand: +1 pierce", UpgradeType.Weapon,
				(p) => { firewand.Stats.Pierce += 1; }, 4));
			AvailableUpgrades.Add(new UpgradeData("Fire Wand: -0.15s cooldown", UpgradeType.Weapon,
				(p) => { firewand.Stats.Cooldown = Mathf.Max(0.1f, firewand.Stats.Cooldown - 0.15f); firewand.RefreshStats(); }, 5));
		}
	}
			//---

			//## Tabela balansowania bazowych statystyk

			//| Broń         | Damage | Cooldown | ProjectileCount | Range | Pierce | Speed |
			//               |--------|----------|-----------------|-------|--------|-------|---|
			//| FireWand     | 10     | 0.8s     | 1               | 500   | 1      | 600   |
			//| Lightning    | 20     | 1.5s     | 3 łańcuchy	   | 400   | —      | —     |
			//| Garlic       | 5	  | 0.5s     | —			   | 200   | —      | —     |
			//| Axe          | 15     | 1.2s     | 1			   | 600   | 2      | 400   |
			//| MagicMissile | 12     | 1.0s     | 1			   | 700   | 1      | 250   |

			//---

			//## Jak to działa razem (flow skalowania)
			//```
			//Gracz podnosi Szpinak(Spinach lvl 1)
			//→ player.DamageMultiplier += 0.1  (teraz 1.1)
			//→ player.RefreshAllWeapons()
			//→ każda broń w Fire() wywołuje GetDamage()
			//→ GetDamage() = Stats.Damage* Player.DamageMultiplier
			//→ wszystkie bronie automatycznie zadają 10% więcej DMG


			//Gracz podnosi Hollow Heart (lvl 3)
			//→ player.AreaMultiplier = 1.3
			//→ Garlic.Fire() wywołuje GetRange() = Stats.Range* 1.3
			//→ aura czosnku jest 30% większa


	/// <summary>
	/// Zwraca najbliższego wroga w określonym zasięgu.
	/// </summary>
	/// <param name="range">Maksymalny zasięg wyszukiwania.</param>
	/// <returns>Najbliższy wróg typu Node2D lub null, jeśli brak wrogów w zasięgu.</returns>
	public Node2D GetClosestEnemy(float range)
	{
		Node2D closest = null;
		float best = range;

		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Node2D enemy)
			{
				float d = GlobalPosition.DistanceTo(enemy.GlobalPosition);
				if (d < best)
				{
					best = d;
					closest = enemy;
				}
			}
		}
		return closest;
	}

	// ── XP curve ─────────────────────────────────────────────────
	// XpToLevel[lvl] = floor(4 * lvl^1.8)  — wolno na początku, szybko potem
	// Ale wystarczy prosta tablica hard-coded dla czytelności:
	private static readonly int[] XpTable = {
	//  lvl:  1   2   3   4   5   6   7   8   9  10
			   5, 10, 18, 28, 40, 55, 72, 92,115,140,
	// 11-20
			  170,202,238,278,322,370,422,478,540,608,
	// 21-30
			  680,760,845,940,1040,1150,1270,1400,1540,1700
	};

	public int Level = 1;
	public int Xp = 0;
	public int XpToLevel => Level - 1 < XpTable.Length ? XpTable[Level - 1] : Level * 80;

	public void GainXp(int amount)
	{
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
		GD.Print($"LEVEL UP! → {Level}");

		var ui = GetTree().CurrentScene.GetNodeOrNull<LevelUpUI>("LevelUpUI");
		if (ui != null)
			ui.ShowUpgrades(this);
	}

	/// <summary>
	/// Odczytuje wejście gracza i ustawia wektor prędkości.
	/// </summary>
	public void GetInput()
	{
		Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");
		Velocity = inputDirection * Speed;
	}

	/// <summary>
	/// Metoda wywoływana w każdej klatce fizyki.
	/// Obsługuje pauzę gry oraz poruszanie graczem.
	/// </summary>
	/// <param name="delta">Czas od ostatniej klatki fizyki w sekundach.</param>
	public override void _PhysicsProcess(double delta)
	{
		//if (Input.IsActionJustPressed("pause"))
		//{
		//	GetTree().Paused = !GetTree().Paused;
		//}

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
			{
				float dist = GlobalPosition.DistanceTo(enemy.GlobalPosition);
				string name = enemy.Stats != null ? enemy.Stats.MobID : enemy.Name;
				lines.AppendLine($"{name}: {dist:F0}px");
			}
		}
		if (_debugLabel != null)
			_debugLabel.Text = lines.ToString();

		QueueRedraw();
	}
}
