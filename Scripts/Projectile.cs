using Godot;

public partial class Projectile : Area2D
{
	protected Vector2 Direction;
	protected WeaponStats Stats;
	protected int PierceLeft;

	public void Setup(Vector2 dir, WeaponStats stats)
	{
		Direction = dir;
		Stats = stats;
		PierceLeft = stats.Pierce;
	}

	public override void _Ready()
	{
		BodyEntered += OnHit;
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += Direction * Stats.Speed * (float)delta;
	}

	protected virtual void OnHit(Node body)
	{
		if (body is Enemy enemy)
		{
			enemy.TakeDamage(Stats.Damage, Direction * Stats.Knockback);
			PierceLeft--;

			if (PierceLeft <= 0)
				QueueFree();
		}
	}
}
