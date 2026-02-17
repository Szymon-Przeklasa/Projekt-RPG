using Godot;

public partial class Garlic : Weapon
{
	private GpuParticles2D aura;

	[Export] PackedScene ProjectileScene;
	protected override void Fire()
	{
		if (aura == null)
		{
			aura = ProjectileScene.Instantiate<GpuParticles2D>();
			Player.AddChild(aura);
			aura.Position = Vector2.Zero;
		}

		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Enemy enemy &&
				Player.GlobalPosition.DistanceTo(enemy.GlobalPosition) <= Stats.Range / 2)
			{
				enemy.TakeDamage(Stats.Damage, Vector2.Zero);
			}
		}
	}
}
