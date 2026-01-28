using Godot;

public partial class Lightning : Weapon
{
	[Export] PackedScene ProjectileScene;
	
	protected override void Fire()
{
	Node2D current = Player.GetClosestEnemy(Stats.Range);
	int chains = Stats.Pierce;

	Node2D from = Player;

	while (current != null && chains-- > 0)
	{
		((Enemy)current).TakeDamage(Stats.Damage, Vector2.Zero);

		// ⚡ visual chain
		var fx = ProjectileScene.Instantiate<LightningChain>();
		fx.Setup(from.GlobalPosition, current.GlobalPosition);
		GetTree().CurrentScene.AddChild(fx);

		from = current;
		current = GetNext(from);
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
