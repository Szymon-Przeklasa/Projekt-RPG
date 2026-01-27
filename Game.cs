using Godot;
using System;

public partial class Game : Node2D
{
	[Export] public PackedScene PlayerScene;
	[Export] public PackedScene EnemyScene;

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
	}
}
