using Godot;
using System.Collections.Generic;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene EnemyScene;
	[Export] public float SpawnRadius = 100f;
	[Export] public float SpawnInterval = 1.2f;
	[Export] public float MinSpawnInterval = 0.05f;
	[Export] public float SpawnDecayFactor = 0.95f;

	/// <summary>
	/// Referencja do TileMapLayer określającej obszar spawnu.
	/// </summary>
	[Export] public TileMapLayer SpawnMap;

	private Player player;
	private Timer timer;
	private List<Vector2> validSpawnPositions = new();

	public override void _Ready()
	{
		player = GetTree().GetFirstNodeInGroup("player") as Player;
		timer = GetNode<Timer>("SpawnTimer");
		timer.WaitTime = SpawnInterval;
		timer.Timeout += SpawnEnemy;
		timer.Start();

		BuildSpawnPositions();
	}

	/// <summary>
	/// Zbiera globalne pozycje środków wszystkich kafelków z TileMapLayer.
	/// Wywołaj ponownie jeśli mapa zmienia się dynamicznie.
	/// </summary>
	private void BuildSpawnPositions()
	{
		validSpawnPositions.Clear();

		if (SpawnMap == null)
		{
			GD.PushWarning("EnemySpawner: SpawnMap not assigned!");
			return;
		}

		foreach (Vector2I cell in SpawnMap.GetUsedCells())
		{
			// Środek kafelka w lokalnych współrzędnych mapy, przeliczony na globalne
			Vector2 localPos = SpawnMap.MapToLocal(cell);
			Vector2 globalPos = SpawnMap.ToGlobal(localPos);
			validSpawnPositions.Add(globalPos);
		}

		GD.Print($"EnemySpawner: Znaleziono {validSpawnPositions.Count} pozycji spawnu.");
	}

	private void SpawnEnemy()
	{
		if (player == null || validSpawnPositions.Count == 0) return;

		Vector2 spawnPos = GetSpawnPositionNearPlayer();

		var enemy = EnemyScene.Instantiate<Enemy>();
		enemy.GlobalPosition = spawnPos;
		GetTree().CurrentScene.AddChild(enemy);

		timer.WaitTime = Mathf.Max(MinSpawnInterval, timer.WaitTime * SpawnDecayFactor);
		timer.Start();
	}

	/// <summary>
	/// Szuka kafelka w pobliżu gracza (w promieniu SpawnRadius).
	/// Jeśli żaden nie pasuje, zwraca losowy kafelek z całej mapy.
	/// </summary>
	private Vector2 GetSpawnPositionNearPlayer()
	{
		// Zbierz kafelki w promieniu SpawnRadius od gracza
		var nearby = new List<Vector2>();
		float radiusSq = SpawnRadius * SpawnRadius;

		foreach (Vector2 pos in validSpawnPositions)
		{
			float minDistSq = 600f * 600f; // nie za blisko gracza
			if (player.GlobalPosition.DistanceSquaredTo(pos) >= minDistSq &&
				player.GlobalPosition.DistanceSquaredTo(pos) <= radiusSq)
				nearby.Add(pos);
		}

		List<Vector2> pool = nearby.Count > 0 ? nearby : validSpawnPositions;
		return pool[(int)(GD.Randi() % (uint)pool.Count)];
	}
}
