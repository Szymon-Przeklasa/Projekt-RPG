using Godot;

/// <summary>
/// Klasa reprezentująca pojedynczy segment wizualnego łańcucha błyskawicy.
/// Odpowiada za wyświetlenie efektu pomiędzy dwoma punktami,
/// na przykład pomiędzy graczem a przeciwnikiem lub pomiędzy kolejnymi celami.
///
/// Efekt:
/// <list type="bullet">
/// <item><description>ustawia pozycję początkową segmentu,</description></item>
/// <item><description>obraca efekt w kierunku celu,</description></item>
/// <item><description>skaluje długość efektu do dystansu między punktami,</description></item>
/// <item><description>automatycznie usuwa się po zakończeniu emisji.</description></item>
/// </list>
///
/// Klasa dziedziczy po <see cref="GpuParticles2D"/>.
/// </summary>
public partial class LightningChain : GpuParticles2D
{
    /// <summary>
    /// Konfiguruje segment błyskawicy pomiędzy dwoma punktami.
    /// Ustawia pozycję, rotację oraz skalę efektu cząsteczkowego
    /// tak, aby wizualnie połączyć oba punkty.
    /// </summary>
    /// <param name="from">Punkt początkowy segmentu.</param>
    /// <param name="to">Punkt końcowy segmentu.</param>
    public void Setup(Vector2 from, Vector2 to)
    {
        // Ustaw pozycję początkową
        GlobalPosition = from;

        // Oblicz kierunek i rotację
        var direction = (to - from).Normalized();
        Rotation = direction.Angle();

        // Długość segmentu (wpływa na skalę efektu)
        float distance = from.DistanceTo(to);

        // Skalowanie w osi X (rozciągnięcie efektu)
        Scale = new Vector2(distance / 100f, 1f);

        // Restart emisji cząsteczek
        Restart();
        Emitting = true;
    }

    /// <summary>
    /// Metoda wywoływana po dodaniu efektu do sceny.
    /// Uruchamia emisję cząsteczek, odczekuje czas życia efektu,
    /// a następnie usuwa obiekt ze sceny.
    /// </summary>
    public override async void _Ready()
    {
        Emitting = true;

        // Poczekaj czas równy Lifetime
        await ToSignal(GetTree().CreateTimer(Lifetime), "timeout");

        // Usuń efekt ze sceny
        QueueFree();
    }
}