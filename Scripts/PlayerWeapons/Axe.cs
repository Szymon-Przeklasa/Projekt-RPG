using Godot;

public partial class Axe : Weapon
{
    [Export] public PackedScene ProjectileScene;

    protected override void Fire()
    {
        var enemy = Player.GetClosestEnemy(GetRange());
        if (enemy == null) return;
        if (ProjectileScene == null) return;

        Vector2 dir = (enemy.GlobalPosition - Player.GlobalPosition).Normalized();

        for (int i = 0; i < Stats.ProjectileCount; i++)
        {
            var p = ProjectileScene.Instantiate<AxeProjectile>();
            p.GlobalPosition = Player.ShootPoint.GlobalPosition;

            Vector2 spread = dir.Rotated(
                Mathf.DegToRad((float)GD.RandRange(-Stats.SpreadAngle, Stats.SpreadAngle))
            );

            p.Setup(spread, Stats, GetDamage(), GetSpeed(), WeaponName);
            GetTree().CurrentScene.AddChild(p);
        }
    }
}

public partial class AxeProjectile : Projectile
{
    private float time;

    public override void _PhysicsProcess(double delta)
    {
        time += (float)delta;
        Direction.Y -= time * 1.3f;
        base._PhysicsProcess(delta);
    }
}