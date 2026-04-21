using Godot;
using System;

/// <summary>
/// Ekran wyboru broni startowej, wyświetlany przed wejściem do gry.
/// Ustaw go jako CanvasLayer w scenie main_menu lub jako osobną scenę przejściową.
/// Po wyborze ładuje scenę gry.
/// </summary>
public partial class WeaponSelectUI : CanvasLayer
{
	// ── Dane broni ────────────────────────────────────────────

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
        "Topór z łukową trajektorią.\nWysoke obrażenia pojedynczego celu."
	};

	private static readonly string[] WeaponEmojis = {
		"🔥", "⚡", "🧄", "✨", "🪓"
	};

	// ── Węzły ────────────────────────────────────────────────

	private int _selectedIndex = 0;
	// Replace BuildUI() with these getters:
	private VBoxContainer Container => GetNode<VBoxContainer>("CenterContainer");
	private Button[] _buttons;
	private Label _descLabel;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		// Cache references to buttons
		_buttons = new Button[]
		{
			GetNode<Button>("CenterContainer/FireWandButton"),
			GetNode<Button>("CenterContainer/LightningButton"),
			GetNode<Button>("CenterContainer/GarlicButton"),
			GetNode<Button>("CenterContainer/MagicMissileButton"),
			GetNode<Button>("CenterContainer/AxeButton")
		};

		_descLabel = GetNode<Label>("CenterContainer/DescriptionLabel");

		// Connect start button
		GetNode<Button>("CenterContainer/StartButton").Pressed += StartGame;

		// Select first weapon
		SelectWeapon(0);
	}

	private void SelectWeapon(int index)
	{
		_selectedIndex = index;
		Player.SelectedStartWeaponIndex = index;

		// Aktualizuj wygląd przycisków
		for (int i = 0; i < _buttons.Length; i++)
		{
			if (i == index)
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

		// Zaktualizuj opis
		if (_descLabel != null)
			_descLabel.Text = WeaponDescriptions[index];
	}

	private void StartGame()
	{
		GetTree().ChangeSceneToFile("res://Scenes/game.tscn");
	}
}
