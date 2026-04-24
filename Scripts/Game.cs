using Godot;
using System;

public partial class Game : Node2D
{
	private Label _timerLabel;
	private double _elapsedTime = 0;
	private bool _timerRunning = false;
	private bool _gameOver = false;

	private WinScreen _winScreen;

	private const double WinTime = 16 * 60;

	public override void _Ready()
	{
		_timerLabel = GetNode<Label>("CanvasLayer/Timer");
		_timerRunning = true;
		
		_winScreen = new WinScreen();
		AddChild(_winScreen);
	}

	public override void _Process(double delta)
	{
		if (!_timerRunning || GetTree().Paused || _gameOver) return;

		_elapsedTime += delta;

		double displayTime = Math.Min(_elapsedTime, WinTime);
		int minutes = (int)displayTime / 60;
		int seconds = (int)displayTime % 60;
		_timerLabel.Text = $"{minutes:00}:{seconds:00}";

		if (_elapsedTime >= WinTime)
		{
			_gameOver = true;
			OnWin();
		}
	}

	private void OnWin()
	{
		_timerRunning = false;

		var spawner = GetTree().CurrentScene.GetNodeOrNull<EnemySpawner>("EnemySpawner");
		if (spawner != null) spawner.ProcessMode = ProcessModeEnum.Disabled;

		var player = GetTree().GetFirstNodeInGroup("player") as Player;
		int playerLevel = player?.Level ?? 1;

		_winScreen.ShowResults(playerLevel, _elapsedTime);
	}

	public void StopTimer() => _timerRunning = false;
}
