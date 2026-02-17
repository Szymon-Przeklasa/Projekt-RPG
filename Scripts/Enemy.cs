using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed = 140f;
	[Export] public int MaxHealth = 100;
	[Export] public PackedScene XpOrbScene;
	[Export] public int XpDrop = 1;
	[Export] public PackedScene DeathParticle;
	private int _health;

	Player player;

	public override void _Ready()
	{
		_health = MaxHealth;
	}

	public void TakeDamage(int damage, Vector2 knockback)
	{
		_health -= damage;
		Velocity += knockback;

		if (DeathParticle != null)
		{
			var fx = DeathParticle.Instantiate<Enemybleed>();
			fx.Setup(GlobalPosition);
			GetTree().CurrentScene.AddChild(fx);
		}

		if (_health <= 0)
		{
			// death
			DropXp();
			QueueFree();
		}
	}

	private void DropXp()
	{
		if (XpOrbScene == null)
			return;

		var orb = XpOrbScene.Instantiate<XpOrb>();
		orb.GlobalPosition = GlobalPosition;
		orb.Value = XpDrop;

		GetTree().CurrentScene.AddChild(orb);
	}


	public override void _PhysicsProcess(double delta)
	{
		if (player == null)
			player = GetTree().GetFirstNodeInGroup("player") as Player;

		if (player != null)
		{
			Vector2 dir =
				(player.GlobalPosition - GlobalPosition).Normalized();
			Velocity += dir * Speed * (float)delta;
		}

		Velocity = Velocity.Lerp(Vector2.Zero, 0.05f);
		MoveAndSlide();
	}
}
