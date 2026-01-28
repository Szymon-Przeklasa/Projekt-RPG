using Godot;

public partial class Garlic : Weapon
{
	[Export] PackedScene ProjectileScene;
	
	private GPUParticles2D aura;

	public override void _Ready()
	{
		aura = ProjectileScene.Instantiate<GPUParticles2D>();
		Player.AddChild(aura);
		aura.Position = Vector2.Zero;
	}
	
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
