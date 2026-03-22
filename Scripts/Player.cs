using Godot;
using System;
using System.Collections.Generic;
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
	/// Aktualna ilość doświadczenia gracza.
	/// </summary>
	[Export] public int Xp = 0;

	/// <summary>
	/// Ilość doświadczenia potrzebna do awansu na kolejny poziom.
	/// </summary>
	[Export] public int XpToLevel = 10;

	/// <summary>
	/// Aktualny poziom gracza.
	/// </summary>
	[Export] public int Level = 1;

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

	/// <summary>
	/// Metoda wywoływana po załadowaniu sceny.
	/// Inicjalizuje bronie i dostępne ulepszenia.
	/// </summary>
	public override void _Ready()
	{
		SetupUpgrades();
		GD.Print("Upgrades count: ", AvailableUpgrades.Count);

		ShootPoint = GetNode<Marker2D>("ShootPoint");

		foreach (Weapon weapon in GetNode("Weapons").GetChildren())
		{
			weapon.Init(this);
			Weapons.Add(weapon);
		}
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

	/// <summary>
	/// Inicjalizuje dostępne ulepszenia gracza.
	/// Dodaje ulepszenia dla konkretnych broni oraz globalne statystyki.
	/// </summary>
	private void SetupUpgrades()
	{
		var lightning = GetNodeOrNull<Lightning>("Weapons/Lightning");
		if (lightning != null)
		{
			AvailableUpgrades.Add(new UpgradeData(
				"Lightning Damage +5",
				UpgradeType.Weapon,
				(p) => { lightning.Stats.Damage += 5; },
				8
			));

			AvailableUpgrades.Add(new UpgradeData(
				"Lightning Cooldown -0.1",
				UpgradeType.Weapon,
				(p) => {
					lightning.Stats.Cooldown = Mathf.Max(0.2f, lightning.Stats.Cooldown - 0.1f);
					lightning.RefreshStats();
				},
				8
			));

			AvailableUpgrades.Add(new UpgradeData(
				"Lightning +1 Projectile",
				UpgradeType.Weapon,
				(p) => { lightning.Stats.ProjectileCount += 1; },
				8
			));
		}

		var garlic = GetNodeOrNull<Garlic>("Weapons/Garlic");
		if (garlic != null)
		{
			AvailableUpgrades.Add(new UpgradeData(
				"Garlic Damage +2",
				UpgradeType.Weapon,
				(p) => { garlic.Stats.Damage += 2; },
				8
			));

			AvailableUpgrades.Add(new UpgradeData(
				"Garlic Range +20",
				UpgradeType.Weapon,
				(p) => { garlic.Stats.Range += 20; },
				8
			));
		}

		// global stat upgrade
		AvailableUpgrades.Add(new UpgradeData(
			"Damage +10%",
			UpgradeType.Stat,
			(p) => { p.DamageMultiplier += 0.1f; },
			5
		));
	}

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

	/// <summary>
	/// Dodaje doświadczenie graczowi i sprawdza, czy nastąpił awans na kolejny poziom.
	/// </summary>
	/// <param name="amount">Ilość doświadczenia do dodania.</param>
	public void GainXp(int amount)
	{
		Xp += amount;

		if (Xp >= XpToLevel)
			LevelUp();
	}

	/// <summary>
	/// Zwiększa poziom gracza i wywołuje interfejs wyboru ulepszeń.
	/// </summary>
	private void LevelUp()
	{
		Level++;
		Xp -= XpToLevel;
		XpToLevel = Mathf.RoundToInt(XpToLevel * 1.4f);

		GD.Print($"LEVEL UP! Level: {Level}");

		var ui = GetTree().CurrentScene.GetNode<LevelUpUI>("LevelUpUI");
		var upgrades = GetUpgradeChoices(3);
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
		if (Input.IsActionJustPressed("pause"))
		{
			GetTree().Paused = !GetTree().Paused;
		}

		GetInput();
		MoveAndSlide();
	}
}
