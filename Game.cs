using Godot;
using System;

public partial class Game : Node2D
{
	[Export] public PackedScene PlayerScene;
	[Export] public PackedScene EnemyScene;
	[Export] public int EnemyCount = 50;
	[Export] public Rect2 SpawnArea = new Rect2(-500, -500, 1000, 1000);

	private Marker2D _playerSpawn;
	private Node2D _enemyContainer;

	public override void _Ready()
	{
		_playerSpawn = GetNode<Marker2D>("PlayerSpawn");
		_enemyContainer = GetNode<Node2D>("Enemies");

		SpawnPlayer();
		SpawnEnemies();
	}

	private void SpawnPlayer()
	{
		var player = PlayerScene.Instantiate<Player>();
		player.GlobalPosition = _playerSpawn.GlobalPosition;
		AddChild(player);
	}

	private void SpawnEnemies()
	{
		foreach (Marker2D spawn in GetNode("EnemySpawns").GetChildren())
		{
			var enemy = EnemyScene.Instantiate<Enemy>();
			enemy.GlobalPosition = spawn.GlobalPosition;
			_enemyContainer.AddChild(enemy);
		}
		for (int i = 0; i < EnemyCount; i++)
		{
			var enemy = EnemyScene.Instantiate<Enemy>();

			enemy.GlobalPosition = new Vector2(
				(float)GD.RandRange(SpawnArea.Position.X, SpawnArea.End.X),
				(float)GD.RandRange(SpawnArea.Position.Y, SpawnArea.End.Y)
			);

			_enemyContainer.AddChild(enemy);
		}
	}
}
