using System;

public enum UpgradeType
{
    Weapon,
    Passive,
    Stat
}

public class UpgradeData
{
    public string Name;
    public UpgradeType Type;

    public int Level = 0;
    public int MaxLevel = 5;

    private Action<Player> ApplyEffect;

    public bool CanUpgrade => Level < MaxLevel;

    public UpgradeData(string name, UpgradeType type, Action<Player> applyEffect, int maxLevel = 5)
    {
        Name = name;
        Type = type;
        ApplyEffect = applyEffect;
        MaxLevel = maxLevel;
    }

    public void Apply(Player player)
    {
        if (!CanUpgrade)
            return;

        Level++;
        ApplyEffect(player);
    }
}