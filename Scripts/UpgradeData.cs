using System;
using System.Collections.Generic;

/// <summary>
/// Typ ulepszenia dostępnego dla gracza.
/// </summary>
public enum UpgradeType
{
    Weapon,
    Passive,
    Stat
}

/// <summary>
/// Pojedynczy poziom ulepszenia — opis i efekt aplikowany na gracza.
/// </summary>
public class UpgradeLevel
{
    /// <summary>
    /// Opis wyświetlany w UI dla tego poziomu.
    /// </summary>
    public string Description;

    /// <summary>
    /// Efekt aplikowany przy wyborze tego poziomu.
    /// </summary>
    public Action<Player> Effect;

    public UpgradeLevel(string description, Action<Player> effect)
    {
        Description = description;
        Effect = effect;
    }
}

/// <summary>
/// Klasa reprezentująca ulepszenie broni/pasywki w stylu Vampire Survivors.
/// Każdy poziom ma własny opis i efekt — gracz wybiera BROŃ do ulepszenia,
/// a system automatycznie aplikuje efekt odpowiedni dla obecnego poziomu.
/// </summary>
public class UpgradeData
{
    /// <summary>Nazwa ulepszenia wyświetlana w UI (np. "Garlic").</summary>
    public string Name;

    /// <summary>Typ ulepszenia.</summary>
    public UpgradeType Type;

    /// <summary>Aktualny poziom (0 = nie ulepszone).</summary>
    public int Level = 0;

    /// <summary>Lista poziomów z opisami i efektami.</summary>
    public List<UpgradeLevel> Levels = new();

    /// <summary>Maksymalny poziom wynikający z liczby zdefiniowanych poziomów.</summary>
    public int MaxLevel => Levels.Count;

    /// <summary>Czy można jeszcze ulepszyć.</summary>
    public bool CanUpgrade => Level < MaxLevel;

    /// <summary>Opis następnego poziomu do wyświetlenia w UI.</summary>
    public string NextLevelDescription => CanUpgrade ? Levels[Level].Description : "MAX";

    public UpgradeData(string name, UpgradeType type)
    {
        Name = name;
        Type = type;
    }

    /// <summary>
    /// Dodaje kolejny poziom ulepszenia.
    /// </summary>
    public UpgradeData AddLevel(string description, Action<Player> effect)
    {
        Levels.Add(new UpgradeLevel(description, effect));
        return this; // fluent API
    }

    /// <summary>
    /// Aplikuje efekt bieżącego poziomu i zwiększa licznik.
    /// </summary>
    public void Apply(Player player)
    {
        if (!CanUpgrade) return;

        Levels[Level].Effect(player);
        Level++;
    }
}