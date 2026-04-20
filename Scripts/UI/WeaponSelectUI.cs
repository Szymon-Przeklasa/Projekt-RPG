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

	private VBoxContainer _container;
	private int _selectedIndex = 0;
	private Button[] _buttons;
	private Label _descLabel;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		BuildUI();
	}

	private void BuildUI()
	{
		// Tło
		var bg = new ColorRect();
		bg.Color = new Color(0.05f, 0.05f, 0.07f, 0.95f);
		bg.AnchorRight = 1; bg.AnchorBottom = 1;
		bg.OffsetRight = 0; bg.OffsetBottom = 0;
		bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		AddChild(bg);

		// Główny kontener wycentrowany
		var center = new VBoxContainer();
		center.SetAnchorsPreset(Control.LayoutPreset.Center);
		center.GrowHorizontal = Control.GrowDirection.Both;
		center.GrowVertical = Control.GrowDirection.Both;
		center.CustomMinimumSize = new Vector2(500, 0);
		center.Alignment = BoxContainer.AlignmentMode.Center;
		AddChild(center);

		// Tytuł
		var title = new Label();
		title.Text = "WYBIERZ BROŃ STARTOWĄ";
		title.HorizontalAlignment = HorizontalAlignment.Center;
		title.AddThemeFontSizeOverride("font_size", 28);
		title.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.2f));
		center.AddChild(title);

		// Separator
		var sep = new Control();
		sep.CustomMinimumSize = new Vector2(0, 20);
		center.AddChild(sep);

		// Przyciski broni
		_buttons = new Button[WeaponNames.Length];
		for (int i = 0; i < WeaponNames.Length; i++)
		{
			int capturedIndex = i;
			var btn = new Button();
			btn.Text = $"{WeaponEmojis[i]}  {WeaponNames[i]}";
			btn.CustomMinimumSize = new Vector2(400, 55);
			btn.AddThemeFontSizeOverride("font_size", 18);
			btn.Pressed += () => SelectWeapon(capturedIndex);
			center.AddChild(btn);
			_buttons[i] = btn;
		}

		// Opis wybranej broni
		var sep2 = new Control();
		sep2.CustomMinimumSize = new Vector2(0, 15);
		center.AddChild(sep2);

		_descLabel = new Label();
		_descLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_descLabel.AddThemeFontSizeOverride("font_size", 15);
		_descLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
		_descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		_descLabel.CustomMinimumSize = new Vector2(400, 60);
		center.AddChild(_descLabel);

		// Przycisk START
		var sep3 = new Control();
		sep3.CustomMinimumSize = new Vector2(0, 10);
		center.AddChild(sep3);

		var startBtn = new Button();
		startBtn.Text = "▶  START";
		startBtn.CustomMinimumSize = new Vector2(400, 65);
		startBtn.AddThemeFontSizeOverride("font_size", 22);
		startBtn.AddThemeColorOverride("font_color", new Color(0.2f, 1f, 0.4f));
		startBtn.Pressed += StartGame;
		center.AddChild(startBtn);

		// Zaznacz domyślnie pierwszą broń
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
