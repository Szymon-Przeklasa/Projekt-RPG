using Godot;

/// <summary>
/// Menu pauzy obsługiwane klawiszem ESC.
/// Zatrzymuje: timer gry, spawner wrogów, ruch wrogów, obrażenia kontaktowe,
/// ruch gracza oraz wszystkie bronie gracza.
/// </summary>
public partial class PauseMenu : CanvasLayer
{
	// ── Węzły UI ─────────────────────────────────────────────

	/// <summary>Główny panel tła menu pauzy.</summary>
	private Panel _panel;
	/// <summary>Przycisk wznawiający rozgrywkę.</summary>
	private Button _resumeButton;
	/// <summary>Przycisk restartujący aktualny poziom.</summary>
	private Button _restartButton;
	/// <summary>Przycisk wyjścia do menu głównego.</summary>
	private Button _quitButton;

	// ── Stan ─────────────────────────────────────────────────

	/// <summary>Flaga sprawdzająca, czy pauza została wywołana przez ten skrypt.</summary>
	private bool _isPausedByThis = false;
	/// <summary>Referencja do obiektu gracza.</summary>
	private Player _player;

	// ── Inicjalizacja ─────────────────────────────────────────

	/// <summary>
	/// Inicjalizuje węzły UI, podłącza sygnały przycisków i znajduje gracza w grupie.
	/// </summary>
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

	/// <summary>
	/// Przechwytuje wejście klawisza ESC (ui_cancel) i przełącza stan pauzy.
	/// </summary>
	/// <param name="event">Zdarzenie wejściowe.</param>
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

	/// <summary>
	/// Aktywuje stan pauzy: zatrzymuje drzewo scen, pokazuje UI i wyłącza procesowanie broni.
	/// </summary>
	private void Pause()
	{
		_isPausedByThis = true;
		GetTree().Paused = true;
		Visible = true;

		SetWeaponsProcessMode(ProcessModeEnum.Disabled);
	}

	// ── Wznowienie ────────────────────────────────────────────

	/// <summary>
	/// Wznawia grę: ukrywa UI, odblokowuje drzewo scen i przywraca procesowanie broni.
	/// </summary>
	private void Resume()
	{
		_isPausedByThis = false;
		GetTree().Paused = false;
		Visible = false;

		SetWeaponsProcessMode(ProcessModeEnum.Inherit);
	}

	// ── Restart ───────────────────────────────────────────────

	/// <summary>
	/// Restartuje bieżącą scenę po uprzednim odblokowaniu procesów gry.
	/// </summary>
	private void Restart()
	{
		var tree = GetTree();
		if (tree == null) return;

		SetWeaponsProcessMode(ProcessModeEnum.Inherit);
		tree.Paused = false;
		tree.ReloadCurrentScene();
	}

	// ── Powrót do menu ────────────────────────────────────────

	/// <summary>
	/// Czyści stan pauzy i zmienia scenę na menu główne.
	/// </summary>
	private void QuitToMenu()
	{
		var tree = GetTree();
		if (tree == null) return;

		SetWeaponsProcessMode(ProcessModeEnum.Inherit);
		tree.Paused = false;
		tree.ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}

	// ── Helper: przełącz ProcessMode wszystkich broni ─────────

	/// <summary>
	/// Zmienia tryb procesowania dla wszystkich broni gracza.
	/// Pozwala to na zatrzymanie broni, nawet gdy gracz ma tryb Always.
	/// </summary>
	/// <param name="mode">Docelowy tryb ProcessMode.</param>
	private void SetWeaponsProcessMode(ProcessModeEnum mode)
	{
		if (_player == null) return;

		foreach (var weapon in _player.Weapons)
			weapon.ProcessMode = mode;
	}
}
