using Godot;

/// <summary>
/// Klasa reprezentująca pocisk topora (AxeProjectile).
/// Dziedziczy po klasie Projectile i dodaje efekt opadającej trajektorii, symulując spadający topór.
/// </summary>
public partial class AxeProjectile : Projectile
{
    /// <summary>
    /// Czas od momentu wystrzelenia pocisku.
    /// Używany do modyfikacji trajektorii w osi Y, aby pocisk "opadał" w czasie lotu.
    /// </summary>
    private float time;

    /// <summary>
    /// Metoda fizyczna wywoływana co klatkę (_PhysicsProcess).
    /// Aktualizuje czas, modyfikuje kierunek pocisku w osi Y, a następnie wykonuje standardową logikę przesuwania pocisku z klasy bazowej.
    /// </summary>
    /// <param name="delta">Czas (w sekundach) od ostatniej klatki.</param>
    public override void _PhysicsProcess(double delta)
    {
        // Zwiększamy czas lotu pocisku
        time += (float)delta;

        // Modyfikujemy kierunek w osi Y, aby pocisk opadał w czasie
        Direction.Y -= time * 1.3f;

        // Wywołanie logiki ruchu pocisku z klasy bazowej
        base._PhysicsProcess(delta);
    }
}