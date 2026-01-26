using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export]
	public float Speed = 150f;
	
	private CharacterBody2D player;
	
	public override void _Ready()
	{
		player = GetNode<CharacterBody2D>("/root/Game/Player");
	}
	
	public override void _PhysicsProcess(double delta)
	{
		
		if (player == null)
			return;

		Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
		Velocity = direction * Speed;
		
		if (GlobalPosition.DistanceTo(player.GlobalPosition) < 5f)
			Velocity = Vector2.Zero;

		MoveAndSlide();
	}
}
