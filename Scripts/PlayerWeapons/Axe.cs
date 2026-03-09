using Godot;

public partial class AxeProjectile : Projectile
{
	float time;

	public override void _PhysicsProcess(double delta)
	{
		time += (float)delta;
		Direction.Y -= time * 1.3f;
		base._PhysicsProcess(delta);
	}
}
