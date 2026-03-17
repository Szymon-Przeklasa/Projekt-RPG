using Godot;

/// <summary>
/// Klasa reprezentuj¹ca pocisk topora.
/// Dziedziczy po klasie Projectile i dodaje efekt opadaj¹cej trajektorii.
/// </summary>
public partial class AxeProjectile : Projectile
{
    /// <summary>
    /// Czas od momentu wystrzelenia pocisku.
    /// Wykorzystywany do modyfikacji trajektorii w osi Y.
    /// </summary>
    private float time;

    /// <summary>
    /// Metoda fizyczna wywo³ywana co klatkê.
    /// Aktualizuje czas i modyfikuje kierunek pocisku, a nastêpnie wywo³uje logikê klasy bazowej.
    /// </summary>
    /// <param name="delta">Czas od ostatniej klatki.</param>
    public override void _PhysicsProcess(double delta)
    {
        time += (float)delta;
        Direction.Y -= time * 1.3f;
        base._PhysicsProcess(delta);
    }
}