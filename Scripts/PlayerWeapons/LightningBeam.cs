using Godot;

/// <summary>
/// Klasa reprezentująca wizualny efekt pioruna (Beam) dla broni Lightning.
/// Tworzy dynamiczną linię z lekkim rozgałęzieniem i animacją migotania.
/// </summary>
public partial class LightningBeam : Node2D
{
	/// <summary>
	/// Referencja do węzła Line2D używanego do rysowania efektu pioruna.
	/// </summary>
	private Line2D line;

	/// <summary>
	/// Metoda wywoływana po dodaniu węzła do drzewa sceny.
	/// Inicjalizuje referencję do Line2D.
	/// </summary>
	public override void _Ready()
	{
		line = GetNode<Line2D>("Line2D");
	}

	/// <summary>
	/// Konfiguruje efekt pioruna między dwoma punktami.
	/// Tworzy segmenty z lekkim rozrzutem dla efektu wizualnego.
	/// </summary>
	/// <param name="from">Pozycja startowa pioruna.</param>
	/// <param name="to">Pozycja końcowa pioruna.</param>
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
	/// Po zakończeniu animacji węzeł usuwa się z drzewa sceny.
	/// </summary>
	private async void Animate()
	{
		// Mały efekt migotania
		for (int i = 0; i < 3; i++)
		{
			line.Width = (float)GD.RandRange(5f, 9f);
			await ToSignal(GetTree().CreateTimer(0.03f), "timeout");
		}

		QueueFree();
	}
}
