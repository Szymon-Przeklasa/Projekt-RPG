using Godot;
using System.Collections.Generic;

/// <summary>
/// Ekran końcowy rundy wyświetlany po przetrwaniu 16 minut.
/// Pokazuje podsumowanie rozgrywki, w tym:
/// <list type="bullet">
/// <item><description>czas przetrwania,</description></item>
/// <item><description>poziom gracza,</description></item>
/// <item><description>łączną liczbę zabójstw,</description></item>
/// <item><description>rozkład zabójstw według przeciwników.</description></item>
/// </list>
///
/// Interfejs jest tworzony w całości w kodzie (bez pliku .tscn).
/// Klasa dziedziczy po <see cref="CanvasLayer"/>.
/// </summary>
public partial class WinScreen : CanvasLayer
{
    /// <summary>
    /// Kolejność wyświetlania przeciwników w tabeli wyników.
    /// </summary>
    private static readonly string[] MobOrder = { "slime", "vampire", "skeleton", "demon", "golem" };

    /// <summary>
    /// Czcionka używana w ekranie końcowym.
    /// </summary>
    private Font _font;

    /// <summary>
    /// Główny kontener UI zawierający wszystkie elementy ekranu końcowego.
    /// </summary>
    private VBoxContainer _root;

    /// <summary>
    /// Inicjalizuje ekran końcowy po dodaniu do drzewa sceny.
    /// Ustawia tryb procesowania, ukrywa UI i buduje layout interfejsu.
    /// </summary>
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;

