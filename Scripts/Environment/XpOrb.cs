using Godot;

/// <summary>
/// Klasa reprezentuj¹ca orb doœwiadczenia (XP) w grze.
/// Po kontakcie z graczem przyznaje mu punkty doœwiadczenia i usuwa siê z gry.
/// </summary>
public partial class XpOrb : Area2D
{
    /// <summary>
    /// Iloœæ punktów doœwiadczenia przyznawanych graczowi po zebraniu orb.
    /// </summary>
    [Export] public int Value = 1;

    /// <summary>
    /// Metoda wywo³ywana po dodaniu wêz³a do drzewa sceny.
    /// Subskrybuje zdarzenie BodyEntered dla wykrywania kolizji z graczem.
    /// </summary>
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    /// <summary>
    /// Wywo³ywana po wejœciu innego wêz³a w obszar orb.
    /// Je¿eli to gracz, przyznaje mu punkty doœwiadczenia i usuwa orb.
    /// </summary>
    /// <param name="body">Wêze³, który wszed³ w obszar orb.</param>
    private void OnBodyEntered(Node body)
    {
        if (body is Player player)
        {
            player.GainXp(Value);
            QueueFree();
        }
    }
}