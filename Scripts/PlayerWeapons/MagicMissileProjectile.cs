using Godot;

/// <summary>
/// Klasa reprezentująca samonaprowadzający się pocisk broni Magic Missile.
/// Pocisk automatycznie śledzi przypisany cel (<see cref="Target"/>),
/// płynnie korygując kierunek lotu za pomocą interpolacji.
/// Jeśli cel zostanie utracony (zniszczony lub poza zasięgiem),
/// pocisk szuka nowego najbliższego wroga.
/// Pocisk usuwa się po upływie <c>lifetime</c> sekund.
/// Dziedziczy po klasie <see cref="Projectile"/>.
/// </summary>
public partial class MagicMissileProjectile : Projectile
{
    /// <summary>
    /// Aktualny cel pocisku.
    /// Jeśli istnieje i jest prawidłowy, pocisk będzie się na niego naprowadzał.
    /// Może być <c>null</c> — wtedy pocisk szuka nowego celu metodą <see cref="FindClosestEnemy"/>.
    /// </summary>
    public Node2D Target;

    /// <summary>
    /// Prędkość obrotu pocisku w kierunku celu (radiany na sekundę, skalowane przez delta).
    /// Wyższa wartość daje ostrzejsze zakręty, niższa — łagodniejsze łuki.
    /// </summary>
    public float TurnRate = 5.5f;

    /// <summary>
    /// Maksymalny zasięg poszukiwania nowego celu po utracie oryginalnego (w jednostkach świata).
    /// </summary>
    public float ReacquireRange = 180f;

    /// <summary>
    /// Pozostały czas życia pocisku w sekundach.
    /// Po jego upływie pocisk wywołuje <see cref="Node.QueueFree"/>.
    /// </summary>
    private float lifetime = 4f;

    /// <summary>
    /// Aktualizacja fizyki pocisku wywoływana każdą klatką.
    /// Odpowiada za:
    /// <list type="bullet">
    ///   <item><description>Skracanie pozostałego czasu życia; usunięcie pocisku po jego upływie.</description></item>
    ///   <item><description>Ponowne namierzenie celu, jeśli oryginalny nie jest już prawidłowy.</description></item>
    ///   <item><description>Płynne naprowadzanie — kierunek jest interpolowany w stronę celu z prędkością <see cref="TurnRate"/>.</description></item>
    ///   <item><description>Obrót sprite'a zgodnie z aktualnym kierunkiem lotu.</description></item>
    ///   <item><description>Przemieszczenie pocisku metodą <see cref="Projectile.Advance"/>.</description></item>
    /// </list>
    /// </summary>
    /// <param name="delta">Czas od poprzedniej klatki fizyki (sekundy).</param>
    public override void _PhysicsProcess(double delta)
    {
        lifetime -= (float)delta;
        if (lifetime <= 0)
        {
            QueueFree();
            return;
        }

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

    /// <summary>
    /// Wyszukuje najbliższego wroga w zasięgu <see cref="ReacquireRange"/>.
    /// Przeszukuje wszystkich wrogów z grupy "enemies".
    /// </summary>
    /// <returns>Najbliższy <see cref="Node2D"/> wroga lub <c>null</c>, jeśli brak celów w zasięgu.</returns>
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