        _font = GD.Load<FontFile>("res://Textures/Jersey15-Regular.ttf");
        BuildLayout();
    }

    /// <summary>
    /// Buduje statyczny układ interfejsu ekranu końcowego.
    /// Tworzy tło, panel centralny oraz kontener na zawartość.
    /// </summary>
    private void BuildLayout()
    {
        // Przyciemnione tło
        var bg = new ColorRect
        {
            Color = new Color(0f, 0f, 0f, 0.82f),
            AnchorLeft = 0,
            AnchorTop = 0,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = Control.GrowDirection.Both,
            GrowVertical = Control.GrowDirection.Both,
        };
        AddChild(bg);

        // Panel centralny
        var panel = new PanelContainer();
        panel.AnchorLeft = 0.5f; panel.AnchorRight = 0.5f;
        panel.AnchorTop = 0.5f; panel.AnchorBottom = 0.5f;
        panel.GrowHorizontal = Control.GrowDirection.Both;
        panel.GrowVertical = Control.GrowDirection.Both;
        panel.CustomMinimumSize = new Vector2(460, 0);
        AddChild(panel);

        _root = new VBoxContainer();
        _root.AddThemeConstantOverride("separation", 10);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 28);
        margin.AddThemeConstantOverride("margin_right", 28);
        margin.AddThemeConstantOverride("margin_top", 24);
        margin.AddThemeConstantOverride("margin_bottom", 24);
        margin.AddChild(_root);
        panel.AddChild(margin);
    }

    /// <summary>
    /// Wyświetla ekran wyników końcowych gry.
    /// Pobiera statystyki z <see cref="KillManager"/> i blokuje rozgrywkę.
    /// </summary>
    /// <param name="playerLevel">Poziom gracza na koniec rundy.</param>
    /// <param name="elapsedSeconds">Czas trwania rundy w sekundach.</param>
    public void ShowResults(int playerLevel, double elapsedSeconds)
    {
        Visible = true;
        GetTree().Paused = true;

        // Czyszczenie poprzednich elementów UI
        foreach (Node child in _root.GetChildren())
        {
            _root.RemoveChild(child);
            child.Free();
        }

        var km = KillManager.Instance;
        var sessionKills = km?.GetSessionKills() ?? new Dictionary<string, int>();

        int totalKills = 0;
        foreach (var v in sessionKills.Values)
            totalKills += v;

        int minutes = (int)elapsedSeconds / 60;
        int seconds = (int)elapsedSeconds % 60;

        // Nagłówek
        _root.AddChild(MakeLabel("PRZETRWAŁEŚ!", 36, new Color(1f, 0.85f, 0.2f), HorizontalAlignment.Center));
        _root.AddChild(MakeLabel("16:00", 28, Colors.White, HorizontalAlignment.Center));
        _root.AddChild(MakeSeparator());

        // Podsumowanie
        var summary = new HBoxContainer();
        summary.AddThemeConstantOverride("separation", 20);
        summary.AddChild(MakeStat("Poziom", playerLevel.ToString()));
        summary.AddChild(MakeStat("Zabójstwa", totalKills.ToString()));
        _root.AddChild(summary);

        _root.AddChild(MakeSeparator());

        // Tabela zabójstw
        _root.AddChild(MakeLabel("ZABÓJSTWA W TEJ RUNDZIE", 20, new Color(0.7f, 0.85f, 1f), HorizontalAlignment.Left));

        foreach (string mobId in MobOrder)
        {
            int count = sessionKills.TryGetValue(mobId, out int k) ? k : 0;
            _root.AddChild(MakeMobRow(mobId, count));
        }

        _root.AddChild(MakeSeparator());

        // Przycisk powrotu
        var btn = new Button();
        btn.Text = "POWRÓT DO MENU";
        btn.AddThemeFontOverride("font", _font);
        btn.AddThemeFontSizeOverride("font_size", 22);
        btn.Pressed += OnMenuPressed;
        _root.AddChild(btn);
    }

    /// <summary>
    /// Tworzy wiersz tabeli z zabójstwami konkretnego przeciwnika.
    /// </summary>
    /// <param name="mobId">Identyfikator przeciwnika.</param>
    /// <param name="count">Liczba zabójstw.</param>
    /// <returns>Wiersz UI przedstawiający wynik.</returns>
    private HBoxContainer MakeMobRow(string mobId, int count)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 8);

        var name = MakeLabel(mobId.Capitalize(), 20, Colors.White, HorizontalAlignment.Left);
        name.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        row.AddChild(name);

        var kills = MakeLabel(
            count.ToString(),
            20,
            count > 0 ? new Color(0.5f, 0.9f, 1f) : new Color(0.5f, 0.5f, 0.5f),
            HorizontalAlignment.Right
        );

        row.AddChild(kills);
        return row;
    }

    /// <summary>
    /// Tworzy blok statystyki (np. poziom lub liczba zabójstw).
    /// </summary>
    /// <param name="statName">Nazwa statystyki.</param>
    /// <param name="value">Wartość statystyki.</param>
    /// <returns>Kontener UI ze statystyką.</returns>
    private VBoxContainer MakeStat(string statName, string value)
    {
        var col = new VBoxContainer();
        col.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        col.AddChild(MakeLabel(value, 28, Colors.White, HorizontalAlignment.Center));
        col.AddChild(MakeLabel(statName, 16, new Color(0.6f, 0.6f, 0.6f), HorizontalAlignment.Center));
        return col;
    }

    /// <summary>
    /// Tworzy etykietę tekstową UI.
    /// </summary>
    /// <param name="text">Tekst do wyświetlenia.</param>
    /// <param name="size">Rozmiar czcionki.</param>
    /// <param name="color">Kolor tekstu.</param>
    /// <param name="align">Wyrównanie tekstu.</param>
    /// <returns>Element Label.</returns>
    private Label MakeLabel(string text, int size, Color color, HorizontalAlignment align)
    {
        var lbl = new Label();
        lbl.Text = text;
        lbl.HorizontalAlignment = align;
        lbl.AddThemeFontOverride("font", _font);
        lbl.AddThemeFontSizeOverride("font_size", size);
        lbl.AddThemeColorOverride("font_color", color);
        return lbl;
    }

    /// <summary>
    /// Tworzy separator wizualny w UI.
    /// </summary>
    /// <returns>Element separatora.</returns>
    private HSeparator MakeSeparator()
    {
        var sep = new HSeparator();
        sep.AddThemeConstantOverride("separation", 6);
        return sep;
    }

    /// <summary>
    /// Obsługuje kliknięcie przycisku powrotu do menu głównego.
    /// </summary>
    private void OnMenuPressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
    }
}