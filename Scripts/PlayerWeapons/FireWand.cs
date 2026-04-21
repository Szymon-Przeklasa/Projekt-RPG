using Godot;

public partial class FireWand : Weapon
{
	[Export] public PackedScene ProjectileScene;

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
