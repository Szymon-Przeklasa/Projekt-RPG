using Godot;

/// <summary>
/// Ekran wyboru broni startowej wyświetlany przed rozpoczęciem rozgrywki.
/// Gracz może wybrać jedną z pięciu broni; wybór zapisywany jest w
/// <see cref="Player.SelectedStartWeaponIndex"/> i utrzymywany podczas zmiany sceny.
/// Obsługuje nawigację klawiaturą (strzałki, Enter, Escape) oraz kliknięcia myszą.
/// </summary>
public partial class WeaponSelectUI : CanvasLayer
{
	/// <summary>Nazwy broni wyświetlane na przyciskach wyboru.</summary>
	private static readonly string[] WeaponNames = {
		"Fire Wand",
		"Lightning",
		"Garlic",
		"Magic Missile",
        "Axe"
	};

	/// <summary>Opisy broni wyświetlane w etykiecie po wyborze.</summary>
	private static readonly string[] WeaponDescriptions = {
		"Strzela pociskami w najbliższego wroga.\nSzybki, niezawodny starter.",
		"Piorun skaczący między wrogami.\nDoskonały do tłumów.",
		"Aura obrażeń wokół gracza.\nDobry na duże skupiska.",
		"Samonaprowadzające pociski.\nŁatwy w użyciu.",
        "Topór z łukową trajektorią.\nWysokie obrażenia pojedynczego celu."
	};

	/// <summary>Emotikony broni wyświetlane na przyciskach obok nazw.</summary>
	private static readonly string[] WeaponEmojis = {
		"🔥", "⚡", "🧄", "✨", "🪓"
	};

	/// <summary>Aktualnie zaznaczony indeks broni (0–4).</summary>
	private int _selectedIndex = 0;

	/// <summary>Tablica przycisków wyboru broni (jeden na broń).</summary>
	private Button[] _buttons;

	/// <summary>Etykieta wyświetlająca opis aktualnie wybranej broni.</summary>
	private Label _descLabel;

	/// <summary>Przycisk potwierdzający wybór i rozpoczynający grę.</summary>
	private Button _startButton;

	/// <summary>Przycisk powrotu do menu głównego (opcjonalny — może nie istnieć w scenie).</summary>
	private Button _backButton;

	/// <summary>
	/// Inicjalizacja UI po dodaniu do sceny.
	/// Pobiera węzły przycisków, subskrybuje zdarzenia i domyślnie zaznacza pierwszą broń.
	/// </summary>
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		_buttons = new Button[]
		{
			GetNode<Button>("CenterContainer/FireWandButton"),
			GetNode<Button>("CenterContainer/LightningButton"),
			GetNode<Button>("CenterContainer/GarlicButton"),
			GetNode<Button>("CenterContainer/MagicMissileButton"),
			GetNode<Button>("CenterContainer/AxeButton")
		};

		_descLabel = GetNode<Label>("CenterContainer/DescriptionLabel");
		_startButton = GetNode<Button>("CenterContainer/StartButton");
		_backButton = GetNodeOrNull<Button>("CenterContainer/BackButton");

		for (int i = 0; i < _buttons.Length; i++)
		{
			int index = i;
			_buttons[i].Pressed += () => SelectWeapon(index);
		}

		_startButton.Pressed += StartGame;
		if (_backButton != null)
			_backButton.Pressed += ReturnToMainMenu;

		SelectWeapon(0);
		_buttons[0].GrabFocus();
	}

	/// <summary>
	/// Zaznacza broń o podanym indeksie.
	/// Indeks jest zawijany modularnie, więc nie może wyjść poza zakres.
	/// Aktualizuje styl zaznaczonego przycisku (żółty tekst + strzałka) i opis.
	/// Zapisuje wybór w <see cref="Player.SelectedStartWeaponIndex"/>.
	/// </summary>
	/// <param name="index">Indeks broni do zaznaczenia (0–4, zawijany modularnie).</param>
	private void SelectWeapon(int index)
	{
		_selectedIndex = Mathf.Wrap(index, 0, _buttons.Length);
		Player.SelectedStartWeaponIndex = _selectedIndex;

		for (int i = 0; i < _buttons.Length; i++)
		{
			if (i == _selectedIndex)
			{
				_buttons[i].AddThemeColorOverride("font_color", new Color(1f, 1f, 0.2f));
				_buttons[i].Text = $"► {WeaponEmojis[i]}  {WeaponNames[i]}";
			}
			else
			{
				_buttons[i].RemoveThemeColorOverride("font_color");
				_buttons[i].Text = $"   {WeaponEmojis[i]}  {WeaponNames[i]}";
			}
		}

		_descLabel.Text = WeaponDescriptions[_selectedIndex];
	}

	/// <summary>
	/// Obsługuje nieobsłużone zdarzenia klawiatury.
	/// Nawigacja w górę/dół zmienia zaznaczenie; Enter potwierdza; Escape wraca do menu.
	/// </summary>
	/// <param name="event">Zdarzenie wejściowe do sprawdzenia.</param>
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_up"))
		{
			SelectWeapon(_selectedIndex - 1);
			_buttons[_selectedIndex].GrabFocus();
			GetViewport().SetInputAsHandled();
			return;
		}

		if (@event.IsActionPressed("ui_down"))
		{
			SelectWeapon(_selectedIndex + 1);
			_buttons[_selectedIndex].GrabFocus();
			GetViewport().SetInputAsHandled();
			return;
		}

		if (@event.IsActionPressed("ui_accept"))
		{
			StartGame();
			GetViewport().SetInputAsHandled();
			return;
		}

		if (@event.IsActionPressed("ui_cancel"))
		{
			ReturnToMainMenu();
			GetViewport().SetInputAsHandled();
		}
	}

	/// <summary>
	/// Ładuje główną scenę gry, kończąc ekran wyboru broni.
	/// </summary>
	private void StartGame()
	{
		GetTree().ChangeSceneToFile("res://Scenes/game.tscn");
	}

	/// <summary>
	/// Wraca do sceny menu głównego bez rozpoczynania gry.
	/// </summary>
	private void ReturnToMainMenu()
	{
		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}
}
