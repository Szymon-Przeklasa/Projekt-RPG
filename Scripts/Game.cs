using Godot;
using System;

public partial class Game : Node2D
{
	private Label _timerLabel;
	private double _elapsedTime = 0;
	private bool _timerRunning = false;

	public override void _Ready()
	{
		_timerLabel = GetNode<Label>("CanvasLayer/Timer"); // sprawdź ścieżkę
		_timerRunning = true;
	}

	public override void _Process(double delta)
	{
		if (!_timerRunning) return;
		
		_elapsedTime += delta;
		int minutes = (int)_elapsedTime / 60;
		int seconds = (int)_elapsedTime % 60;
		_timerLabel.Text = $"{minutes:00}:{seconds:00}";
	}
}
