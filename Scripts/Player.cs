using Godot;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

public partial class Player : CharacterBody2D
{
	[Export] public PackedScene ProjectileScene;
	[Export] public WeaponStats Weapon;
	[Export] public int Speed = 600;
	[Export] public int Xp = 0;
	[Export] public int XpToLevel = 10;
	[Export] public int Level = 1;

	public List<UpgradeData> AvailableUpgrades = new();



	public Marker2D ShootPoint;


	public override void _Ready()
	{
		SetupUpgrades();

		ShootPoint = GetNode<Marker2D>("ShootPoint");

		foreach (Weapon weapon in GetNode("Weapons").GetChildren())
			weapon.Init(this);
	}


	private void SetupUpgrades()
	{
		var lightning = GetNodeOrNull<Lightning>("Lightning");
		var garlic = GetNodeOrNull<Garlic>("Garlic");

		if (lightning != null)
		{
			AvailableUpgrades.Add(new UpgradeData(
				"Lightning Damage +5",
				() => {
					lightning.Stats.Damage += 5;
				}
			));

			AvailableUpgrades.Add(new UpgradeData(
				"Lightning Cooldown -0.5",
				() => {
					lightning.Stats.Cooldown = Mathf.Max(0.2f, lightning.Stats.Cooldown - 0.5f);
					lightning.RefreshStats();
					GD.Print("upgraded");
				}
			));

			AvailableUpgrades.Add(new UpgradeData(
				"Lightning +1 Projectile",
				() => {
					lightning.Stats.ProjectileCount += 1;
				}
			));
		}

		if (garlic != null)
		{
			AvailableUpgrades.Add(new UpgradeData(
				"Garlic Damage +2",
				() => {
					garlic.Stats.Damage += 2;
				}
			));

			AvailableUpgrades.Add(new UpgradeData(
				"Garlic Range +20",
				() => {
					garlic.Stats.Range += 20;
				}
			));
		}

		// Global stat upgrade example
		AvailableUpgrades.Add(new UpgradeData(
			"Global Knockback +50",
			() => {
				foreach (var weapon in GetChildren())
				{
					if (weapon is Weapon w)
						w.Stats.Knockback += 50;
				}
			}
		));
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
				if (d < best)
				{
					best = d;
					closest = enemy;
				}
			}
		}
		return closest;
	}

	public void GainXp(int amount)
	{
		Xp += amount;

		if (Xp >= XpToLevel)
			LevelUp();
	}

	private void LevelUp()
	{
		Level++;
		Xp -= XpToLevel;
		XpToLevel = Mathf.RoundToInt(XpToLevel * 1.4f);

		GD.Print($"LEVEL UP! Level: {Level}");

		var ui = GetTree().CurrentScene.GetNode<LevelUpUI>("LevelUpUI");
		GD.Print(ui.GetType());
		ui.ShowUpgrades(this);
	}

	public void GetInput()
	{
		Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");
		Velocity = inputDirection * Speed;
	}

	public override void _PhysicsProcess(double delta)
	{
		GetInput();
		MoveAndSlide();
	}
}
