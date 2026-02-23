using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed = 140f;
	[Export] public int MaxHealth = 100;
	[Export] public PackedScene XpOrbScene;
	[Export] public int XpDrop = 1;
	[Export] public PackedScene HitParticle;
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

		if (HitParticle != null)
		{
			var fx = HitParticle.Instantiate<Enemybleed>();
			AddChild(fx);               // attach to enemy
			fx.Position = Vector2.Zero; // local position
			fx.Emitting = true;
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

		GetTree().CurrentScene.CallDeferred(Node.MethodName.AddChild, orb);
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
