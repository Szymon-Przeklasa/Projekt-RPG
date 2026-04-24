using Godot;

/// <summary>
/// Klasa reprezentująca broń typu Axe (Topór).
/// Wystrzeliwuje pociski (<see cref="AxeProjectile"/>) z łukową trajektorią w kierunku wrogów.
/// Obsługuje wielokrotne pociski z kątowym rozrzutem i przesunięciem bocznym przy spawnie.
/// Dziedziczy po klasie <see cref="Weapon"/>.
/// </summary>
public partial class Axe : Weapon
{
	/// <summary>
	/// Scena pocisku (<see cref="AxeProjectile"/>) wystrzeliwanego przez topór.
	/// Musi być przypisana w inspektorze Godot.
	/// </summary>
	[Export] public PackedScene ProjectileScene;

	/// <summary>
	/// Metoda wywoływana przy każdym strzale.
	/// Pobiera listę najbliższych wrogów, a następnie dla każdego pocisku
	/// wyznacza cel, oblicza kierunek i rozrzut kątowy, a następnie tworzy
	/// instancję <see cref="AxeProjectile"/> z odpowiednim przesunięciem spawnu.
	/// Każdy pocisk jest inicjowany metodą <see cref="Projectile.Setup"/> i dodawany do sceny.
	/// </summary>
	protected override void Fire()
	{
		if (ProjectileScene == null) return;

		int projectileCount = Mathf.Max(1, Stats.ProjectileCount);
		var targets = GetClosestEnemies(GetRange(), Mathf.Max(1, projectileCount), Player.ShootPoint.GlobalPosition);
		if (targets.Count == 0) return;

		for (int i = 0; i < projectileCount; i++)
		{
			var target = targets[i % targets.Count];
			Vector2 dir = (GetAimPosition(target) - Player.ShootPoint.GlobalPosition).Normalized();
			var p = ProjectileScene.Instantiate<AxeProjectile>();
			float angleOffset = GetCenteredOffset(i, projectileCount, Mathf.Max(10f, Stats.SpreadAngle));
			Vector2 spread = dir.Rotated(Mathf.DegToRad(angleOffset));
			Vector2 spawnOffset = spread.Orthogonal() * GetCenteredOffset(i, projectileCount, 12f);
			p.GlobalPosition = Player.ShootPoint.GlobalPosition + spawnOffset;

			p.Setup(spread, Stats, GetDamage(), GetSpeed(), WeaponName);
			GetTree().CurrentScene.AddChild(p);
		}
	}
}
