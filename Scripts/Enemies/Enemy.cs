using Godot;

/// <summary>
/// Ranga elitarności przeciwnika.
/// Wpływa na HP, XP drop, rozmiar i efekt wizualny obrysu.
/// </summary>
public enum EliteRank
{
	/// <summary>Standardowy przeciwnik bez modyfikatorów.</summary>
	Normal = 0,

	/// <summary>Elitarny przeciwnik: 2× HP, 2.5× XP, 1.2× rozmiar, niebieski obrys.</summary>
	Elite = 1,

	/// <summary>Legendarny przeciwnik: 4× HP, 5× XP, 1.5× rozmiar, złoty obrys.</summary>
	Legendary = 2
}

/// <summary>
/// Klasa bazowa wszystkich przeciwników w grze.
/// Zarządza ruchem w kierunku gracza, zadawaniem obrażeń kontaktowych,
/// otrzymywaniem obrażeń, efektami wizualnymi trafień oraz śmiercią i dropem XP.
/// Przeciwnicy NIE kolidują fizycznie z graczem (nie blokują ruchu);
/// obrażenia kontaktowe są sprawdzane przez odległość w <see cref="HandleContactDamage"/>.
/// </summary>
public partial class Enemy : CharacterBody2D
{
	/// <summary>Statystyki przeciwnika ładowane z zasobu <see cref="EnemyStats"/>.</summary>
	[Export] public EnemyStats Stats;

	/// <summary>Scena orb XP (<see cref="XpOrb"/>) dropowana po śmierci.</summary>
	[Export] public PackedScene XpOrbScene;

	/// <summary>Scena efektu cząsteczkowego trafienia (<see cref="Enemybleed"/>).</summary>
	[Export] public PackedScene HitParticle;

	/// <summary>Aktualna liczba punktów życia przeciwnika.</summary>
	protected int _health;

	/// <summary>Referencja do gracza, pobierana z grupy "player".</summary>
	private Player _player;

	/// <summary>Pozostały czas cooldownu obrażeń kontaktowych (sekundy).</summary>
	private float _contactCooldown;

	/// <summary>Czcionka używana do renderowania etykiet obrażeń.</summary>
	private Font _font = GD.Load<FontFile>("res://Textures/Jersey15-Regular.ttf");

	/// <summary>Shader obrysu wczytywany raz dla wszystkich instancji.</summary>
	private static readonly Shader _outlineShader = GD.Load<Shader>("res://Shaders/outline.gdshader");

	/// <summary>Prędkość ruchu przeciwnika (jednostki/s). Nadpisywana przez <see cref="Stats"/>.</summary>
	[Export] public float Speed = 140f;

	/// <summary>Maksymalne punkty życia. Nadpisywane przez <see cref="Stats"/> i rangę.</summary>
	[Export] public int MaxHealth = 100;

	/// <summary>Liczba XP dropowana po śmierci. Skaluje się z rangą i czasem rozgrywki.</summary>
	[Export] public int XpDrop = 1;

	/// <summary>Identyfikator typu wroga zgodny z <see cref="EnemyStats.MobID"/> (np. "slime").</summary>
	[Export] public string MobId = "enemy";

	/// <summary>
	/// Ranga elitarności ustawiana przez <see cref="EnemySpawner"/> przed <see cref="_Ready"/>.
	/// Wpływa na statystyki, rozmiar i efekt wizualny obrysu.
	/// </summary>
	public EliteRank Rank = EliteRank.Normal;

	/// <summary>
	/// Kompatybilny eksport bool dla starszych scen.
	/// Odczytanie zwraca <c>true</c> jeśli ranga to Elite lub Legendary.
	/// Ustawienie na <c>true</c> promuje rangę Normal → Elite (nie wpływa na Legendary).
	/// </summary>
	[Export]
	public bool IsElite
	{
		get => Rank >= EliteRank.Elite;
		set { if (value && Rank == EliteRank.Normal) Rank = EliteRank.Elite; }
	}

	/// <summary>
	/// Inicjalizacja po dodaniu do sceny.
	/// Kopiuje statystyki z <see cref="Stats"/> (jeśli przypisane), stosuje modyfikatory rangi
	/// i ustawia bazowe HP.
	/// </summary>
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

