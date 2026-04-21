using Godot;

public partial class Axe : Weapon
{
    [Export] public PackedScene ProjectileScene;

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
