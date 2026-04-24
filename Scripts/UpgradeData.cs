using System;
using System.Collections.Generic;

/// <summary>
/// Typ ulepszenia dostępnego dla gracza.
/// Określa, czy dany upgrade dotyczy broni, pasywki czy statystyk.
/// </summary>
public enum UpgradeType
{
    Weapon,
    Passive,
    Stat
}

/// <summary>
/// Pojedynczy poziom ulepszenia.
/// Zawiera opis oraz efekt, który zostaje zastosowany na graczu.
/// </summary>
public class UpgradeLevel
{
    /// <summary>
    /// Opis poziomu wyświetlany w interfejsie użytkownika.
    /// </summary>
    public string Description;

    /// <summary>
    /// Funkcja wywoływana przy aktywacji tego poziomu ulepszenia.
    /// </summary>
    public Action<Player> Effect;

    /// <summary>
    /// Tworzy nowy poziom ulepszenia.
    /// </summary>
    /// <param name="description">Opis poziomu.</param>
    /// <param name="effect">Efekt aplikowany na gracza.</param>
    public UpgradeLevel(string description, Action<Player> effect)
    {
        Description = description;
        Effect = effect;
    }
}

/// <summary>
/// Reprezentuje dane ulepszenia (broń, pasywka lub statystyka).
///
/// Każde ulepszenie składa się z wielu poziomów, z których każdy
/// posiada własny opis i efekt.
///
/// System działa w stylu Vampire Survivors:
/// gracz wybiera ulepszenie, a system automatycznie aplikuje
/// odpowiedni poziom.
/// </summary>
public class UpgradeData
{
    /// <summary>Nazwa ulepszenia (np. "Garlic").</summary>
    public string Name;

    /// <summary>Typ ulepszenia (Weapon / Passive / Stat).</summary>
    public UpgradeType Type;

    /// <summary>Aktualny poziom ulepszenia (0 = brak ulepszenia).</summary>
    public int Level = 0;

    /// <summary>Lista wszystkich poziomów ulepszenia.</summary>
    public List<UpgradeLevel> Levels = new();

    /// <summary>Maksymalny poziom ulepszenia.</summary>
    public int MaxLevel => Levels.Count;

    /// <summary>
    /// Określa czy ulepszenie może być jeszcze rozwijane.
    /// </summary>
    public bool CanUpgrade => Level < MaxLevel;

    /// <summary>
    /// Opis następnego poziomu ulepszenia.
    /// Jeśli osiągnięto maksymalny poziom, zwraca "MAX".
    /// </summary>
    public string NextLevelDescription =>
        CanUpgrade ? Levels[Level].Description : "MAX";

    /// <summary>
    /// Tworzy nowe ulepszenie o podanej nazwie i typie.
    /// </summary>
    /// <param name="name">Nazwa ulepszenia.</param>
    /// <param name="type">Typ ulepszenia.</param>
    public UpgradeData(string name, UpgradeType type)
    {
        Name = name;
        Type = type;
    }

    /// <summary>
    /// Dodaje nowy poziom ulepszenia.
    /// Umożliwia budowanie systemu w stylu fluent API.
    /// </summary>
    /// <param name="description">Opis poziomu.</param>
    /// <param name="effect">Efekt aplikowany na gracza.</param>
    /// <returns>Referencja do obiektu UpgradeData.</returns>
    public UpgradeData AddLevel(string description, Action<Player> effect)
    {
        Levels.Add(new UpgradeLevel(description, effect));
        return this;
    }

    /// <summary>
    /// Aplikuje efekt aktualnego poziomu ulepszenia i zwiększa poziom.
    /// Jeśli osiągnięto maksymalny poziom, metoda nie wykonuje żadnej akcji.
    /// </summary>
    /// <param name="player">Gracz, na którego działa ulepszenie.</param>
    public void Apply(Player player)
    {
        if (!CanUpgrade)
            return;

        Levels[Level].Effect(player);
        Level++;
    }
}