		ApplyRank();
		_health = MaxHealth;
	}

	// ── Ranga ─────────────────────────────────────────────────

	/// <summary>
	/// Stosuje modyfikatory rangi: skaluje HP, XP, rozmiar wizualny i dodaje obrys szaderowy.
	/// Wywoływana jednorazowo z <see cref="_Ready"/>.
	/// </summary>
	private void ApplyRank()
	{
		if (Rank == EliteRank.Normal) return;

		if (Rank == EliteRank.Elite)
		{
			MaxHealth = (int)(MaxHealth * 2f);
			XpDrop = (int)(XpDrop * 2.5f);
			Scale *= 1.2f;
			ApplyOutline(new Color(0.2f, 0.4f, 1f, 1f), 2f);   // niebieski
		}
		else if (Rank == EliteRank.Legendary)
		{
			MaxHealth = (int)(MaxHealth * 4f);
			XpDrop = (int)(XpDrop * 5f);
			Scale *= 1.5f;
			ApplyOutline(new Color(1f, 0.8f, 0.1f, 1f), 2.5f); // złoty
		}
	}

	/// <summary>
	/// Dodaje obrys szaderowy do sprite'a przeciwnika jako osobny węzeł potomny.
	/// Obrys jest lekko powiększony względem oryginału i renderowany pod sprite'em (ZIndex-1).
	/// </summary>
	/// <param name="color">Kolor obrysu.</param>
	/// <param name="width">Szerokość obrysu w pikselach tekstury.</param>
	/// <param name="progress">Nieużywane — zarezerwowane na przyszłe animacje.</param>
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

	/// <summary>
	/// Rekurencyjnie przeszukuje poddrzewo węzła w poszukiwaniu pierwszego <see cref="Sprite2D"/>.
	/// </summary>
	/// <param name="node">Węzeł startowy przeszukiwania.</param>
	/// <returns>Pierwszy znaleziony <see cref="Sprite2D"/> lub <c>null</c>.</returns>
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

	// ── Obrażenia i śmierć ────────────────────────────────────

	/// <summary>
	/// Zadaje obrażenia przeciwnikowi, stosuje odrzut, odtwarza dźwięk trafienia,
	/// tworzy efekt cząsteczkowy i pływającą etykietę obrażeń.
	/// Jeśli HP spadnie do zera lub poniżej, wywołuje <see cref="Die"/>.
	/// </summary>
	/// <param name="damage">Liczba obrażeń do odjęcia od HP.</param>
	/// <param name="knockback">Wektor odrzutu dodawany do prędkości przeciwnika.</param>
	/// <param name="weaponName">Nazwa broni źródłowej (do logowania).</param>
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

		// Spawn etykiety na korzeniu sceny, aby przetrwała QueueFree przy trafieniu jednym strzałem
		SpawnDamageLabel(damage, GlobalPosition);

		if (_health <= 0)
			Die();
	}

	/// <summary>
	/// Tworzy pływającą etykietę tekstową z liczbą obrażeń w podanej pozycji świata.
	/// Etykieta animuje się w górę i zanika, a następnie usuwa się automatycznie.
	/// </summary>
	/// <param name="damage">Liczba obrażeń do wyświetlenia.</param>
	/// <param name="worldPos">Pozycja globalna, w której pojawi się etykieta.</param>
	private void SpawnDamageLabel(int damage, Vector2 worldPos)
	{
		var scene = GetTree().CurrentScene;
		if (scene == null) return;

		var label = new Label
		{
			Text = $"{damage}",
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

	/// <summary>
	/// Obsługuje śmierć przeciwnika: rejestruje zabójstwo w <see cref="KillManager"/>,
	/// dropuje orby XP i usuwa węzeł ze sceny.
	/// </summary>
	private void Die()
	{
		GetNode<KillManager>("/root/KillManager").RegisterKill(MobId);
		DropXp();
		QueueFree();
	}

	/// <summary>
	/// Tworzy instancję orba XP (<see cref="XpOrb"/>) i dodaje ją do sceny za pomocą
	/// <c>CallDeferred</c>, aby uniknąć problemów z drzewem sceny podczas usuwania węzła.
	/// </summary>
	private void DropXp()
	{
		if (XpOrbScene == null) return;
		var orb = XpOrbScene.Instantiate<XpOrb>();
		orb.GlobalPosition = GlobalPosition;
		orb.Value = XpDrop;
		GetTree().CurrentScene.CallDeferred(Node.MethodName.AddChild, orb);
	}

	// ── Fizyka ────────────────────────────────────────────────

	/// <summary>
	/// Aktualizacja fizyki wywoływana każdą klatką.
	/// Pobiera referencję do gracza (jeśli brak), oblicza kierunek ruchu,
	/// spowalnia przeciwnika gdy nachodził na gracza (zapobiega "przyklejaniu"),
	/// absorbuje nadmiarowy odrzut i wywołuje <see cref="CharacterBody2D.MoveAndSlide"/>.
	/// Na końcu sprawdza obrażenia kontaktowe z graczem.
	/// </summary>
	/// <param name="delta">Czas od poprzedniej klatki fizyki (sekundy).</param>
	public override void _PhysicsProcess(double delta)
	{
		if (_player == null)
			_player = GetTree().GetFirstNodeInGroup("player") as Player;

		if (_player != null)
		{
			Vector2 dir = (_player.GlobalPosition - GlobalPosition).Normalized();

			// Spowolnienie gdy przeciwnik nakłada się na gracza
			float dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
			float contactRange = 28f * (Stats?.Scale ?? 1f);
			float speedFactor = dist < contactRange ? 0.25f : 1f;

			Velocity = dir * Speed * speedFactor;
		}

		// Absorpcja nadmiarowego odrzutu
		if (Velocity.Length() > Speed * 2f)
			Velocity = Velocity.Lerp(Velocity.Normalized() * Speed, 0.2f);

		MoveAndSlide();
		HandleContactDamage(delta);
	}

	/// <summary>
	/// Sprawdza czy przeciwnik jest wystarczająco blisko gracza, aby zadać obrażenia kontaktowe.
	/// Obrażenia aplikowane są maksymalnie raz na 0.25 sekundy (cooldown).
	/// Zasięg kontaktu skaluje się z <see cref="EnemyStats.Scale"/> przeciwnika.
	/// </summary>
	/// <param name="delta">Czas od poprzedniej klatki fizyki (sekundy).</param>
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
