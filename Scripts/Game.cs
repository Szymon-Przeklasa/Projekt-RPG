using Godot;
using System;

public partial class Game : Node2D
{
	private Label _timerLabel;
	private double _elapsedTime = 0;
	private bool _timerRunning = false;
	private bool _gameOver = false;

	/// <summary>Czas w sekundach, po którym gra kończy się wygraną.</summary>
	private const double WinTime = 20 * 60; // 20 minut

	public override void _Ready()
	{
		_timerLabel = GetNode<Label>("CanvasLayer/Timer");
		_timerRunning = true;
	}

	public override void _Process(double delta)
	{
		if (!_timerRunning || GetTree().Paused || _gameOver) return;

		_elapsedTime += delta;

		// Ogranicz wyświetlanie do 20:00
		double displayTime = Math.Min(_elapsedTime, WinTime);
		int minutes = (int)displayTime / 60;
		int seconds = (int)displayTime % 60;
		_timerLabel.Text = $"{minutes:00}:{seconds:00}";

		// Sprawdź koniec gry po 20 minutach
		if (_elapsedTime >= WinTime)
		{
			_gameOver = true;
			OnWin();
		}
	}

	private void OnWin()
	{
		_timerRunning = false;

		// Zatrzymaj spawner
		var spawner = GetTree().CurrentScene.GetNodeOrNull<EnemySpawner>("EnemySpawner");
		if (spawner != null) spawner.ProcessMode = ProcessModeEnum.Disabled;

		// Pokaż ekran wygranej jeśli istnieje, wpp wróć do menu
		var winScreen = GetTree().CurrentScene.GetNodeOrNull<CanvasLayer>("WinScreen");
		if (winScreen != null)
		{
			winScreen.ProcessMode = ProcessModeEnum.Always;
			winScreen.Visible = true;
		}
		else
		{
			// Fallback: wróć do menu po 3 sekundach
			GetTree().Paused = true;
			var timer = GetTree().CreateTimer(3.0);
			timer.Timeout += () =>
			{
				GetTree().Paused = false;
				GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
			};
		}
	}

	/// <summary>Publiczne API do zatrzymania timera (np. przy śmierci gracza).</summary>
	public void StopTimer() => _timerRunning = false;
}
