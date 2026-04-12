using Godot;
using System;

public partial class MiniMap : SubViewport
{
	private CharacterBody2D _player;
	private Camera2D _camera;

	public override void _Ready()
	{
		_player = GetTree().GetFirstNodeInGroup("player") as CharacterBody2D;
		_camera = GetNode<Camera2D>("Camera2D");
		
		// Współdziel świat z główną sceną
		var viewport = GetNode<SubViewport>(".");
		viewport.World2D = GetTree().Root.World2D;
	}

	public override void _PhysicsProcess(double delta)
	{
		_camera.GlobalPosition = _player.GlobalPosition;
	}
}
