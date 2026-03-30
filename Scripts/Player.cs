using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Klasa reprezentująca gracza w grze.
/// Odpowiada za sterowanie ruchem, zarządzanie broniami, ulepszeniami,
/// doświadczeniem (XP) oraz interakcją z przeciwnikami.
/// </summary>
public partial class Player : CharacterBody2D
{
	/// <summary>
	/// Scena pocisku wykorzystywana przez bronie gracza.
	/// </summary>
	[Export] public PackedScene ProjectileScene;

	/// <summary>
	/// Bazowe statystyki aktualnej broni (jeśli używane globalnie).
	/// </summary>
	[Export] public WeaponStats Weapon;

	/// <summary>
	/// Bazowa prędkość poruszania się gracza.
	/// </summary>
	[Export] public int Speed = 600;

	/// <summary>
	/// Lista wszystkich dostępnych ulepszeń (losowanych przy level upie).
	/// </summary>
	public List<UpgradeData> AvailableUpgrades = new();

	/// <summary>
	/// Globalny mnożnik obrażeń (wpływa na wszystkie bronie).
	/// </summary>
	public float DamageMultiplier = 1f;

	/// <summary>
	/// Globalny mnożnik czasu odnowienia (im mniejszy, tym szybciej strzelamy).
	/// </summary>
	public float CooldownMultiplier = 1f;

	/// <summary>
	/// Globalny mnożnik zasięgu efektów (np. aura Garlic).
	/// </summary>
	public float AreaMultiplier = 1f;

	/// <summary>
	/// Globalny mnożnik prędkości ruchu gracza.
	/// </summary>
	public float SpeedMultiplier = 1f;

	/// <summary>
	/// Globalny mnożnik prędkości pocisków.
	/// </summary>
	public float ProjectileSpeedMultiplier = 1f;

	/// <summary>
	/// Maksymalna liczba posiadanych broni.
	/// </summary>
	public const int MAX_WEAPONS = 5;

	/// <summary>
	/// Maksymalna liczba pasywnych ulepszeń.
	/// </summary>
	public const int MAX_PASSIVES = 5;

	/// <summary>
	/// Lista aktualnie posiadanych broni.
	/// </summary>
	public List<Weapon> Weapons = new();

	/// <summary>
	/// Lista aktywnych pasywnych ulepszeń.
	/// </summary>
	public List<PassiveData> Passives = new();

	/// <summary>
	/// Punkt, z którego wystrzeliwane są pociski.
	/// </summary>
	public Marker2D ShootPoint;

	/// <summary>
	/// Pasek doświadczenia (UI).
	/// </summary>
	private ProgressBar xpBar;

	/// <summary>
	/// Czy rysować debugowe linie do przeciwników.
	/// </summary>
	[Export] public bool DebugDrawEnemyLines = true;

	private Label _debugLabel;

	/// <summary>
	/// Metoda rysująca debugowe linie do przeciwników.
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
	/// Inicjalizacja gracza po załadowaniu sceny.
	/// </summary>
	public override void _Ready()
	{
		SetupUpgrades();

		ShootPoint = GetNode<Marker2D>("ShootPoint");
		xpBar = GetTree().CurrentScene.GetNode<ProgressBar>("CanvasLayer/XPBar");

		foreach (Weapon weapon in GetNode("Weapons").GetChildren())
		{
			weapon.Init(this);
			Weapons.Add(weapon);
		}

		_debugLabel = new Label();
		_debugLabel.ZIndex = 20;
		_debugLabel.Position = new Vector2(20, -80);
		_debugLabel.AddThemeColorOverride("font_color", Colors.Cyan);
		AddChild(_debugLabel);

		UpdateXpBar();
	}

	/// <summary>
	/// Dodaje nową broń do gracza.
	/// </summary>
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
	/// Dodaje lub ulepsza pasywkę.
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
	/// Zwraca losowe dostępne ulepszenia.
	/// </summary>
	public List<UpgradeData> GetUpgradeChoices(int count = 3)
	{
		List<UpgradeData> valid = new();

		foreach (var upgrade in AvailableUpgrades)
			if (upgrade.CanUpgrade)
				valid.Add(upgrade);

		Shuffle(valid);

		if (valid.Count > count)
			valid.RemoveRange(count, valid.Count - count);

		return valid;
	}

	/// <summary>
	/// Tasuje listę elementów (Fisher-Yates).
	/// </summary>
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
	/// Odświeża wszystkie bronie po zmianie statystyk.
	/// </summary>
	public void RefreshAllWeapons()
	{
		foreach (var weapon in Weapons)
			weapon.RefreshStats();
	}

	/// <summary>
	/// Tworzy listę dostępnych ulepszeń (pasywne + bronie).
	/// </summary>
	private void SetupUpgrades()
	{
		// (tu logika tworzenia PassiveData i UpgradeData)
	}

	/// <summary>
	/// Zwraca najbliższego przeciwnika w danym zasięgu.
	/// </summary>
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
	/// Aktualny poziom gracza.
	/// </summary>
	public int Level = 1;

	/// <summary>
	/// Aktualna ilość doświadczenia.
	/// </summary>
	public int Xp = 0;

	/// <summary>
	/// Ilość XP potrzebna do kolejnego poziomu.
	/// </summary>
	public int XpToLevel => Level * 80;

	/// <summary>
	/// Dodaje doświadczenie i obsługuje level up.
	/// </summary>
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

	/// <summary>
	/// Aktualizuje pasek XP w UI.
	/// </summary>
	private void UpdateXpBar()
	{
		if (xpBar == null) return;

		xpBar.MaxValue = XpToLevel;
		xpBar.Value = Xp;
	}

	/// <summary>
	/// Obsługuje awans na wyższy poziom.
	/// </summary>
	private void LevelUp()
	{
		Level++;

		var ui = GetTree().CurrentScene.GetNodeOrNull<LevelUpUI>("LevelUpUI");
		ui?.ShowUpgrades(this);
	}

	/// <summary>
	/// Odczytuje input i ustawia prędkość gracza.
	/// </summary>
	public void GetInput()
	{
		Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");
		Velocity = inputDirection * Speed * SpeedMultiplier;
	}

	/// <summary>
	/// Obsługa fizyki (ruch gracza).
	/// </summary>
	public override void _PhysicsProcess(double delta)
	{
		GetInput();
		MoveAndSlide();
	}

	/// <summary>
	/// Debug: wyświetla odległości do przeciwników.
	/// </summary>
	public override void _Process(double delta)
	{
		if (!DebugDrawEnemyLines) return;

		var lines = new System.Text.StringBuilder();

		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Enemy enemy)
			{
				float dist = GlobalPosition.DistanceTo(enemy.GlobalPosition);
				lines.AppendLine($"{enemy.Name}: {dist:F0}px");
			}
		}

		if (_debugLabel != null)
			_debugLabel.Text = lines.ToString();

		QueueRedraw();
	}
}