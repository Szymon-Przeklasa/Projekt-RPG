using System;

/// <summary>
/// Typ ulepszenia dostępnego dla gracza.
/// </summary>
public enum UpgradeType
{
    /// <summary>
    /// Ulepszenie związane z bronią.
    /// </summary>
    Weapon,

    /// <summary>
    /// Ulepszenie pasywne.
    /// </summary>
    Passive,

    /// <summary>
    /// Ulepszenie globalnej statystyki gracza (np. DamageMultiplier, SpeedMultiplier).
    /// </summary>
    Stat
}

/// <summary>
/// Klasa reprezentująca pojedyncze ulepszenie dla gracza.
/// Zawiera nazwę, typ, poziom, maksymalny poziom oraz efekt do zastosowania.
/// </summary>
public class UpgradeData
{
    /// <summary>
    /// Nazwa ulepszenia.
    /// </summary>
    public string Name;

    /// <summary>
    /// Typ ulepszenia.
    /// </summary>
    public UpgradeType Type;

    /// <summary>
    /// Aktualny poziom ulepszenia.
    /// </summary>
    public int Level = 0;

    /// <summary>
    /// Maksymalny poziom ulepszenia.
    /// </summary>
    public int MaxLevel = 5;

    /// <summary>
    /// Funkcja wywoływana po zastosowaniu ulepszenia na gracza.
    /// </summary>
    private Action<Player> ApplyEffect;

    /// <summary>
    /// Zwraca true, jeśli ulepszenie może być jeszcze podniesione (Level &lt; MaxLevel).
    /// </summary>
    public bool CanUpgrade => Level < MaxLevel;

    /// <summary>
    /// Konstruktor klasy UpgradeData.
    /// </summary>
    /// <param name="name">Nazwa ulepszenia.</param>
    /// <param name="type">Typ ulepszenia.</param>
    /// <param name="applyEffect">Funkcja wywoływana przy zastosowaniu ulepszenia na gracza.</param>
    /// <param name="maxLevel">Maksymalny poziom ulepszenia (domyślnie 5).</param>
    public UpgradeData(string name, UpgradeType type, Action<Player> applyEffect, int maxLevel = 5)
    {
        Name = name;
        Type = type;
        ApplyEffect = applyEffect;
        MaxLevel = maxLevel;
    }

    /// <summary>
    /// Zastosowuje ulepszenie na gracza.
    /// Zwiększa poziom i wywołuje przypisaną funkcję ApplyEffect, jeśli ulepszenie może być podniesione.
    /// </summary>
    /// <param name="player">Gracz, na którym zostanie zastosowane ulepszenie.</param>
    public void Apply(Player player)
    {
        if (!CanUpgrade)
            return;

        Level++;
        ApplyEffect(player);
    }
}