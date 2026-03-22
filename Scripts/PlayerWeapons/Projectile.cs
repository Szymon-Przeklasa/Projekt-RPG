using Godot;

/// <summary>
/// Klasa reprezentująca pocisk wystrzeliwany przez broń.
/// Obsługuje ruch pocisku, kolizje z wrogami oraz ilość przebić (Pierce).
/// </summary>
public partial class Projectile : Area2D
{
    /// <summary>
    /// Kierunek poruszania się pocisku.
    /// </summary>
    protected Vector2 Direction;

    /// <summary>
    /// Statystyki broni, z której pochodzi pocisk.
    /// </summary>
    protected WeaponStats Stats;

    /// <summary>
    /// Pozostała liczba przebić pocisku.
    /// </summary>
    protected int PierceLeft;

    /// <summary>
    /// Inicjalizuje pocisk z kierunkiem, statystykami broni i ustawieniem ilości przebić.
    /// </summary>
    /// <param name="dir">Kierunek poruszania się pocisku.</param>
    /// <param name="stats">Statystyki broni.</param>
    public void Setup(Vector2 dir, WeaponStats stats)
    {
        Direction = dir;
        Stats = stats;
        PierceLeft = stats.Pierce;
    }

    /// <summary>
    /// Metoda wywoływana po dodaniu węzła do drzewa sceny.
    /// Subskrybuje zdarzenie BodyEntered do obsługi kolizji z wrogami.
    /// </summary>
    public override void _Ready()
    {
        BodyEntered += OnHit;
    }

    /// <summary>
    /// Metoda fizyczna wywoływana co klatkę.
    /// Przesuwa pocisk w kierunku Direction z uwzględnieniem prędkości Stats.Speed.
    /// </summary>
    /// <param name="delta">Czas od ostatniej klatki.</param>
    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += Direction * Stats.Speed * (float)delta;
    }

    /// <summary>
    /// Wywoływana po trafieniu w inny węzeł.
    /// Jeżeli węzeł jest przeciwnikiem, zadaje obrażenia i zmniejsza liczbę przebić.
    /// </summary>
    /// <param name="body">Węzeł, który wszedł w kolizję z pociskiem.</param>
    protected virtual void OnHit(Node body)
    {
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(Stats.Damage, Direction * Stats.Knockback);
            PierceLeft--;

            if (PierceLeft <= 0)
                QueueFree();
        }
    }
}