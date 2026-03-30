using Godot;

/// <summary>
/// Klasa reprezentująca pojedynczy segment wizualny łańcucha pioruna.
/// Odpowiada za efekt graficzny między dwoma punktami (np. między wrogami).
/// Dziedziczy po GpuParticles2D.
/// </summary>
public partial class LightningChain : GpuParticles2D
{
	/// <summary>
	/// Konfiguruje segment pioruna między dwoma punktami w świecie gry.
	/// Ustawia pozycję, rotację oraz skalę efektu cząsteczkowego.
	/// </summary>
	/// <param name="from">Punkt początkowy (np. źródło ataku).</param>
	/// <param name="to">Punkt końcowy (np. trafiony przeciwnik).</param>
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
	/// Wywoływana po dodaniu węzła do sceny.
	/// Automatycznie uruchamia emisję i usuwa efekt po czasie życia (Lifetime).
	/// </summary>
	public override async void _Ready()
	{
		Emitting = true;

		// Poczekaj czas równy Lifetime (z GpuParticles2D)
		await ToSignal(GetTree().CreateTimer(Lifetime), "timeout");

		// Usuń efekt ze sceny
		QueueFree();
	}
}