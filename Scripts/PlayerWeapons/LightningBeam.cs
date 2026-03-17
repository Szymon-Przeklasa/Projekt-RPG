using Godot;

/// <summary>
/// Klasa reprezentuj¹ca wizualny efekt pioruna (Beam) dla broni Lightning.
/// Tworzy dynamiczn¹ liniê z lekkim rozga³êzieniem i animacj¹ migotania.
/// </summary>
public partial class LightningBeam : Node2D
{
    /// <summary>
    /// Referencja do wêz³a Line2D u¿ywanego do rysowania efektu pioruna.
    /// </summary>
    private Line2D line;

    /// <summary>
    /// Metoda wywo³ywana po dodaniu wêz³a do drzewa sceny.
    /// Inicjalizuje referencjê do Line2D.
    /// </summary>
    public override void _Ready()
    {
        line = GetNode<Line2D>("Line2D");
    }

    /// <summary>
    /// Konfiguruje efekt pioruna miêdzy dwoma punktami.
    /// Tworzy segmenty z lekkim rozrzutem dla efektu wizualnego.
    /// </summary>
    /// <param name="from">Pozycja startowa pioruna.</param>
    /// <param name="to">Pozycja koñcowa pioruna.</param>
    public void Setup(Vector2 from, Vector2 to)
    {
        line.ClearPoints();
        GlobalPosition = from;

        Vector2 dir = to - from;
        float length = dir.Length();
        Vector2 normal = dir.Normalized().Orthogonal();

        int segments = 6;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector2 point = dir * t;

            if (i != 0 && i != segments)
            {
                point += normal * (float)GD.RandRange(-15f, 15f);
            }

            line.AddPoint(point);
        }

        Animate();
    }

    /// <summary>
    /// Asynchroniczna animacja pioruna – migotanie linii przez krótki czas.
    /// Po zakoñczeniu animacji wêze³ usuwa siê z drzewa sceny.
    /// </summary>
    private async void Animate()
    {
        // Ma³y efekt migotania
        for (int i = 0; i < 3; i++)
        {
            line.Width = (float)GD.RandRange(5f, 9f);
            await ToSignal(GetTree().CreateTimer(0.03f), "timeout");
        }

        QueueFree();
    }
}