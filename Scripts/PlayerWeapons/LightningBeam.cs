using Godot;

/// <summary>
/// Klasa odpowiedzialna za wizualny efekt pioruna (beam).
/// Tworzy dynamiczną, "poszarpaną" linię między dwoma punktami
/// oraz krótką animację migotania.
/// </summary>
public partial class LightningBeam : Node2D
{
	/// <summary>
	/// Referencja do komponentu Line2D używanego do rysowania pioruna.
	/// </summary>
	private Line2D line;

	/// <summary>
	/// Inicjalizacja węzła po dodaniu do sceny.
	/// Pobiera referencję do Line2D.
	/// </summary>
	public override void _Ready()
	{
		line = GetNode<Line2D>("Line2D");
	}

	/// <summary>
	/// Konfiguruje efekt pioruna pomiędzy dwoma punktami.
	/// Generuje segmenty z losowym odchyleniem, aby uzyskać efekt elektrycznego łuku.
	/// </summary>
	/// <param name="from">Punkt początkowy (źródło pioruna).</param>
	/// <param name="to">Punkt końcowy (cel pioruna).</param>
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
	/// Krótka animacja migotania pioruna.
	/// Zmienia grubość linii w krótkich odstępach czasu,
	/// po czym usuwa efekt ze sceny.
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