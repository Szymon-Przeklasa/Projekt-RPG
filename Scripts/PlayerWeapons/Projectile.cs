using Godot;

/// <summary>
/// Klasa reprezentuj¹ca pocisk wystrzeliwany przez broñ.
/// Obs³uguje ruch pocisku, kolizje z wrogami oraz iloœæ przebiæ (Pierce).
/// </summary>
public partial class Projectile : Area2D
{
    /// <summary>
    /// Kierunek poruszania siê pocisku.
    /// </summary>
    protected Vector2 Direction;

    /// <summary>
    /// Statystyki broni, z której pochodzi pocisk.
    /// </summary>
    protected WeaponStats Stats;

    /// <summary>
    /// Pozosta³a liczba przebiæ pocisku.
    /// </summary>
    protected int PierceLeft;

    /// <summary>
    /// Inicjalizuje pocisk z kierunkiem, statystykami broni i ustawieniem iloœci przebiæ.
    /// </summary>
    /// <param name="dir">Kierunek poruszania siê pocisku.</param>
    /// <param name="stats">Statystyki broni.</param>
    public void Setup(Vector2 dir, WeaponStats stats)
    {
        Direction = dir;
        Stats = stats;
        PierceLeft = stats.Pierce;
    }

    /// <summary>
    /// Metoda wywo³ywana po dodaniu wêz³a do drzewa sceny.
    /// Subskrybuje zdarzenie BodyEntered do obs³ugi kolizji z wrogami.
    /// </summary>
    public override void _Ready()
    {
        BodyEntered += OnHit;
    }

    /// <summary>
    /// Metoda fizyczna wywo³ywana co klatkê.
    /// Przesuwa pocisk w kierunku Direction z uwzglêdnieniem prêdkoœci Stats.Speed.
    /// </summary>
    /// <param name="delta">Czas od ostatniej klatki.</param>
    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += Direction * Stats.Speed * (float)delta;
    }

    /// <summary>
    /// Wywo³ywana po trafieniu w inny wêze³.
    /// Je¿eli wêze³ jest przeciwnikiem, zadaje obra¿enia i zmniejsza liczbê przebiæ.
    /// </summary>
    /// <param name="body">Wêze³, który wszed³ w kolizjê z pociskiem.</param>
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