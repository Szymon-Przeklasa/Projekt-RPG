using Godot;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene EnemyScene;
	[Export] public float SpawnRadius = 100f;
	[Export] public float SpawnInterval = 1.2f;

	private Player player;
	private Timer timer;

	public override void _Ready()
	{
		player = GetTree().GetFirstNodeInGroup("player") as Player;

		timer = GetNode<Timer>("SpawnTimer");
		timer.WaitTime = SpawnInterval;
		timer.Timeout += SpawnEnemy;
		timer.Start();
	}

	private void SpawnEnemy()
	{
		if (player == null) return;

		Vector2 direction =
			Vector2.Right.Rotated(GD.Randf() * Mathf.Tau);

		Vector2 spawnPos =
			player.GlobalPosition + direction * SpawnRadius;

		var enemy = EnemyScene.Instantiate<Enemy>();
		enemy.GlobalPosition = spawnPos;

		GetTree().CurrentScene.AddChild(enemy);
	}
}
