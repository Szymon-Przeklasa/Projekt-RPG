using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public PackedScene ProjectileScene;
	[Export] public float ShootRange = 600f;
	[Export] public int Speed = 600;

	private Marker2D _shootPoint;

	public override void _Ready()
	{
		_shootPoint = GetNode<Marker2D>("ShootPoint");
		GetNode<Timer>("AutoShootTimer").Timeout += ShootAtClosestEnemy;
	}

	private void ShootAtClosestEnemy()
	{
		Node2D closestEnemy = GetClosestEnemy();
		if (closestEnemy == null)
			return;

		Vector2 direction =
			(closestEnemy.GlobalPosition - _shootPoint.GlobalPosition).Normalized();

		var projectile = ProjectileScene.Instantiate<Projectile>();
		projectile.GlobalPosition = _shootPoint.GlobalPosition;
		projectile.Direction = direction;

		GetTree().CurrentScene.AddChild(projectile);
	}

	private Node2D GetClosestEnemy()
	{
		Node2D closest = null;
		float closestDist = ShootRange;

		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Node2D enemy)
			{
				float dist = GlobalPosition.DistanceTo(enemy.GlobalPosition);
				if (dist < closestDist)
				{
					closestDist = dist;
					closest = enemy;
				}
			}
		}

		return closest;
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
