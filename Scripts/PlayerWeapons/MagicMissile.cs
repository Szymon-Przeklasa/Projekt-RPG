using Godot;

public partial class MagicMissile : Weapon
{
    [Export] public PackedScene ProjectileScene;

    protected override void Fire()
    {
        for (int i = 0; i < Stats.ProjectileCount; i++)
        {
            var enemy = Player.GetClosestEnemy(GetRange());
            if (enemy == null) return;

            if (ProjectileScene == null) return;
            var p = ProjectileScene.Instantiate<MagicMissileProjectile>();
            p.GlobalPosition = Player.ShootPoint.GlobalPosition;
            p.Setup(
                (enemy.GlobalPosition - Player.GlobalPosition).Normalized(),
                Stats,
                GetDamage(),
                GetSpeed()
            );
            p.Target = enemy;
            GetTree().CurrentScene.AddChild(p);
        }
    }
}