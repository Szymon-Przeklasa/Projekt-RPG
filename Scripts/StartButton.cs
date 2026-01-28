using Godot;
using System;

public partial class StartButton : TextureButton
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}
	private void StartGame()
	{
		GetTree().ChangeSceneToFile("res://Scenes/game.tscn");
	}
	private void MouseOn()
	{
		this.Modulate = new Color(0.8f,0.8f,0.8f,1f);
	}
	private void MouseOff()
	{
		this.Modulate = new Color(1f,1f,1f,1f);
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
