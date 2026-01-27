using Godot;
using System;

public partial class Projectile : Area2D
{
	[Export] public float Speed = 800f;
	[Export] public int Damage = 10;
	[Export] public float KnockbackForce = 300f;

	public Vector2 Direction;

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += Direction * Speed * (float)delta;
	}

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node body)
	{
		if (body is Enemy enemy)
		{
			enemy.TakeDamage(Damage, Direction * KnockbackForce);
			QueueFree();
		}
	}
}
