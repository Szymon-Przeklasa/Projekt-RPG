using Godot;

public partial class Magnet : Weapon
{
	[Export] PackedScene ProjectileScene;
	
	protected override void Fire()
	{
		foreach (Node node in GetTree().GetNodesInGroup("xp"))
		{
			if (node is Node2D orb &&
				Player.GlobalPosition.DistanceTo(orb.GlobalPosition) <= Stats.Range)
			{
				orb.GlobalPosition =
					orb.GlobalPosition.Lerp(Player.GlobalPosition, 0.15f);
			}
		}
	}
}
