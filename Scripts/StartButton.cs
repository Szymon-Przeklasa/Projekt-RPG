using Godot;
using System;

public partial class StartButton : TextureButton
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//this.PivotOffsetRatio = new Vector2(0.5f, 0.5f);
		//this.Position = new Vector2(728f, 488f);
	}
	private void StartGame()
	{
		GetTree().ChangeSceneToFile("res://Scenes/game.tscn");
	}
	private void MouseOn()
	{
		this.Scale = new Vector2(0.45f, 0.45f);
	}
	private void MouseOff()
	{
		this.Scale = new Vector2(0.4f, 0.4f);
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
