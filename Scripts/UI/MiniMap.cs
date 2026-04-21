using Godot;

public partial class MiniMap : SubViewport
{
	private CharacterBody2D _player;
	private Camera2D _camera;

	public override void _Ready()
	{
		_player = GetTree().GetFirstNodeInGroup("player") as CharacterBody2D;
		_camera = GetNode<Camera2D>("Camera2D");

		var sceneViewport = GetTree().CurrentScene?.GetViewport();
		if (sceneViewport != null)
			World2D = sceneViewport.World2D;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_player == null)
			_player = GetTree().GetFirstNodeInGroup("player") as CharacterBody2D;

		if (_player == null || _camera == null)
			return;

		_camera.GlobalPosition = _player.GlobalPosition;
	}
}
