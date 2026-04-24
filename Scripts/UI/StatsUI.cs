using Godot;
using System.Collections.Generic;

/// <summary>
/// Panel statystyk wyświetlany w menu pauzy.
/// Pokazuje poziom gracza, doświadczenie oraz rozwój:
/// <list type="bullet">
/// <item><description>broni (Weapons),</description></item>
/// <item><description>pasywek (Passives).</description></item>
/// </list>
///
/// Każdy element prezentowany jest w formie graficznej (● / ○),
/// gdzie wypełnione pola oznaczają zdobyte poziomy.
/// </summary>
public partial class StatsUI : HBoxContainer
{
    /// <summary>Label wyświetlający poziom gracza.</summary>
    private Label _levelValue;

    /// <summary>Label wyświetlający ilość doświadczenia.</summary>
    private Label _xpValue;

    /// <summary>
    /// Mapa labeli kropek (UI poziomów) przypisana do nazw przedmiotów.
    /// </summary>
    private readonly Dictionary<string, Label> _dotLabels = new();

    /// <summary>
    /// Lista broni wyświetlanych w panelu statystyk wraz z maksymalnym poziomem.
    /// </summary>
    private static readonly (string name, int max)[] WeaponEntries =
    {
        ("Fire Wand",     8),
        ("Lightning",     8),
        ("Garlic",        8),
        ("Magic Missile", 8),
        ("Axe",           8),
        ("Magnet",        5),
    };

    /// <summary>
    /// Lista pasywek wyświetlanych w panelu statystyk wraz z maksymalnym poziomem.
    /// </summary>
    private static readonly (string name, int max)[] PassiveEntries =
    {
        ("Spinach",       5),
        ("Pummarola",     5),
        ("Hollow Heart",  5),
        ("Bracer",        5),
        ("Wings",         5),
    };

    /// <summary>Czcionka używana w UI statystyk.</summary>
    private Font _font;

    /// <summary>Kolumna UI zawierająca bronie.</summary>
    private VBoxContainer _weaponColumn;

    /// <summary>Kolumna UI zawierająca pasywki.</summary>
    private VBoxContainer _passiveColumn;

    /// <summary>
    /// Inicjalizuje panel statystyk po dodaniu do sceny.
    /// Pobiera referencje do UI oraz buduje kolumny broni i pasywek.
    /// </summary>
    public override void _Ready()
    {
        _levelValue = GetNode<Label>("LeftColumn/StatValues/HBoxContainer/LevelValue");
        _xpValue = GetNode<Label>("LeftColumn/StatValues/HBoxContainer2/XPValue");
        _font = GD.Load<FontFile>("res://Textures/Jersey15-Regular.ttf");

        _weaponColumn = GetNode<VBoxContainer>("RightColumn");
        _passiveColumn = BuildPassiveColumn();

        BuildWeaponRows();
        BuildPassiveRows();
    }

    // ─────────────────────────────────────────────────────────────
    // Budowanie UI
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Tworzy kolumnę UI dla pasywek.
    /// </summary>
    private VBoxContainer BuildPassiveColumn()
    {
        var col = new VBoxContainer();
        col.LayoutMode = 2;
        col.SizeFlagsHorizontal = SizeFlags.Expand;
        AddChild(col);
        return col;
    }

    /// <summary>
    /// Buduje listę wierszy dla broni.
    /// </summary>
    private void BuildWeaponRows()
    {
        foreach (Node child in _weaponColumn.GetChildren())
        {
            _weaponColumn.RemoveChild(child);
            child.Free();
        }

        var header = MakeLabel("BRONIE", 18, new Color(0.7f, 0.85f, 1f));
        _weaponColumn.AddChild(header);

        foreach (var (name, max) in WeaponEntries)
        {
            var row = MakeUpgradeRow(name, 0, max);
            _weaponColumn.AddChild(row);
        }
    }

    /// <summary>
    /// Buduje listę wierszy dla pasywek.
    /// </summary>
    private void BuildPassiveRows()
    {
        var header = MakeLabel("PASYWKI", 18, new Color(0.7f, 1f, 0.75f));
        _passiveColumn.AddChild(header);

        foreach (var (name, max) in PassiveEntries)
        {
            var row = MakeUpgradeRow(name, 0, max);
            _passiveColumn.AddChild(row);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Tworzenie wierszy
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Tworzy pojedynczy wiersz statystyki w formie:
    /// Nazwa + graficzny pasek poziomu (■ □).
    /// </summary>
    /// <param name="itemName">Nazwa przedmiotu.</param>
    /// <param name="level">Aktualny poziom.</param>
    /// <param name="max">Maksymalny poziom.</param>
    /// <returns>Wiersz UI.</returns>
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

    // ─────────────────────────────────────────────────────────────
    // Aktualizacja UI
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Odświeża panel statystyk na podstawie danych gracza.
    /// </summary>
    /// <param name="player">Obiekt gracza.</param>
    public void Refresh(Player player)
    {
        if (_levelValue == null) return;

        if (player == null)
        {
            _levelValue.Text = "-";
            _xpValue.Text = "-";

            foreach (var lbl in _dotLabels.Values)
                lbl.Text = "";

            return;
        }

        _levelValue.Text = player.Level.ToString();
        _xpValue.Text = $"{player.Xp}/{player.XpToLevel}";

        foreach (var (name, max) in WeaponEntries)
            SetDots(name, GetWeaponLevel(player, name), max);

        foreach (var (name, max) in PassiveEntries)
            SetDots(name, GetPassiveLevel(player, name), max);
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────

    /// <summary>Ustawia graficzny pasek poziomu (■ □).</summary>
    private void SetDots(string name, int level, int max)
    {
        if (!_dotLabels.TryGetValue(name, out var lbl)) return;

        lbl.Text = DotsString(level, max);
        lbl.Modulate = DotColor(level, max);
    }

    /// <summary>Tworzy tekstowy pasek poziomu.</summary>
    private static string DotsString(int level, int max)
    {
        var sb = new System.Text.StringBuilder();

        for (int i = 0; i < max; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(i < level ? "■" : "□");
        }

        return sb.ToString();
    }

    /// <summary>Zwraca kolor paska poziomu.</summary>
    private static Color DotColor(int level, int max)
    {
        if (level == 0) return Colors.White;
        if (level >= max) return new Color(1f, 0.85f, 0.2f);
        return new Color(0.5f, 0.9f, 1f);
    }

    /// <summary>Pobiera poziom broni gracza.</summary>
    private static int GetWeaponLevel(Player player, string name)
    {
        foreach (var upg in player.AvailableUpgrades)
            if (upg.Type == UpgradeType.Weapon && upg.Name == name)
                return upg.Level;

        return 0;
    }

    /// <summary>Pobiera poziom pasywki gracza.</summary>
    private static int GetPassiveLevel(Player player, string name)
    {
        foreach (var passive in player.Passives)
            if (passive.Name == name)
                return passive.CurrentLevel;

        return 0;
    }

    /// <summary>Tworzy etykietę tekstową UI.</summary>
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