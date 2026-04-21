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
	/// <summary>Przycisk restartujący aktualny poziom.</summary>
	private Button _restartButton;
	/// <summary>Przycisk wyświetlający statystyki.</summary>
	private Button _statsButton;
	/// <summary>Przycisk wyświetlający dane autorów.</summary>
	private Button _creditsButton;
	/// <summary>Przycisk wyjścia do menu głównego.</summary>
	private Button _quitButton;
	/// <summary>Zmienna czasu sesji gry.</summary>
	private double _sessionT;
	/// <summary>Zmienna Labela czasu sesji gry.</summary>
	private Label _sessionLabel;
	
	private HBoxContainer _statsUI;
	private HBoxContainer _creditsUI;
	private Theme _normalTheme;
	private Theme _openedTheme;

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

		_panel = GetNode<Panel>("Panel");
		_restartButton = GetNode<Button>("Panel/VBoxContainer/HBoxContainer/RestartButton");
		_statsButton = GetNode<Button>("Panel/VBoxContainer/HBoxContainer2/StatsButton");
		_creditsButton = GetNode<Button>("Panel/VBoxContainer/HBoxContainer2/CreditsButton");
		_quitButton = GetNode<Button>("Panel/VBoxContainer/HBoxContainer2/QuitButton");
		
		_sessionLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer/SessionLabel");
		
		_statsUI = GetNode<HBoxContainer>("Panel/VBoxContainer/StatsUI");
		_creditsUI = GetNode<HBoxContainer>("Panel/VBoxContainer/CreditsUI");
		_normalTheme = GD.Load<Theme>("res://Textures/NewButton.tres");
		_openedTheme = GD.Load<Theme>("res://Textures/NewButtonOpened.tres");
		
		_restartButton.Pressed += Restart;
		_statsButton.Pressed += OpenStats;
		_creditsButton.Pressed += OpenCredits;
		_quitButton.Pressed += QuitToMenu;

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
	
	private void OpenStats()
	{
		_statsButton.Theme = _openedTheme;
		_creditsButton.Theme = _normalTheme;

		_statsUI.Visible = true;
		_creditsUI.Visible = false;
	}
	private void OpenCredits()
	{
		_statsButton.Theme = _normalTheme;
		_statsUI.Visible = false;
		_creditsButton.Theme = _openedTheme;
		_creditsUI.Visible = true;
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
	
	public override void _Process(double delta)
	{
		if (_sessionLabel == null)
		return;

		double time = Time.GetTicksMsec() / 1000.0;

		int hours = (int)time / 3600;
		int minutes = ((int)time % 3600) / 60;
		int seconds = (int)time % 60;

		_sessionLabel.Text = $"Session: {hours:00}:{minutes:00}:{seconds:00}";
	}
}
