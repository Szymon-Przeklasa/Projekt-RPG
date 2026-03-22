using Godot;

/// <summary>
/// Klasa odpowiedzialna za generowanie przeciwników w grze.
/// Tworzy nowych wrogów wokół gracza w określonym promieniu i z czasem skraca odstęp między spawnami.
/// </summary>
public partial class EnemySpawner : Node2D
{
	/// <summary>
	/// Scena przeciwnika do tworzenia instancji.
	/// </summary>
	[Export] public PackedScene EnemyScene;

	/// <summary>
	/// Promień wokół gracza, w którym przeciwnicy mogą się pojawiać.
	/// </summary>
	[Export] public float SpawnRadius = 100f;

	/// <summary>
	/// Początkowy odstęp między spawnami przeciwników (w sekundach).
	/// </summary>
	[Export] public float SpawnInterval = 1.2f;

	/// <summary>
	/// Minimalny możliwy odstęp między spawnami.
	/// </summary>
	[Export] public float MinSpawnInterval = 0.05f;

	/// <summary>
	/// Współczynnik zmniejszający odstęp między spawnami po każdym wrogu.
	/// </summary>
	[Export] public float SpawnDecayFactor = 0.95f;

	/// <summary>
	/// Referencja do gracza, wokół którego spawnują się wrogowie.
	/// </summary>
	private Player player;

	/// <summary>
	/// Timer odpowiedzialny za wywoływanie spawnów przeciwników.
	/// </summary>
	private Timer timer;

	/// <summary>
	/// Metoda wywoływana po dodaniu węzła do drzewa sceny.
	/// Inicjalizuje gracza i timer.
	/// </summary>
	public override void _Ready()
	{
		player = GetTree().GetFirstNodeInGroup("player") as Player;

		timer = GetNode<Timer>("SpawnTimer");
		timer.WaitTime = SpawnInterval;
		timer.Timeout += SpawnEnemy;
		timer.Start();
	}

	/// <summary>
	/// Tworzy nowego przeciwnika w losowej pozycji wokół gracza.
	/// Skraca odstęp między spawnami zgodnie ze współczynnikiem SpawnDecayFactor.
	/// </summary>
	private void SpawnEnemy()
	{
		if (player == null) return;

		// Losowa pozycja w promieniu SpawnRadius
		Vector2 direction = Vector2.Right.Rotated(GD.Randf() * Mathf.Tau);
		Vector2 spawnPos = player.GlobalPosition + direction * SpawnRadius;

		// Tworzenie wroga
		var enemy = EnemyScene.Instantiate<Enemy>();
		enemy.GlobalPosition = spawnPos;
		GetTree().CurrentScene.AddChild(enemy);

		// Stopniowe zmniejszanie odstępu między spawnami
		timer.WaitTime = Mathf.Max(MinSpawnInterval, timer.WaitTime * SpawnDecayFactor);
		timer.Start(); // restart timera z nowym odstępem
	}
}
