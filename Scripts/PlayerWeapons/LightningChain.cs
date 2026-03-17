using Godot;

/// <summary>
/// Klasa reprezentuj¹ca pojedynczy segment ³añcucha pioruna dla broni Lightning.
/// Dziedziczy po GpuParticles2D i odpowiada za wizualizacjê miêdzy dwoma punktami.
/// </summary>
public partial class LightningChain : GpuParticles2D
{
    /// <summary>
    /// Konfiguruje segment ³añcucha miêdzy dwoma punktami.
    /// Ustawia pozycjê, obrót, skalê i uruchamia emisjê cz¹steczek.
    /// </summary>
    /// <param name="from">Pozycja startowa segmentu.</param>
    /// <param name="to">Pozycja koñcowa segmentu.</param>
    public void Setup(Vector2 from, Vector2 to)
    {
        GlobalPosition = from;

        var direction = (to - from).Normalized();
        Rotation = direction.Angle();

        float distance = from.DistanceTo(to);

        Scale = new Vector2(distance / 100f, 1f);

        Restart();
        Emitting = true;
    }

    /// <summary>
    /// Metoda wywo³ywana po dodaniu wêz³a do drzewa sceny.
    /// W³¹cza emisjê cz¹steczek i po okreœlonym czasie usuwa segment z gry.
    /// </summary>
    public override async void _Ready()
    {
        Emitting = true;
        await ToSignal(GetTree().CreateTimer(Lifetime), "timeout");
        QueueFree();
    }
}