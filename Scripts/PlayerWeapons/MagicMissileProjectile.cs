using Godot;

/// <summary>
/// Specjalny typ pocisku Magic Missile.
/// Pocisk automatycznie naprowadza się na wyznaczony cel (Target)
/// oraz posiada ograniczony czas życia.
/// </summary>
public partial class MagicMissileProjectile : Projectile
{
    /// <summary>
    /// Aktualny cel pocisku.
    /// Jeśli istnieje, pocisk będzie próbował się do niego naprowadzać.
    /// </summary>
    public Node2D Target;

    public float TurnRate = 5.5f;
    public float ReacquireRange = 180f;

    /// <summary>
    /// Maksymalny czas życia pocisku (w sekundach).
    /// Po jego upływie pocisk znika.
    /// </summary>
    private float lifetime = 4f;

    /// <summary>
    /// Aktualizacja fizyki pocisku.
    /// Odpowiada za:
    /// - skracanie czasu życia,
    /// - naprowadzanie na cel,
    /// - ruch pocisku.
    /// </summary>
    public override void _PhysicsProcess(double delta)
    {
        // Zmniejszanie czasu życia
        lifetime -= (float)delta;
        if (lifetime <= 0)
        {
            QueueFree();
            return;
        }

        // Naprowadzanie na cel (jeśli istnieje i jest poprawny)
        if (Target == null || !IsInstanceValid(Target))
        {
            Target = FindClosestEnemy();
        }

        if (Target != null && IsInstanceValid(Target))
        {
            Vector2 toTarget = (Target.GlobalPosition - GlobalPosition).Normalized();

            Direction = Direction
                .Lerp(toTarget, TurnRate * (float)delta)
                .Normalized();
        }

        Rotation = Direction.Angle();
        Advance(Direction * RuntimeSpeed * (float)delta);
    }

    private Node2D FindClosestEnemy()
    {
        Node2D closest = null;
        float bestDist = ReacquireRange;

        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is not Node2D enemy)
                continue;

            float dist = GlobalPosition.DistanceTo(enemy.GlobalPosition);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = enemy;
            }
        }

        return closest;
    }
}
