using Godot;

/// <summary>
/// Pocisk MagicMissile — naprowadza się na cel.
/// </summary>
public partial class MagicMissileProjectile : Projectile
{
    public Node2D Target;
    private float lifetime = 4f;

    public override void _PhysicsProcess(double delta)
    {
        lifetime -= (float)delta;
        if (lifetime <= 0) { QueueFree(); return; }

        if (Target != null && IsInstanceValid(Target))
        {
            Vector2 toTarget = (Target.GlobalPosition - GlobalPosition).Normalized();
            Direction = Direction.Lerp(toTarget, 3f * (float)delta).Normalized();
        }

        GlobalPosition += Direction * RuntimeSpeed * (float)delta;
    }
}