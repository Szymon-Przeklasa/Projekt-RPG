using Godot;

/// <summary>
/// Interfejs użytkownika wyświetlający statystyki zabitych przeciwników.
///
/// Dane pobierane są z <see cref="KillManager"/>, a następnie
/// prezentowane w formie przewijanej listy wpisów.
/// Każdy wpis reprezentowany jest przez scenę <see cref="MobEntry"/>.
///
/// Klasa dziedziczy po <see cref="CanvasLayer"/>.
/// </summary>
public partial class KillsUI : CanvasLayer
{
	/// <summary>
	/// Scena używana do tworzenia pojedynczego wpisu przeciwnika.
	/// </summary>
	[Export]
	public PackedScene MobEntryScene;

	/// <summary>
	/// Kontener przechowujący listę wpisów przeciwników.
	/// </summary>
	private VBoxContainer _mobGroup;

	/// <summary>
	/// Kontener przewijania listy zabójstw.
	/// </summary>
	private ScrollContainer _scrollContainer;

	/// <summary>
	/// Zapamiętany stan pauzy gry przed otwarciem okna.
	/// </summary>
	private bool _wasPaused;

	/// <summary>
	/// Kolejność wyświetlania typów przeciwników na liście.
	/// </summary>
	private readonly string[] _orderedMobIDs =
	{
		"slime",
		"vampire",
		"skeleton",
		"demon",
        "golem"
	};

	/// <summary>
	/// Inicjalizuje interfejs po dodaniu do drzewa sceny.
	/// Ustawia początkową niewidoczność panelu oraz
	/// subskrybuje zdarzenie aktualizacji zabójstw.
	/// </summary>
	public override void _Ready()
	{
		Visible = false;
		var killManager = GetNodeOrNull<KillManager>("/root/KillManager");
		if (killManager != null)
			killManager.KillUpdated += OnKillUpdated;

		_mobGroup = GetNode<VBoxContainer>("Panel/ScrollContainer/VBoxContainer/MobGroup");
		_scrollContainer = GetNode<ScrollContainer>("Panel/ScrollContainer");
	}

	/// <summary>
	/// Obsługuje zdarzenie aktualizacji liczby zabójstw.
	/// Aktualnie wypisuje informację diagnostyczną do konsoli.
	/// </summary>
	/// <param name="mobID">Identyfikator przeciwnika.</param>
	/// <param name="kills">Aktualna liczba zabójstw danego przeciwnika.</param>
	private void OnKillUpdated(string mobID, int kills)
	{
		GD.Print($"{mobID} kills: {kills}");
	}

	/// <summary>
	/// Wyświetla panel statystyk zabójstw.
	/// Czyści poprzednią zawartość, tworzy nowe wpisy
	/// i zatrzymuje rozgrywkę do momentu zamknięcia panelu.
	/// </summary>
	public void ShowKills()
	{
		if (Visible)
			return;

		_wasPaused = GetTree().Paused;
		Visible = true;
		GetTree().Paused = true;

		_scrollContainer.ScrollVertical = 0;

		foreach (Node child in _mobGroup.GetChildren())
			child.QueueFree();

		var allKills = KillManager.Instance?.GetAllKills()
			?? new System.Collections.Generic.Dictionary<string, int>();

		foreach (string mobID in _orderedMobIDs)
		{
			int killCount = allKills.ContainsKey(mobID) ? allKills[mobID] : 0;

			var entry = MobEntryScene.Instantiate<MobEntry>();
			_mobGroup.AddChild(entry);

			entry.SetData(mobID, killCount);
		}
	}

	/// <summary>
	/// Obsługuje kliknięcie w tło panelu.
	/// Zamknięcie następuje po kliknięciu lewym przyciskiem myszy.
	/// </summary>
	/// <param name="event">Zdarzenie wejściowe użytkownika.</param>
	private void OnBackgroundClicked(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent &&
			mouseEvent.Pressed &&
			mouseEvent.ButtonIndex == MouseButton.Left)
		{
			Close();
		}
	}

	/// <summary>
	/// Obsługuje globalne wejście użytkownika,
	/// umożliwiając zamknięcie panelu klawiszem anulowania.
	/// </summary>
	/// <param name="event">Zdarzenie wejściowe.</param>
	public override void _UnhandledInput(InputEvent @event)
	{
		if (!Visible)
			return;

		if (@event.IsActionPressed("ui_cancel"))
		{
			Close();
			GetViewport().SetInputAsHandled();
		}
	}

	/// <summary>
	/// Zamyka panel statystyk i przywraca poprzedni stan gry.
	/// </summary>
	private void Close()
	{
		Visible = false;
		GetTree().Paused = _wasPaused;
	}
}
