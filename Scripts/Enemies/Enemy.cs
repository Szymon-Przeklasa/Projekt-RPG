using Godot;
using System;

/// <summary>
/// Wrogowie NIE kolidują fizycznie z graczem (żeby nie blokowali ruchu).
/// Obrażenia kontaktowe działają przez sprawdzenie odległości.
/// Wrogowie NIE kolidują też ze sobą (layer 2, mask 0 dla fizyki gracza/mapy).
/// Mapa ma layer/mask 1,1 — wrogowie muszą mieć mask 1 żeby nie chodzić przez ściany.
/// </summary>
public partial class Enemy : CharacterBody2D
{
	[Export] public EnemyStats Stats;
	[Export] public PackedScene XpOrbScene;
	[Export] public PackedScene HitParticle;

	protected int _health;
	private Player _player;
	private float _contactCooldown;
	
	private Font font = GD.Load<FontFile>("res://Textures/Jersey15-Regular.ttf");

	[Export] public float Speed = 140f;
	[Export] public int MaxHealth = 100;
	[Export] public int XpDrop = 1;
	[Export] public string MobId = "enemy";
	[Export] public bool IsElite = false;

	public override void _Ready()
	{
		if (Stats != null)
		{
			Speed = Stats.Speed;
			MaxHealth = Stats.MaxHealth;
			XpDrop = Stats.XpDrop;
			MobId = Stats.MobID;
			Scale = new Vector2(Stats.Scale, Stats.Scale);
		}
		_health = MaxHealth;
	}

	public void TakeDamage(int damage, Vector2 knockback, string weaponName = "unknown")
	{
		_health -= damage;
		Velocity += knockback;

		SoundManager.Instance?.PlayHit();

		if (HitParticle != null)
		{
			var fx = HitParticle.Instantiate<Enemybleed>();
			AddChild(fx);
			fx.Position = Vector2.Zero;
			fx.Emitting = true;
		}

		SpawnDamageLabel(damage);

		if (_health <= 0)
			Die();
	}

	private void SpawnDamageLabel(int damage)
	{
		var label = new Label
		{
			Text = $"{damage}",
			ZIndex = 10,
			Position = new Vector2(-15f, -50f),
		};

		label.AddThemeColorOverride("font_color", Colors.Yellow);
		label.AddThemeFontSizeOverride("font_size", 14);
		label.AddThemeFontOverride("font", font);
		AddChild(label);

		var tween = CreateTween();
		tween.TweenProperty(label, "position", label.Position + new Vector2(0, -35f), 0.7f);
		tween.Parallel().TweenProperty(label, "modulate:a", 0f, 0.7f);
		tween.TweenCallback(Callable.From(() => label.QueueFree()));
	}

	private void Die()
	{
		GetNode<KillManager>("/root/KillManager").RegisterKill(MobId);
		DropXp();
		QueueFree();
	}

	private void DropXp()
	{
		if (XpOrbScene == null) return;
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

			// Slow enemy when overlapping player — punishes contact instead of letting it stick
			float dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
			float contactRange = 28f * (Stats?.Scale ?? 1f);
			float speedFactor = dist < contactRange ? 0.25f : 1f;

			Velocity = dir * Speed * speedFactor;
		}

		// Amortyzuj knockback powyżej normalnej prędkości
		if (Velocity.Length() > Speed * 2f)
			Velocity = Velocity.Lerp(Velocity.Normalized() * Speed, 0.2f);

		MoveAndSlide();
		HandleContactDamage(delta);
	}

	private void HandleContactDamage(double delta)
	{
		_contactCooldown -= (float)delta;
		if (_contactCooldown > 0) return;
		if (_player == null) return;

		float contactRange = 28f * (Stats?.Scale ?? 1f);
		float dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
		if (dist > contactRange) return;

		int dmg = Stats != null ? Stats.ContactDamage : 1;
		_player.TakeDamage(dmg);
		_contactCooldown = 0.25f;
	}
}
