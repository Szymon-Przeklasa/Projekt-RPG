using Godot;
using System;

/// <summary>
/// Klasa reprezentująca przeciwnika w grze.
/// Obsługuje ruch w kierunku gracza, otrzymywanie obrażeń, śmierć i drop doświadczenia.
/// </summary>
public partial class Enemy : CharacterBody2D
{
	// -- konfiguracja -------------------------------------------------------------
	[Export] public EnemyStats Stats;
    [Export] public PackedScene XpOrbScene;
    [Export] public PackedScene HitParticle;

	// -- stan ---------------------------------------------------------------------
	private int _health;
	private Player _player;
	private float _contactCooldown;

	// MobID / Speed / MaxHealth / XpDrop czytamy ze Stats, ale zostawiamy dla zgodności inicjalne wartości
    [Export] public float Speed = 140f;
	[Export] public int MaxHealth = 100;
	[Export] public int XpDrop = 1;
	[Export] public string MobID = "enemy";

	public override void _Ready()
	{
		if (Stats != null)
		{
			Speed = Stats.Speed;
			MaxHealth = Stats.MaxHealth;
			XpDrop = Stats.XpDrop;
			MobID = Stats.MobID;
			Scale = new Vector2(Stats.Scale, Stats.Scale);
		}
		_health = MaxHealth;
	}

	// -----------------------------------------------------------------
	public void TakeDamage(int damage, Vector2 knockback)
	{
		_health -= damage;
		Velocity += knockback;

		if (HitParticle != null)
		{
			var fx = HitParticle.Instantiate<Enemybleed>();
			AddChild(fx);
			fx.Position = Vector2.Zero;
			fx.Emitting = true;
		} 

		if (_health <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
        GetNode<KillManager>("/root/KillManager").RegisterKill(MobID);
        DropXp();
        QueueFree();
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
		if (_player == null)
			_player = GetTree().GetFirstNodeInGroup("player") as Player;

		if (_player != null)
		{
			Vector2 dir = (_player.GlobalPosition - GlobalPosition).Normalized();
			Velocity += dir * Speed * (float)delta;
		}

		Velocity = Velocity.Lerp(Vector2.Zero, 0.05f);
		MoveAndSlide();


		HandleContactDamage(delta);
	}
	

	private void HandleContactDamage(double delta)
	{
		_contactCooldown -= (float)delta;
		if (_contactCooldown > 0) return;

		if (_player == null) return;
		float dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
		if (dist > 40f) return;

		int dmg = Stats != null ? Stats.ContactDamage : 1;
		//_player.TakeDamage(dmg);
		_contactCooldown = 0.5f;
    }
}
