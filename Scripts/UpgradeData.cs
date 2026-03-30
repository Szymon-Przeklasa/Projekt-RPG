using System;

/// <summary>
/// Typ ulepszenia dostępnego dla gracza.
/// Określa kategorię, do której należy dane ulepszenie.
/// </summary>
public enum UpgradeType
{
    /// <summary>
    /// Ulepszenie związane z bronią (np. nowe bronie lub ich rozwój).
    /// </summary>
    Weapon,

    /// <summary>
    /// Ulepszenie pasywne wpływające na statystyki pośrednio (np. bonusy procentowe).
    /// </summary>
    Passive,

    /// <summary>
    /// Ulepszenie bezpośrednio modyfikujące globalne statystyki gracza
    /// (np. DamageMultiplier, SpeedMultiplier).
    /// </summary>
    Stat
}

/// <summary>
/// Klasa reprezentująca pojedyncze ulepszenie dostępne dla gracza.
/// Przechowuje dane o nazwie, typie, poziomie oraz logikę aplikowania efektu.
/// </summary>
public class UpgradeData
{
    /// <summary>
    /// Nazwa ulepszenia wyświetlana w grze.
    /// </summary>
    public string Name;

    /// <summary>
    /// Typ ulepszenia określający jego kategorię.
    /// </summary>
    public UpgradeType Type;

    /// <summary>
    /// Aktualny poziom ulepszenia.
    /// </summary>
    public int Level = 0;

    /// <summary>
    /// Maksymalny możliwy poziom ulepszenia.
    /// </summary>
    public int MaxLevel = 5;

    /// <summary>
    /// Funkcja zawierająca efekt ulepszenia,
    /// wywoływana przy jego zastosowaniu na graczu.
    /// </summary>
    private Action<Player> ApplyEffect;

    /// <summary>
    /// Określa, czy ulepszenie może zostać jeszcze rozwinięte.
    /// </summary>
    public bool CanUpgrade => Level < MaxLevel;

    /// <summary>
    /// Tworzy nowe ulepszenie.
    /// </summary>
    /// <param name="name">Nazwa ulepszenia.</param>
    /// <param name="type">Typ ulepszenia.</param>
    /// <param name="applyEffect">
    /// Funkcja definiująca efekt ulepszenia (np. modyfikacja statystyk gracza).
    /// </param>
    /// <param name="maxLevel">Maksymalny poziom ulepszenia (domyślnie 5).</param>
    public UpgradeData(string name, UpgradeType type, Action<Player> applyEffect, int maxLevel = 5)
    {
        Name = name;
        Type = type;
        ApplyEffect = applyEffect;
        MaxLevel = maxLevel;
    }

    /// <summary>
    /// Aplikuje ulepszenie na podanego gracza.
    /// Jeśli ulepszenie nie osiągnęło maksymalnego poziomu,
    /// zwiększa jego poziom i wywołuje przypisany efekt.
    /// </summary>
    /// <param name="player">Obiekt gracza, na którym zostanie zastosowane ulepszenie.</param>
    public void Apply(Player player)
    {
        if (!CanUpgrade)
            return;

        Level++;

        // Wykonanie logiki ulepszenia (np. zwiększenie statystyk)
        ApplyEffect(player);
    }
}