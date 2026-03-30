using Godot;

/// <summary>
/// Klasa reprezentująca efekt cząsteczkowy przy trafieniu wroga.
/// Dziedziczy po GpuParticles2D i automatycznie włącza emisję po dodaniu do sceny.
/// </summary>
public partial class Enemybleed : GpuParticles2D
{
    /// <summary>
    /// Metoda wywoływana po dodaniu węzła do drzewa sceny.
    /// Włącza emisję cząsteczek, aby efekt był widoczny od razu.
    /// </summary>
    public override void _Ready()
    {
        Emitting = true;
    }
}