using Godot;

/// <summary>
/// Klasa reprezentująca dane pasywnego ulepszenia.
/// Dziedziczy po Resource, dzięki czemu może być łatwo edytowana w edytorze Godot.
/// Zawiera informacje o nazwie, opisie, typie oraz skalowaniu efektu.
/// </summary>
[GlobalClass]
public partial class PassiveData : Resource
{
    /// <summary>
    /// Nazwa pasywki wyświetlana w grze.
    /// </summary>
    [Export] public string Name = "";

    /// <summary>
    /// Opis działania pasywki (np. w UI lub tooltipie).
    /// </summary>
    [Export] public string Description = "";

    /// <summary>
    /// Typ pasywki określający, jaką statystykę modyfikuje.
    /// </summary>
    [Export] public PassiveType Type;

    /// <summary>
    /// Maksymalny poziom ulepszenia pasywki.
    /// </summary>
    [Export] public int MaxLevel = 5;

    /// <summary>
    /// Wartość bonusu dodawanego na każdy poziom (np. 0.1 = +10%).
    /// </summary>
    [Export] public float BonusPerLevel = 0.1f;

    /// <summary>
    /// Aktualny poziom pasywki.
    /// </summary>
    public int CurrentLevel = 0;

    /// <summary>
    /// Określa, czy pasywka może zostać jeszcze ulepszona.
    /// </summary>
    public bool CanUpgrade => CurrentLevel < MaxLevel;

    /// <summary>
    /// Aplikuje efekt pasywny na gracza oraz zwiększa poziom pasywki.
    /// Modyfikuje odpowiednie mnożniki statystyk w zależności od typu pasywki,
    /// a następnie odświeża wszystkie bronie gracza.
    /// </summary>
    /// <param name="player">Obiekt gracza, na którym zostanie zastosowany efekt.</param>
    public void Apply(Player player)
    {
        CurrentLevel++;
        float bonus = BonusPerLevel;

        switch (Type)
        {
            case PassiveType.Spinach:
                // Zwiększenie obrażeń
                player.DamageMultiplier += bonus;
                break;

            case PassiveType.Pummarola:
                // Skrócenie czasu odnowienia (z dolnym limitem)
                player.CooldownMultiplier = Mathf.Max(0.2f, player.CooldownMultiplier - bonus);
                break;

            case PassiveType.HollowHeart:
                // Zwiększenie obszaru działania
                player.AreaMultiplier += bonus;
                break;

            case PassiveType.Bracer:
                // Zwiększenie prędkości pocisków
                player.ProjectileSpeedMultiplier += bonus;
                break;

            case PassiveType.Wings:
                // Zwiększenie prędkości ruchu gracza
                player.SpeedMultiplier += bonus;
                break;
        }

        // Odśwież wszystkie bronie po zmianie statystyk
        player.RefreshAllWeapons();
    }
}