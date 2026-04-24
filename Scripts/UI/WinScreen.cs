using Godot;
using System.Collections.Generic;

/// <summary>
/// End-of-round screen shown when the player survives 20 minutes.
/// Displays round stats: time survived, player level, total kills, and per-mob kill breakdown.
/// Built entirely in code — no .tscn needed.
/// </summary>
public partial class WinScreen : CanvasLayer
{
	private static readonly string[] MobOrder = { "slime", "vampire", "skeleton", "demon", "golem" };

	private Font _font;
	private VBoxContainer _root;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Visible     = false;

		_font = GD.Load<FontFile>("res://Textures/Jersey15-Regular.ttf");
		BuildLayout();
	}

	// ── Layout ────────────────────────────────────────────────

	private void BuildLayout()
	{
		// Dim background
		var bg = new ColorRect
		{
			Color              = new Color(0f, 0f, 0f, 0.82f),
			AnchorLeft         = 0, AnchorTop    = 0,
			AnchorRight        = 1, AnchorBottom = 1,
			GrowHorizontal     = Control.GrowDirection.Both,
			GrowVertical       = Control.GrowDirection.Both,
		};
		AddChild(bg);

		// Centered panel
		var panel = new PanelContainer();
		panel.AnchorLeft   = 0.5f; panel.AnchorRight  = 0.5f;
		panel.AnchorTop    = 0.5f; panel.AnchorBottom = 0.5f;
		panel.GrowHorizontal = Control.GrowDirection.Both;
		panel.GrowVertical   = Control.GrowDirection.Both;
		panel.CustomMinimumSize = new Vector2(460, 0);
		AddChild(panel);

		_root = new VBoxContainer();
		_root.AddThemeConstantOverride("separation", 10);

		var margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_left",   28);
		margin.AddThemeConstantOverride("margin_right",  28);
		margin.AddThemeConstantOverride("margin_top",    24);
		margin.AddThemeConstantOverride("margin_bottom", 24);
		margin.AddChild(_root);
		panel.AddChild(margin);
	}

	// ── Public API ────────────────────────────────────────────

	/// <summary>Called by Game.cs when the 16-minute timer expires.</summary>
	public void ShowResults(int playerLevel, double elapsedSeconds)
	{
		Visible = true;
		GetTree().Paused = true;

		// Clear previous content
		foreach (Node child in _root.GetChildren())
		{
			_root.RemoveChild(child);
			child.Free();
		}

		var km = KillManager.Instance;
		var sessionKills = km?.GetSessionKills() ?? new Dictionary<string, int>();

		int totalKills = 0;
		foreach (var v in sessionKills.Values) totalKills += v;

		int minutes = (int)elapsedSeconds / 60;
		int seconds = (int)elapsedSeconds % 60;

		// ── Title
		_root.AddChild(MakeLabel("YOU SURVIVED!", 36, new Color(1f, 0.85f, 0.2f), HorizontalAlignment.Center));
		_root.AddChild(MakeLabel("16:00", 28, Colors.White, HorizontalAlignment.Center));
		_root.AddChild(MakeSeparator());

		// ── Summary row
		var summary = new HBoxContainer();
		summary.AddThemeConstantOverride("separation", 20);
		summary.AddChild(MakeStat("Level", playerLevel.ToString()));
		summary.AddChild(MakeStat("Kills", totalKills.ToString()));
		_root.AddChild(summary);

		_root.AddChild(MakeSeparator());

		// ── Per-mob kill table
		_root.AddChild(MakeLabel("KILLS THIS RUN", 20, new Color(0.7f, 0.85f, 1f), HorizontalAlignment.Left));

		foreach (string mobId in MobOrder)
		{
			int count = sessionKills.TryGetValue(mobId, out int k) ? k : 0;
			_root.AddChild(MakeMobRow(mobId, count));
		}

		_root.AddChild(MakeSeparator());

		// ── Back to menu button
		var btn = new Button();
		btn.Text = "BACK TO MENU";
		btn.AddThemeFontOverride("font", _font);
		btn.AddThemeFontSizeOverride("font_size", 22);
		btn.Pressed += OnMenuPressed;
		_root.AddChild(btn);
	}

	// ── Helpers ───────────────────────────────────────────────

	private HBoxContainer MakeMobRow(string mobId, int count)
	{
		var row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 8);

		var name  = MakeLabel(mobId.Capitalize(), 20, Colors.White, HorizontalAlignment.Left);
		name.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		row.AddChild(name);

		var kills = MakeLabel(count.ToString(), 20, count > 0 ? new Color(0.5f, 0.9f, 1f) : new Color(0.5f, 0.5f, 0.5f), HorizontalAlignment.Right);
		row.AddChild(kills);

		return row;
	}

	private VBoxContainer MakeStat(string statName, string value)
	{
		var col = new VBoxContainer();
		col.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		col.AddChild(MakeLabel(value,    28, Colors.White,                   HorizontalAlignment.Center));
		col.AddChild(MakeLabel(statName, 16, new Color(0.6f, 0.6f, 0.6f), HorizontalAlignment.Center));
		return col;
	}

	private Label MakeLabel(string text, int size, Color color, HorizontalAlignment align)
	{
		var lbl = new Label();
		lbl.Text                = text;
		lbl.HorizontalAlignment = align;
		lbl.AddThemeFontOverride("font", _font);
		lbl.AddThemeFontSizeOverride("font_size", size);
		lbl.AddThemeColorOverride("font_color", color);
		return lbl;
	}

	private HSeparator MakeSeparator()
	{
		var sep = new HSeparator();
		sep.AddThemeConstantOverride("separation", 6);
		return sep;
	}

	private void OnMenuPressed()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
	}
}
