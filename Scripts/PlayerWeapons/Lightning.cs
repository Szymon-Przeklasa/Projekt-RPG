using Godot;
using System.Collections.Generic;

public partial class Lightning : Weapon
{
	[Export] public PackedScene ProjectileScene;

	protected override void Fire()
	{
		var enemies = GetTree().GetNodesInGroup("enemies");
		if (enemies.Count == 0) return;

		float range = GetRange();
		int chainsLeft = Stats.ProjectileCount;
		Node2D current = Player.GetClosestEnemy(range);
		if (current == null) return;

		var hitEnemies = new HashSet<Node2D>();
		Vector2 fromPosition = Player.ShootPoint.GlobalPosition;

		while (current != null && chainsLeft-- > 0)
		{
			if (hitEnemies.Contains(current)) break;

			hitEnemies.Add(current);
			Vector2 toPosition = GetAimPosition(current);

			((Enemy)current).TakeDamage(GetDamage(), Vector2.Zero, WeaponName);

			SpawnLightningFX(fromPosition, toPosition);
			fromPosition = toPosition;
			current = GetClosestUnhitEnemy(toPosition, hitEnemies, range);
		}
	}

	Node2D GetClosestUnhitEnemy(Vector2 fromPos, HashSet<Node2D> hitEnemies, float range)
	{
		Node2D closest = null;
		float closestDist = float.MaxValue;

		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Node2D enemy && !hitEnemies.Contains(enemy))
			{
				float dist = fromPos.DistanceTo(GetAimPosition(enemy));
				if (dist < closestDist && dist <= range)
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
		if (ProjectileScene == null) return;
		var beam = ProjectileScene.Instantiate<LightningBeam>();
		GetTree().CurrentScene.AddChild(beam);
		beam.Setup(from, to);
	}
}
