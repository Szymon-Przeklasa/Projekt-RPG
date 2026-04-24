using Godot;

/// <summary>
/// Rank 0 = normal, 1 = Elite (blue outline, 2× HP/XP, 1.2× size),
/// 2 = Legendary (gold outline, 4× HP/XP, 1.5× size).
/// </summary>
public enum EliteRank { Normal = 0, Elite = 1, Legendary = 2 }

/// <summary>
/// Wrogowie NIE kolidują fizycznie z graczem (żeby nie blokowali ruchu).
/// Obrażenia kontaktowe działają przez sprawdzenie odległości.
/// </summary>
public partial class Enemy : CharacterBody2D
{
	[Export] public EnemyStats Stats;
	[Export] public PackedScene XpOrbScene;
	[Export] public PackedScene HitParticle;

	protected int _health;
	private Player _player;
	private float _contactCooldown;

	private Font _font = GD.Load<FontFile>("res://Textures/Jersey15-Regular.ttf");
	private static readonly Shader _outlineShader = GD.Load<Shader>("res://Shaders/outline.gdshader");

	[Export] public float Speed = 140f;
	[Export] public int MaxHealth = 100;
	[Export] public int XpDrop = 1;
	[Export] public string MobId = "enemy";

	/// <summary>Set before _Ready() by EnemySpawner to apply elite bonuses.</summary>
	public EliteRank Rank = EliteRank.Normal;

	// Keep the old bool export so existing scenes stay compatible
	[Export] public bool IsElite
	{
		get => Rank >= EliteRank.Elite;
		set { if (value && Rank == EliteRank.Normal) Rank = EliteRank.Elite; }
	}

	public override void _Ready()
	{
		if (Stats != null)
		{
			Speed     = Stats.Speed;
			MaxHealth = Stats.MaxHealth;
			XpDrop    = Stats.XpDrop;
			MobId     = Stats.MobID;
			Scale     = new Vector2(Stats.Scale, Stats.Scale);
		}

		ApplyRank();
		_health = MaxHealth;
	}

	// ── Rank application ──────────────────────────────────────

	private void ApplyRank()
	{
		if (Rank == EliteRank.Normal) return;

		if (Rank == EliteRank.Elite)
		{
			MaxHealth = (int)(MaxHealth * 2f);
			XpDrop    = (int)(XpDrop    * 2.5f);
			Scale    *= 1.2f;
			ApplyOutline(new Color(0.2f, 0.4f, 1f, 1f), 2f);   // blue
		}
		else if (Rank == EliteRank.Legendary)
		{
			MaxHealth = (int)(MaxHealth * 4f);
			XpDrop    = (int)(XpDrop    * 5f);
			Scale    *= 1.5f;
			ApplyOutline(new Color(1f, 0.8f, 0.1f, 1f), 2.5f); // gold
		}
	}

	private void ApplyOutline(Color color, float width, float progress = 1.0f)
	{
		var sprite = FindSprite(this);
		if (sprite == null) return;

		var existing = sprite.GetNodeOrNull<Sprite2D>("Outline");
		if (existing != null) existing.QueueFree();

		var outline = new Sprite2D();
		outline.Name = "Outline";
		outline.Texture = sprite.Texture;
		outline.Hframes = sprite.Hframes;
		outline.Vframes = sprite.Vframes;
		outline.Frame = sprite.Frame;
		outline.FlipH = sprite.FlipH;
		outline.FlipV = sprite.FlipV;
		outline.Centered = sprite.Centered;
		outline.Offset = sprite.Offset; 
		outline.ZIndex = sprite.ZIndex - 1;
		outline.Position = Vector2.Zero;  

		var texSize = sprite.Texture.GetSize();
		float scaleX = (texSize.X + width * 2) / texSize.X;
		float scaleY = (texSize.Y + width * 2) / texSize.Y;
		outline.Scale = new Vector2(scaleX, scaleY);

		var mat = new ShaderMaterial();
		mat.Shader = _outlineShader;
		mat.SetShaderParameter("outline_color", color);
		outline.Material = mat;

		sprite.AddChild(outline);
	}

	private static Sprite2D FindSprite(Node node)
	{
		if (node is Sprite2D s) return s;
		foreach (Node child in node.GetChildren())
		{
			var found = FindSprite(child);
			if (found != null) return found;
		}
		return null;
	}

	// ── Damage / death ────────────────────────────────────────

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

		// Spawn label on scene root so it survives QueueFree on one-shot kills
		SpawnDamageLabel(damage, GlobalPosition);

		if (_health <= 0)
			Die();
	}

	private void SpawnDamageLabel(int damage, Vector2 worldPos)
	{
		var scene = GetTree().CurrentScene;
		if (scene == null) return;

		var label = new Label
		{
			Text   = $"{damage}",
			ZIndex = 20,
		};

		label.AddThemeColorOverride("font_color", Colors.Yellow);
		label.AddThemeFontSizeOverride("font_size", 14);
		label.AddThemeFontOverride("font", _font);
		label.Position = worldPos + new Vector2(-15f, -50f);

		scene.AddChild(label);

		var tween = label.CreateTween();
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

	// ── Physics ───────────────────────────────────────────────

	public override void _PhysicsProcess(double delta)
	{
		if (_player == null)
			_player = GetTree().GetFirstNodeInGroup("player") as Player;

		if (_player != null)
		{
			Vector2 dir = (_player.GlobalPosition - GlobalPosition).Normalized();

			// Slow enemy when overlapping player — punishes contact instead of letting it stick
			float dist        = GlobalPosition.DistanceTo(_player.GlobalPosition);
			float contactRange = 28f * (Stats?.Scale ?? 1f);
			float speedFactor = dist < contactRange ? 0.25f : 1f;

			Velocity = dir * Speed * speedFactor;
		}

		// Absorb knockback above normal speed
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

		float contactRange = 20f * (Stats?.Scale ?? 1f);
		float dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
		if (dist > contactRange) return;

		int dmg = Stats != null ? Stats.ContactDamage : 1;
		_player.TakeDamage(dmg);
		_contactCooldown = 0.25f;
	}
}
