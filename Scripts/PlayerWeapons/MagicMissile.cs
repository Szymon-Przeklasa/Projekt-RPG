using Godot;

public partial class MagicMissile : Weapon
{
    [Export] public PackedScene ProjectileScene;

    protected override void Fire()
    {
        if (ProjectileScene == null) return;

        int projectileCount = Mathf.Max(1, Stats.ProjectileCount);
        var targets = GetClosestEnemies(GetRange() * 1.15f, Mathf.Max(1, projectileCount), Player.ShootPoint.GlobalPosition);
        if (targets.Count == 0) return;

        for (int i = 0; i < projectileCount; i++)
        {
            var enemy = targets[i % targets.Count];
            Vector2 toTarget = (GetAimPosition(enemy) - Player.ShootPoint.GlobalPosition).Normalized();
            float angleOffset = GetCenteredOffset(i, projectileCount, 7f);
            Vector2 launchDirection = toTarget.Rotated(Mathf.DegToRad(angleOffset));
            Vector2 spawnOffset = launchDirection.Orthogonal() * GetCenteredOffset(i, projectileCount, 10f);
            var p = ProjectileScene.Instantiate<MagicMissileProjectile>();
            p.GlobalPosition = Player.ShootPoint.GlobalPosition + spawnOffset;
            p.Setup(
                launchDirection,
                Stats,
                GetDamage(),
                GetSpeed(),
                WeaponName
            );
            p.Target = enemy;
            GetTree().CurrentScene.AddChild(p);
        }
    }
}
