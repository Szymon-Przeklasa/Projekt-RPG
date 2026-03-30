using Godot;
[GlobalClass]

/// <summary>
/// Klasa reprezentująca dane konfiguracyjne przeciwnika.
/// Dziedziczy po Resource, dzięki czemu można tworzyć instancje w edytorze Godot (.tres/.res).
/// </summary>
public partial class EnemyStats : Resource
{
    /// <summary>
    /// Unikalny identyfikator typu wroga (np. "slime").
    /// </summary>
    [Export] public string MobID = "slime";

    /// <summary>
    /// Nazwa wyświetlana w grze (UI, tooltipy itp.).
    /// </summary>
    [Export] public string DisplayName = "Slime";

    /// <summary>
    /// Prędkość poruszania się wroga w jednostkach gry.
    /// </summary>
    [Export] public float Speed = 120f;

    /// <summary>
    /// Maksymalna liczba punktów życia wroga.
    /// </summary>
    [Export] public int MaxHealth = 30;

    /// <summary>
    /// Liczba punktów doświadczenia, które wróg daje po zabiciu.
    /// </summary>
    [Export] public int XpDrop = 1;

    /// <summary>
    /// Obrażenia kontaktowe przy zderzeniu z graczem.
    /// </summary>
    [Export] public int ContactDamage = 10;

    /// <summary>
    /// Skala wizualna wroga (1 = standardowa skala).
    /// </summary>
    [Export] public float Scale = 1f;

    /// <summary>
    /// Referencja do sceny wroga (PackedScene), która będzie instancjonowana w grze.
    /// </summary>
    [Export] public PackedScene Scene;
}