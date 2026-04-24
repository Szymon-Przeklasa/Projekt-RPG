using Godot;

/// <summary>
/// Klasa reprezentująca broń typu Fire Wand (Różdżka Ognia).
/// Strzela jednym lub wieloma pociskami w kierunku najbliższego wroga.
/// Obsługuje wielokrotne pociski z rozrzutem kątowym i przesunięciem spawnu.
/// Dziedziczy po klasie <see cref="Weapon"/>.
/// </summary>
public partial class FireWand : Weapon
{
	/// <summary>
	/// Scena pocisku (<see cref="Projectile"/>) wystrzeliwanego przez różdżkę.
	/// Musi być przypisana w inspektorze Godot.
	/// </summary>
	[Export] public PackedScene ProjectileScene;

	/// <summary>
	/// Metoda wywoływana przy każdym strzale.
	/// Pobiera najbliższego wroga w zasięgu, oblicza kierunek strzału,
	/// a następnie tworzy <see cref="WeaponStats.ProjectileCount"/> pocisków
	/// z symetrycznym rozrzutem kątowym wokół celu.
	/// Każdy pocisk jest inicjowany metodą <see cref="Projectile.Setup"/> i dodawany do sceny.
	/// </summary>
	protected override void Fire()
	{
		if (ProjectileScene == null) return;

		var enemy = Player.GetClosestEnemy(GetRange());
		if (enemy == null) return;

		int projectileCount = Mathf.Max(1, Stats.ProjectileCount);
		Vector2 dir = (GetAimPosition(enemy) - Player.ShootPoint.GlobalPosition).Normalized();

		for (int i = 0; i < projectileCount; i++)
		{
			var p = ProjectileScene.Instantiate<Projectile>();
			float angleOffset = GetCenteredOffset(i, projectileCount, Mathf.Max(4f, Stats.SpreadAngle));
			Vector2 spread = dir.Rotated(Mathf.DegToRad(angleOffset));
			Vector2 spawnOffset = spread.Orthogonal() * GetCenteredOffset(i, projectileCount, 8f);
			p.GlobalPosition = Player.ShootPoint.GlobalPosition + spawnOffset;

			p.Setup(spread, Stats, GetDamage(), GetSpeed(), WeaponName);
			GetTree().CurrentScene.AddChild(p);
		}
	}
}
