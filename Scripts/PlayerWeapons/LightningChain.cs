using Godot;

public partial class LightningChain : GpuParticles2D
{
	public void Setup(Vector2 from, Vector2 to)
	{
		GlobalPosition = from;

		var direction = (to - from).Normalized();
		Rotation = direction.Angle();

		float distance = from.DistanceTo(to);

		Scale = new Vector2(distance / 100f, 1f);

		Restart();
		Emitting = true;
	}

	public override async void _Ready()
	{
		Emitting = true;
		await ToSignal(GetTree().CreateTimer(Lifetime), "timeout");
		QueueFree();
	}
}
