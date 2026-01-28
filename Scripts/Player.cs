using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
	[Export] public PackedScene ProjectileScene;
	[Export] public WeaponStats Weapon;
	[Export] public int Speed = 600;
	[Export] public int Xp = 0;
	[Export] public int XpToLevel = 10;
	[Export] public int Level = 1;


	public Marker2D ShootPoint;

	public override void _Ready()
	{
		ShootPoint = GetNode<Marker2D>("ShootPoint");

		foreach (Weapon weapon in GetNode("Weapons").GetChildren())
			weapon.Init(this);
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
