using Godot;

/// <summary>
/// Klasa odpowiedzialna za wizualny efekt błyskawicy.
/// Generuje dynamiczną, nieregularną linię pomiędzy dwoma punktami,
/// imitującą elektryczny łuk.
/// 
/// Efekt:
/// <list type="bullet">
/// <item><description>tworzy poszarpaną linię między źródłem a celem,</description></item>
/// <item><description>losowo zmienia grubość linii dla efektu migotania,</description></item>
/// <item><description>automatycznie usuwa się po zakończeniu animacji.</description></item>
/// </list>
/// 
/// Klasa dziedziczy po <see cref="Node2D"/>.
/// </summary>
public partial class LightningBeam : Node2D
{
    /// <summary>
    /// Referencja do komponentu <see cref="Line2D"/> używanego
    /// do renderowania wizualnej linii błyskawicy.
    /// </summary>
    private Line2D line;

    /// <summary>
    /// Metoda wywoływana po dodaniu obiektu do sceny.
    /// Pobiera referencję do komponentu <see cref="Line2D"/>.
    /// </summary>
    public override void _Ready()
    {
        line = GetNode<Line2D>("Line2D");
    }

    /// <summary>
    /// Konfiguruje efekt błyskawicy pomiędzy dwoma punktami.
    /// Generuje serię segmentów z losowym odchyleniem,
    /// aby uzyskać naturalny wygląd elektrycznego wyładowania.
    /// </summary>
    /// <param name="from">Punkt początkowy efektu.</param>
    /// <param name="to">Punkt końcowy efektu.</param>
    public void Setup(Vector2 from, Vector2 to)
    {
        line.ClearPoints();

        // Ustawienie pozycji bazowej
        GlobalPosition = from;

        Vector2 dir = to - from;
        Vector2 normal = dir.Normalized().Orthogonal();

        int segments = 6;

        // Tworzenie punktów linii
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector2 point = dir * t;

            // Dodanie losowego odchylenia (oprócz początku i końca)
            if (i != 0 && i != segments)
            {
                point += normal * (float)GD.RandRange(-12f, 12f);
            }

            line.AddPoint(point);
        }

        Animate();
    }

    /// <summary>
    /// Odtwarza krótką animację migotania błyskawicy.
    /// W kilku krokach losowo zmienia szerokość linii,
    /// a następnie usuwa efekt ze sceny.
    /// </summary>
    private async void Animate()
    {
        for (int i = 0; i < 3; i++)
        {
            line.Width = (float)GD.RandRange(2f, 5f);
            await ToSignal(GetTree().CreateTimer(0.03f), "timeout");
        }

        // Usunięcie efektu po animacji
        QueueFree();
    }
}