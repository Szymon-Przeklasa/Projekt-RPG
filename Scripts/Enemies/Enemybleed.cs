using Godot;

/// <summary>
/// Prosty efekt cząsteczkowy dla przeciwnika (np. przy otrzymaniu obrażeń).
/// </summary>
public partial class Enemybleed : GpuParticles2D
{
    /// <summary>
    /// Metoda wywoływana po dodaniu węzła do drzewa sceny.
    /// Włącza emisję cząsteczek.
    /// </summary>
    public override void _Ready()
    {
        Emitting = true;
    }
}