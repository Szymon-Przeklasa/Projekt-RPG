using Godot;

public partial class FireWand : Weapon
{
	[Export] PackedScene ProjectileScene;

	protected override void Fire()
	{
		var enemy = Player.GetClosestEnemy(GetRange());
		if (enemy == null) return;

		Vector2 dir = (enemy.GlobalPosition - Player.GlobalPosition).Normalized();

		for (int i = 0; i < Stats.ProjectileCount; i++)
		{
			var p = ProjectileScene.Instantiate<Projectile>();
			p.GlobalPosition = Player.ShootPoint.GlobalPosition;

			Vector2 spread = dir.Rotated(
				Mathf.DegToRad((float)GD.RandRange(-Stats.SpreadAngle, Stats.SpreadAngle))
			);

            // Przekazujemy zmodyfikowane statystyki przez WeaponStatsRuntime
            p.Setup(spread, Stats, GetDamage(), GetSpeed(), WeaponName);
            GetTree().CurrentScene.AddChild(p);
		}
	}
}
