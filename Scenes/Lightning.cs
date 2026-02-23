using Godot;

public partial class Lightning : Weapon
{
	[Export] PackedScene ProjectileScene;

	protected override void Fire()
	{
		Node2D current = Player.GetClosestEnemy(Stats.Range);
		int chains = Stats.ProjectileCount;

		Node2D from = Player.ShootPoint;

		while (current != null && chains-- > 0)
		{
			var targetCenter = current.GetNode<Marker2D>("Center");

			((Enemy)current).TakeDamage(Stats.Damage, Vector2.Zero);

			var fx = ProjectileScene.Instantiate<LightningChain>();
			fx.Setup(from.GlobalPosition, targetCenter.GlobalPosition);
			GetTree().CurrentScene.AddChild(fx);

			GD.Print($"Lightning: {from.GlobalPosition} to: {targetCenter.GlobalPosition}");

			from = targetCenter;
			current = GetNext(current);
		}
	}


	Node2D GetNext(Node2D fromEnemy)
	{
		var fromCenter = fromEnemy.GetNode<Marker2D>("Center").GlobalPosition;

		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Node2D enemy && enemy != fromEnemy)
			{
				var enemyCenter = enemy.GetNode<Marker2D>("Center").GlobalPosition;

				if (fromCenter.DistanceTo(enemyCenter) < 200)
					return enemy;
			}
		}

		return null;
	}
}
