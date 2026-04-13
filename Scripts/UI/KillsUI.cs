using Godot;
using System;

/// <summary>
/// Interfejs użytkownika wyświetlający statystyki zabójstw przeciwników.
/// Pobiera dane z KillManager i wyświetla je w formie listy.
/// </summary>
public partial class KillsUI : CanvasLayer
{
	/// <summary>
	/// Scena używana do tworzenia pojedynczego wpisu przeciwnika (MobEntry).
	/// </summary>
	[Export]
	public PackedScene MobEntryScene;
	
	private VBoxContainer _mobGroup;
	private ScrollContainer _scrollContainer;

	/// <summary>
	/// Metoda wywoływana po dodaniu węzła do drzewa sceny.
	/// Inicjalizuje widoczność UI i subskrybuje sygnały KillManager.
	/// </summary>
	public override void _Ready()
	{
		Visible = false;
		var killManager = GetNode<KillManager>("/root/KillManager");
		killManager.KillUpdated += OnKillUpdated;

		_mobGroup = GetNode<VBoxContainer>("Panel/ScrollContainer/VBoxContainer/MobGroup");
		_scrollContainer = GetNode<ScrollContainer>("Panel/ScrollContainer");
	}

	/// <summary>
	/// Obsługuje sygnał KillUpdated z KillManager.
	/// W tej chwili wypisuje informacje do konsoli.
	/// </summary>
	/// <param name="mobID">Identyfikator przeciwnika.</param>
	/// <param name="kills">Aktualna liczba zabójstw dla danego przeciwnika.</param>
	private void OnKillUpdated(string mobID, int kills)
	{
		GD.Print($"{mobID} kills: {kills}");
	}

	/// <summary>
	/// Wyświetla UI ze wszystkimi zabójstwami.
	/// Tworzy wpisy dla każdego przeciwnika i pauzuje grę.
	/// </summary>
	public void ShowKills()
	{
		Visible = true;
		GetTree().Paused = true;

		// Resetuj scroll na górę
		_scrollContainer.ScrollVertical = 0;
		
		// Usuń poprzednie wpisy
		foreach (Node child in _mobGroup.GetChildren())
			child.QueueFree();
		
		// Dodaj nowe wpisy
		foreach (var pair in KillManager.Instance.GetAllKills())
		{
			var entry = MobEntryScene.Instantiate<MobEntry>();
			entry.SetData(pair.Key, pair.Value);
			_mobGroup.AddChild(entry);
		}
	}

	/// <summary>
	/// Obsługuje kliknięcie w tło UI.
	/// Zamknie interfejs po kliknięciu lewym przyciskiem myszy.
	/// </summary>
	/// <param name="event">Zdarzenie wejścia myszy.</param>
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
	/// Metoda wywoływana co klatkę.
	/// </summary>
	/// <param name="delta">Czas od poprzedniej klatki.</param>
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Zamyka interfejs UI i wznawia grę.
	/// </summary>
	private void Close()
	{
		Visible = false;
		GetTree().Paused = false;
	}
}
