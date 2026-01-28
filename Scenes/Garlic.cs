using Godot;

public partial class Garlic : Weapon
{
	protected override void Fire()
	{
		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Enemy enemy)
			{
				if (Player.GlobalPosition.DistanceTo(enemy.GlobalPosition) <= Stats.Range)
				{
					enemy.TakeDamage(Stats.Damage, Vector2.Zero);
				}
			}
		}
	}
}
