using Godot;

/// <summary>
/// Prosty efekt cz¹steczkowy dla przeciwnika (np. przy otrzymaniu obra¿eñ).
/// </summary>
public partial class Enemybleed : GpuParticles2D
{
    /// <summary>
    /// Metoda wywo³ywana po dodaniu wêz³a do drzewa sceny.
    /// W³¹cza emisjê cz¹steczek.
    /// </summary>
    public override void _Ready()
    {
        Emitting = true;
    }
}