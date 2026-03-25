using Godot;

public partial class Projectile : Area2D
{
    protected Vector2 Direction;
    protected WeaponStats Stats;
    protected int PierceLeft;
    protected int RuntimeDamage;
    protected float RuntimeSpeed;
    protected string SourceWeapon = "Projectile";

    /// <summary>
    /// Setup z możliwością przekazania przeliczonych wartości dmg i speed.
    /// </summary>
    public void Setup(Vector2 dir, WeaponStats stats, int damage = -1, float speed = -1, string weaponName = "Projectile")
    {
        Direction = dir;
        Stats = stats;
        PierceLeft = stats.Pierce;
        RuntimeDamage = damage < 0 ? stats.Damage : damage;
        RuntimeSpeed = speed < 0 ? stats.Speed : speed;
        SourceWeapon = weaponName;
    }

    public override void _Ready()
    {
        BodyEntered += OnHit;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += Direction * RuntimeSpeed * (float)delta;
    }

    protected virtual void OnHit(Node body)
    {
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(RuntimeDamage, Direction * Stats.Knockback, SourceWeapon);
            PierceLeft--;
            if (PierceLeft <= 0)
                QueueFree();
        }
    }
}