using Godot;

public partial class XpOrb : Area2D
{
	[Export] public int Value = 1;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node body)
	{
		if (body is Player player)
		{
			player.GainXp(Value);
			QueueFree();
		}
	}
}
