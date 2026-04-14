using Godot;

/// <summary>
/// Klasa reprezentująca broń FireWand.
/// Wystrzeliwuje pociski w kierunku najbliższego wroga z możliwością rozproszenia (spread).
/// Dziedziczy po klasie Weapon.
/// </summary>
public partial class FireWand : Weapon
{
	/// <summary>
	/// Scena pocisku (Projectile) instancjonowana przy każdym strzale.
	/// </summary>
	[Export] PackedScene ProjectileScene;

	/// <summary>
	/// Metoda wywoływana przy strzale.
	/// Wystrzeliwuje określoną liczbę pocisków (Stats.ProjectileCount) w kierunku najbliższego wroga.
	/// Każdy pocisk może mieć losowe rozproszenie w zakresie określonym przez Stats.SpreadAngle.
	/// </summary>
	protected override void Fire()
	{
		// Szukamy najbliższego wroga w zasięgu broni
		var enemy = Player.GetClosestEnemy(GetRange());
		if (enemy == null) return;

		Vector2 dir = (enemy.GlobalPosition - Player.GlobalPosition).Normalized();

		for (int i = 0; i < Stats.ProjectileCount; i++)
		{
			// Instancjonujemy nowy pocisk
			var p = ProjectileScene.Instantiate<Projectile>();
			p.GlobalPosition = Player.ShootPoint.GlobalPosition;

			// Losowe rozproszenie (spread) pocisku
			Vector2 spread = dir.Rotated(
				Mathf.DegToRad((float)GD.RandRange(-Stats.SpreadAngle, Stats.SpreadAngle))
			);

			// Przekazujemy zmodyfikowane statystyki przez WeaponStatsRuntime
			p.Setup(spread, Stats, GetDamage(), GetSpeed(), WeaponName);

			// Dodajemy pocisk do bieżącej sceny
			GetTree().CurrentScene.AddChild(p);
		}
	}
}
