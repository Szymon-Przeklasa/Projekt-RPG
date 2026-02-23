using Godot;
using System;

public partial class KillsUI : CanvasLayer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;
	}
	public void ShowKills()
	{
		Visible = true;
		GetTree().Paused = true;
		
		var bg = GetNode<ColorRect>("ColorRect");
		bg.Color = new Color(0f,0f,0f,0.7f);
	}
	private void OnBackgroundClicked(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent &&
			mouseEvent.Pressed &&
			mouseEvent.ButtonIndex == MouseButton.Left)
		{
			Close();
		}
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void Close()
	{
		Visible = false;
		GetTree().Paused = false;
	}
}
