using Godot;

/// <summary>
/// Menu pauzy obsługiwane klawiszem ESC.
/// Zatrzymuje: timer gry, spawner wrogów, ruch wrogów, obrażenia kontaktowe,
/// ruch gracza oraz wszystkie bronie gracza.
///
/// Problem: Player ma ProcessMode = Always (żeby GetInput() działał podczas LevelUpUI).
/// Bronie są dziećmi Playera i dziedziczą Always, więc GetTree().Paused ich nie zatrzymuje.
/// Rozwiązanie: ręcznie wyłączamy/włączamy ProcessMode broni przy pauzie.
/// </summary>
public partial class PauseMenu : CanvasLayer
{
	// ── Węzły UI ─────────────────────────────────────────────

	private Panel _panel;
	private Button _resumeButton;
	private Button _restartButton;
	private Button _quitButton;

	// ── Stan ─────────────────────────────────────────────────

	private bool _isPausedByThis = false;
	private Player _player;

	// ── Inicjalizacja ─────────────────────────────────────────

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Visible = false;

		_panel         = GetNode<Panel>("Panel");
		_resumeButton  = GetNode<Button>("Panel/VBoxContainer/ResumeButton");
		_restartButton = GetNode<Button>("Panel/VBoxContainer/RestartButton");
		_quitButton    = GetNode<Button>("Panel/VBoxContainer/QuitButton");

		_resumeButton.Pressed  += Resume;
		_restartButton.Pressed += Restart;
		_quitButton.Pressed    += QuitToMenu;

		_player = GetTree().GetFirstNodeInGroup("player") as Player;
	}

	// ── Obsługa klawisza ESC ──────────────────────────────────

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			if (GetTree().Paused && !_isPausedByThis)
				return;

			if (_isPausedByThis)
				Resume();
			else
				Pause();

			GetViewport().SetInputAsHandled();
		}
	}

	// ── Pauza ─────────────────────────────────────────────────

	private void Pause()
	{
		_isPausedByThis  = true;
		GetTree().Paused = true;
		Visible          = true;

		SetWeaponsProcessMode(ProcessModeEnum.Disabled);
	}

	// ── Wznowienie ────────────────────────────────────────────

	private void Resume()
	{
		_isPausedByThis  = false;
		GetTree().Paused = false;
		Visible          = false;

		SetWeaponsProcessMode(ProcessModeEnum.Inherit);
	}

	// ── Restart ───────────────────────────────────────────────

	private void Restart()
	{
		SetWeaponsProcessMode(ProcessModeEnum.Inherit);
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}

	// ── Powrót do menu ────────────────────────────────────────

	private void QuitToMenu()
	{
		SetWeaponsProcessMode(ProcessModeEnum.Inherit);
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}

	// ── Helper: przełącz ProcessMode wszystkich broni ─────────

	private void SetWeaponsProcessMode(ProcessModeEnum mode)
	{
		if (_player == null) return;

		foreach (var weapon in _player.Weapons)
			weapon.ProcessMode = mode;
	}
}
