using Godot;

/// <summary>
/// Klasa przechowująca dane pasywnego ulepszenia.
/// Dziedziczy po <see cref="Resource"/>, dzięki czemu może być edytowana w edytorze Godot.
/// Definiuje nazwę, opis, typ oraz sposób skalowania efektu.
/// </summary>
[GlobalClass]
public partial class PassiveData : Resource
{
    /// <summary>
    /// Nazwa pasywki wyświetlana w UI (np. "Spinach").
    /// </summary>
    [Export] public string Name = "";

    /// <summary>
    /// Opis działania pasywki wyświetlany w UI lub tooltipach.
    /// </summary>
    [Export] public string Description = "";

    /// <summary>
    /// Typ pasywki określający, którą statystykę gracza modyfikuje.
    /// </summary>
    [Export] public PassiveType Type;

    /// <summary>
    /// Maksymalny poziom ulepszenia pasywki.
    /// </summary>
    [Export] public int MaxLevel = 5;

    /// <summary>
    /// Bonus dodawany na każdy poziom (np. 0.1 = +10%).
    /// </summary>
    [Export] public float BonusPerLevel = 0.1f;

    /// <summary>
    /// Aktualny poziom pasywki (0 = brak ulepszenia).
    /// </summary>
    public int CurrentLevel = 0;

    /// <summary>
    /// Informacja czy pasywka może być jeszcze ulepszana.
    /// </summary>
    public bool CanUpgrade => CurrentLevel < MaxLevel;

    /// <summary>
    /// Aplikuje efekt pasywki na gracza i zwiększa jej poziom.
    /// Modyfikuje odpowiednie mnożniki statystyk w zależności od typu
    /// oraz odświeża bronie gracza.
    /// </summary>
    /// <param name="player">Gracz, na którym stosowany jest efekt.</param>
    public void Apply(Player player)
    {
        CurrentLevel++;
        float bonus = BonusPerLevel;

        switch (Type)
        {
            case PassiveType.Spinach:
                // Zwiększenie obrażeń gracza
                player.DamageMultiplier += bonus;
                break;

            case PassiveType.Pummarola:
                // Skrócenie cooldownu (z dolnym limitem)
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

        // Odświeżenie wszystkich broni po zmianie statystyk
        player.RefreshAllWeapons();
    }
}