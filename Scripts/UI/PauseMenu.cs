using Godot;

/// <summary>
/// Interfejs menu pauzy wywoływany klawiszem ESC.
///
/// Menu:
/// <list type="bullet">
/// <item><description>zatrzymuje rozgrywkę,</description></item>
/// <item><description>blokuje działanie broni gracza,</description></item>
/// <item><description>umożliwia restart poziomu,</description></item>
/// <item><description>pozwala wyświetlić statystyki i autorów,</description></item>
/// <item><description>umożliwia powrót do menu głównego.</description></item>
/// </list>
///
/// Klasa dziedziczy po <see cref="CanvasLayer"/>.
/// </summary>
public partial class PauseMenu : CanvasLayer
{
	/// <summary>
	/// Główny panel tła menu pauzy.
	/// </summary>
	private Panel _panel;

	/// <summary>
	/// Przycisk restartujący aktualny poziom.
	/// </summary>
	private Button _restartButton;

	/// <summary>
	/// Przycisk otwierający panel statystyk.
	/// </summary>
	private Button _statsButton;

	/// <summary>
	/// Przycisk otwierający panel autorów.
	/// </summary>
	private Button _creditsButton;

	/// <summary>
	/// Przycisk powrotu do menu głównego.
	/// </summary>
	private Button _quitButton;

	/// <summary>
	/// Czas trwania aktualnej sesji gry.
	/// </summary>
	private double _sessionT;

	/// <summary>
	/// Etykieta wyświetlająca czas sesji.
	/// </summary>
	private Label _sessionLabel;

	/// <summary>
	/// Panel statystyk gracza.
	/// </summary>
	private StatsUI _statsUI;

	/// <summary>
	/// Panel autorów projektu.
	/// </summary>
	private CreditsUI _creditsUI;

	/// <summary>
	/// Domyślny motyw przycisków.
	/// </summary>
	private Theme _normalTheme;

	/// <summary>
	/// Motyw aktywnego przycisku.
	/// </summary>
	private Theme _openedTheme;

	/// <summary>
	/// Flaga określająca, czy pauza została aktywowana przez ten skrypt.
	/// </summary>
	private bool _isPausedByThis = false;

	/// <summary>
	/// Referencja do obiektu gracza.
	/// </summary>
	private Player _player;

	/// <summary>
	/// Inicjalizuje menu pauzy.
	/// Pobiera referencje do elementów UI, ładuje motywy
	/// oraz podłącza zdarzenia przycisków.
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

		_statsUI = GetNode<StatsUI>("Panel/VBoxContainer/StatsUI");
		_creditsUI = GetNode<CreditsUI>("Panel/VBoxContainer/CreditsUI");
		_normalTheme = GD.Load<Theme>("res://Textures/NewButton.tres");
		_openedTheme = GD.Load<Theme>("res://Textures/NewButtonOpened.tres");

		_restartButton.Pressed += Restart;
		_statsButton.Pressed += OpenStats;
		_creditsButton.Pressed += OpenCredits;
		_quitButton.Pressed += QuitToMenu;

		_player = GetTree().GetFirstNodeInGroup("player") as Player;
		OpenStats();
	}

	/// <summary>
	/// Obsługuje wejście użytkownika.
	/// Klawisz <c>ESC</c> przełącza pomiędzy pauzą i wznowieniem gry.
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

	/// <summary>
	/// Włącza stan pauzy.
	/// Zatrzymuje grę, wyświetla interfejs
	/// oraz blokuje działanie broni gracza.
	/// </summary>
	private void Pause()
	{
		_isPausedByThis = true;
		GetTree().Paused = true;
		Visible = true;

		SetWeaponsProcessMode(ProcessModeEnum.Disabled);
		_statsUI.Refresh(_player);
		OpenStats();
	}

	/// <summary>
	/// Wyłącza stan pauzy i wznawia grę.
	/// </summary>
	private void Resume()
	{
		_isPausedByThis = false;
		GetTree().Paused = false;
		Visible = false;

		SetWeaponsProcessMode(ProcessModeEnum.Inherit);
	}

	/// <summary>
	/// Restartuje aktualnie załadowaną scenę.
	/// </summary>
	private void Restart()
	{
		var tree = GetTree();
		if (tree == null) return;

		SetWeaponsProcessMode(ProcessModeEnum.Inherit);
		tree.Paused = false;
		tree.ReloadCurrentScene();
	}

	/// <summary>
	/// Kończy aktualną grę i przechodzi do menu głównego.
	/// </summary>
	private void QuitToMenu()
	{
		var tree = GetTree();
		if (tree == null) return;

		SetWeaponsProcessMode(ProcessModeEnum.Inherit);
		tree.Paused = false;
		tree.ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}

	/// <summary>
	/// Otwiera panel statystyk i ukrywa panel autorów.
	/// </summary>
	private void OpenStats()
	{
		_statsButton.Theme = _openedTheme;
		_creditsButton.Theme = _normalTheme;

		_statsUI.Refresh(_player);
		_statsUI.Visible = true;
		_creditsUI.Visible = false;
	}

	/// <summary>
	/// Otwiera panel autorów i ukrywa panel statystyk.
	/// </summary>
	private void OpenCredits()
	{
		_statsButton.Theme = _normalTheme;
		_statsUI.Visible = false;
		_creditsButton.Theme = _openedTheme;
		_creditsUI.Visible = true;
	}

	/// <summary>
	/// Ustawia tryb procesowania dla wszystkich broni gracza.
	/// Pozwala na ich zatrzymanie niezależnie od stanu gracza.
	/// </summary>
	/// <param name="mode">Docelowy tryb przetwarzania.</param>
	private void SetWeaponsProcessMode(ProcessModeEnum mode)
	{
		if (_player == null) return;

		foreach (var weapon in _player.Weapons)
			weapon.ProcessMode = mode;
	}

	/// <summary>
	/// Aktualizuje czas sesji oraz odświeża statystyki
	/// podczas wyświetlania menu pauzy.
	/// </summary>
	/// <param name="delta">Czas od poprzedniej klatki.</param>
	public override void _Process(double delta)
	{
		if (_sessionLabel == null)
			return;

		double _sessionT = Time.GetTicksMsec() / 1000.0;

		int hours = (int)_sessionT / 3600;
		int minutes = ((int)_sessionT % 3600) / 60;
		int seconds = (int)_sessionT % 60;

		_sessionLabel.Text = $"Session: {hours:00}:{minutes:00}:{seconds:00}";

		if (Visible)
			_statsUI.Refresh(_player);
	}
}
