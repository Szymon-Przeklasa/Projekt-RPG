using Godot;
using System.Collections.Generic;

/// <summary>
/// Panel statystyk wyświetlany w menu pauzy.
/// Generuje wiersze dla broni i pasywek z kompaktowym wyświetlaniem poziomów (●○ kropki).
/// </summary>
public partial class StatsUI : HBoxContainer
{
	private Label _levelValue;
	private Label _xpValue;

	// Dynamicznie tworzone etykiety kropek (name → label)
	private readonly Dictionary<string, Label> _dotLabels = new();

	// Nazwy broni w kolejności wyświetlania (z max poziomami)
	private static readonly (string name, int max)[] WeaponEntries =
	{
		("Fire Wand",     8),
		("Lightning",     8),
		("Garlic",        8),
		("Magic Missile", 8),
		("Axe",           8),
		("Magnet",        5),
	};

	// Nazwy pasywek w kolejności wyświetlania (z max poziomami)
	private static readonly (string name, int max)[] PassiveEntries =
	{
		("Spinach",       5),
		("Pummarola",     5),
		("Hollow Heart",  5),
		("Bracer",        5),
		("Wings",         5),
	};

	private Font _font;
	private VBoxContainer _weaponColumn;
	private VBoxContainer _passiveColumn;

	public override void _Ready()
	{
		_levelValue = GetNode<Label>("LeftColumn/StatValues/HBoxContainer/LevelValue");
		_xpValue    = GetNode<Label>("LeftColumn/StatValues/HBoxContainer2/XPValue");
		_font        = GD.Load<FontFile>("res://Textures/Jersey15-Regular.ttf");

		// Use existing RightColumn for weapons, add a third column for passives
		_weaponColumn  = GetNode<VBoxContainer>("RightColumn");
		_passiveColumn = BuildPassiveColumn();

		BuildWeaponRows();
		BuildPassiveRows();
	}

	// ── Build columns ─────────────────────────────────────────

	private VBoxContainer BuildPassiveColumn()
	{
		var col = new VBoxContainer();
		col.LayoutMode = 2;
		col.SizeFlagsHorizontal = SizeFlags.Expand;
		AddChild(col);
		return col;
	}

	private void BuildWeaponRows()
	{
		// Remove existing children (ProgressBar rows from scene)
		foreach (Node child in _weaponColumn.GetChildren())
		{
			_weaponColumn.RemoveChild(child);
			child.Free();
		}

		var header = MakeLabel("WEAPONS", 18, new Color(0.7f, 0.85f, 1f));
		_weaponColumn.AddChild(header);

		foreach (var (name, max) in WeaponEntries)
		{
			var row = MakeUpgradeRow(name, 0, max);
			_weaponColumn.AddChild(row);
		}
	}

	private void BuildPassiveRows()
	{
		var header = MakeLabel("PASSIVES", 18, new Color(0.7f, 1f, 0.75f));
		_passiveColumn.AddChild(header);

		foreach (var (name, max) in PassiveEntries)
		{
			var row = MakeUpgradeRow(name, 0, max);
			_passiveColumn.AddChild(row);
		}
	}

	// ── Row factory ───────────────────────────────────────────

	/// <summary>Creates a compact row: "Name   ●●●○○" </summary>
	private HBoxContainer MakeUpgradeRow(string itemName, int level, int max)
	{
		var row = new HBoxContainer();
		row.LayoutMode = 2;
		row.AddThemeConstantOverride("separation", 6);

		var nameLabel = MakeLabel(itemName, 18, new Color(0.85f, 0.85f, 0.85f));
		nameLabel.SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill;
		row.AddChild(nameLabel);

		var dotsLabel = MakeLabel(DotsString(level, max), 14, DotColor(level, max));
		dotsLabel.HorizontalAlignment = HorizontalAlignment.Right;
		_dotLabels[itemName] = dotsLabel;
		row.AddChild(dotsLabel);

		return row;
	}

	// ── Refresh ───────────────────────────────────────────────

	public void Refresh(Player player)
	{
		if (_levelValue == null) return;

		if (player == null)
		{
			_levelValue.Text = "-";
			_xpValue.Text    = "-";
			foreach (var lbl in _dotLabels.Values)
				lbl.Text = "";
			return;
		}

		_levelValue.Text = player.Level.ToString();
		_xpValue.Text    = $"{player.Xp}/{player.XpToLevel}";

		// Weapons
		foreach (var (name, max) in WeaponEntries)
		{
			int lvl = GetWeaponLevel(player, name);
			SetDots(name, lvl, max);
		}

		// Passives
		foreach (var (name, max) in PassiveEntries)
		{
			int lvl = GetPassiveLevel(player, name);
			SetDots(name, lvl, max);
		}
	}

	// ── Helpers ───────────────────────────────────────────────

	private void SetDots(string name, int level, int max)
	{
		if (!_dotLabels.TryGetValue(name, out var lbl)) return;
		lbl.Text    = DotsString(level, max);
		lbl.Modulate = DotColor(level, max);
	}

	private static string DotsString(int level, int max)
	{
		var sb = new System.Text.StringBuilder();
		for (int i = 0; i < max; i++)
		{
			if (i > 0) sb.Append(' ');
			sb.Append(i < level ? "\u25a0" : "\u25a1"); // ■ filled, □ empty
		}
		return sb.ToString();
	}

	private static Color DotColor(int level, int max)
	{
		if (level == 0)        return Colors.White;
		if (level >= max)      return new Color(1f, 0.85f, 0.2f);   // gold = maxed
		return new Color(0.5f, 0.9f, 1f);                            // cyan = in progress
	}

	private static int GetWeaponLevel(Player player, string name)
	{
		foreach (var upg in player.AvailableUpgrades)
			if (upg.Type == UpgradeType.Weapon && upg.Name == name)
				return upg.Level;
		return 0;
	}

	private static int GetPassiveLevel(Player player, string name)
	{
		foreach (var passive in player.Passives)
			if (passive.Name == name)
				return passive.CurrentLevel;
		return 0;
	}

	private Label MakeLabel(string text, int fontSize, Color color)
	{
		var lbl = new Label();
		lbl.LayoutMode = 2;
		lbl.Text = text;
		lbl.AddThemeFontOverride("font", _font);
		lbl.AddThemeFontSizeOverride("font_size", fontSize);
		lbl.AddThemeColorOverride("font_color", color);
		return lbl;
	}
}
