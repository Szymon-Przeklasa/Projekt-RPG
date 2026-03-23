using Godot;

[GlobalClass]
public partial class PassiveData : Resource
{
    [Export] public string Name = "";
    [Export] public string Description = "";
    [Export] public PassiveType Type;
    [Export] public int MaxLevel = 5;
    [Export] public float BonusPerLevel = 0.1f;

    public int CurrentLevel = 0;

    public bool CanUpgrade => CurrentLevel < MaxLevel;

    /// <summary>
    /// Aplikuje efekt pasywny na gracza i odświeża bronie.
    /// </summary>
    public void Apply(Player player)
    {
        CurrentLevel++;
        float bonus = BonusPerLevel;

        switch (Type)
        {
            case PassiveType.Spinach:
                player.DamageMultiplier += bonus;
                break;
            case PassiveType.Pummarola:
                player.CooldownMultiplier = Mathf.Max(0.2f, player.CooldownMultiplier - bonus);
                break;
            case PassiveType.HollowHeart:
                player.AreaMultiplier += bonus;
                break;
            case PassiveType.Bracer:
                player.ProjectileSpeedMultiplier += bonus;
                break;
            case PassiveType.Wings:
                player.SpeedMultiplier += bonus;
                break;
        }

        // Odśwież wszystkie bronie po zmianie statystyk
        player.RefreshAllWeapons();
    }
}