using Godot;

public partial class LightningBeam : Node2D
{
	private Line2D line;

	public override void _Ready()
	{
		line = GetNode<Line2D>("Line2D");
	}

	public void Setup(Vector2 from, Vector2 to)
	{
		line.ClearPoints();
		GlobalPosition = from;

		Vector2 dir = to - from;
		float length = dir.Length();
		Vector2 normal = dir.Normalized().Orthogonal();

		int segments = 6;

		for (int i = 0; i <= segments; i++)
		{
			float t = i / (float)segments;
			Vector2 point = dir * t;

			if (i != 0 && i != segments)
			{
				point += normal * (float)GD.RandRange(-15f, 15f);
			}

			line.AddPoint(point);
		}

		Animate();
	}

	private async void Animate()
	{
		// Small flicker effect
		for (int i = 0; i < 3; i++)
		{
			line.Width = (float)GD.RandRange(5f, 9f);
			await ToSignal(GetTree().CreateTimer(0.03f), "timeout");
		}

		QueueFree();
	}
}
