using Godot;

/// <summary>
/// Ekran wyboru broni startowej, wyświetlany przed wejściem do gry.
/// Ustaw go jako CanvasLayer w scenie main_menu lub jako osobną scenę przejściową.
/// Po wyborze ładuje scenę gry.
/// </summary>
public partial class WeaponSelectUI : CanvasLayer
{
	private static readonly string[] WeaponNames = {
		"Fire Wand",
		"Lightning",
		"Garlic",
		"Magic Missile",
        "Axe"
	};

	private static readonly string[] WeaponDescriptions = {
		"Strzela pociskami w najbliższego wroga.\nSzybki, niezawodny starter.",
		"Piorun skaczący między wrogami.\nDoskonały do tłumów.",
		"Aura obrażeń wokół gracza.\nDobry na duże skupiska.",
		"Samonaprowadzające pociski.\nŁatwy w użyciu.",
		"Topór z łukową trajektorią.\nWysokie obrażenia pojedynczego celu."
	};

	private static readonly string[] WeaponEmojis = {
		"🔥", "⚡", "🧄", "✨", "🪓"
	};

	private int _selectedIndex = 0;
	private Button[] _buttons;
	private Label _descLabel;
	private Button _startButton;
	private Button _backButton;

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

	private void StartGame()
	{
		GetTree().ChangeSceneToFile("res://Scenes/game.tscn");
	}

	private void ReturnToMainMenu()
	{
		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}
}
