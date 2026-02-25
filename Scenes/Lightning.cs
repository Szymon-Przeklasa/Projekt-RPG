using Godot;
using System.Collections.Generic;

public partial class Lightning : Weapon
{
	[Export] PackedScene ProjectileScene;

	protected override void Fire()
	{
		var enemies = GetTree().GetNodesInGroup("enemies");

		if (enemies.Count == 0)
			return;

		int chainsLeft = Stats.ProjectileCount;
		Node2D current = Player.GetClosestEnemy(Stats.Range);

		if (current == null)
			return;

		var hitEnemies = new HashSet<Node2D>();

		Vector2 fromPosition = Player.ShootPoint.GlobalPosition;

		while (current != null && chainsLeft-- > 0)
		{
			if (hitEnemies.Contains(current))
				break;

			hitEnemies.Add(current);

			var center = current.GetNode<Marker2D>("Center");
			Vector2 toPosition = center.GlobalPosition;

			// Damage
			((Enemy)current).TakeDamage(Stats.Damage, Vector2.Zero);

			// Visual FX
			SpawnLightningFX(fromPosition, toPosition);

			fromPosition = toPosition;

			current = GetClosestUnhitEnemy(toPosition, hitEnemies);
		}
	}


	Node2D GetClosestUnhitEnemy(Vector2 fromPos, HashSet<Node2D> hitEnemies)
	{
		Node2D closest = null;
		float closestDist = float.MaxValue;

		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Node2D enemy && !hitEnemies.Contains(enemy))
			{
				var center = enemy.GetNode<Marker2D>("Center");
				float dist = fromPos.DistanceTo(center.GlobalPosition);

				if (dist < closestDist && dist <= Stats.Range)
				{
					closestDist = dist;
					closest = enemy;
				}
			}
		}

		return closest;
	}

	void SpawnLightningFX(Vector2 from, Vector2 to)
	{
		var beam = ProjectileScene.Instantiate<LightningBeam>();
		GetTree().CurrentScene.AddChild(beam);
		beam.Setup(from, to);
	}
}
