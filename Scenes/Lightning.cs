using Godot;

public partial class Lightning : Weapon
{
	protected override void Fire()
	{
		Node2D current = Player.GetClosestEnemy(Stats.Range);
		int chains = Stats.Pierce;

		while (current != null && chains-- > 0)
		{
			((Enemy)current).TakeDamage(Stats.Damage, Vector2.Zero);
			current = GetNext(current);
		}
	}

	Node2D GetNext(Node2D from)
	{
		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Node2D enemy &&
				enemy != from &&
				from.GlobalPosition.DistanceTo(enemy.GlobalPosition) < 200)
				return enemy;
		}
		return null;
	}
}
