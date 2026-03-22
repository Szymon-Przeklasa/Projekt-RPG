using Godot;

/// <summary>
/// Klasa reprezentująca broń typu FireWand.
/// Wystrzeliwuje pociski w kierunku najbliższego przeciwnika z określonym rozrzutem i ilością projektów.
/// Dziedziczy po klasie Weapon.
/// </summary>
public partial class FireWand : Weapon
{
	/// <summary>
	/// Scena pocisku, która będzie tworzona przy strzale.
	/// </summary>
	[Export] PackedScene ProjectileScene;

	/// <summary>
	/// Metoda wywoływana przy strzale.
	/// Tworzy pociski w kierunku najbliższego przeciwnika z uwzględnieniem rozrzutu i liczby pocisków.
	/// </summary>
	protected override void Fire()
	{
		var enemy = Player.GetClosestEnemy(Stats.Range);
		if (enemy == null) return;

		Vector2 dir = (enemy.GlobalPosition - Player.GlobalPosition).Normalized();

		for (int i = 0; i < Stats.ProjectileCount; i++)
		{
			var p = ProjectileScene.Instantiate<Projectile>();
			p.GlobalPosition = Player.ShootPoint.GlobalPosition;

			Vector2 spread = dir.Rotated(
				Mathf.DegToRad((float)GD.RandRange(-Stats.SpreadAngle, Stats.SpreadAngle))
			);

			p.Setup(spread, Stats);
			GetTree().CurrentScene.AddChild(p);
		}
	}
}
