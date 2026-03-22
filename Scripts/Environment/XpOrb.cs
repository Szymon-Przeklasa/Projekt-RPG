using Godot;

/// <summary>
/// Klasa reprezentująca orb doświadczenia (XP) w grze.
/// Po kontakcie z graczem przyznaje mu punkty doświadczenia i usuwa się z gry.
/// </summary>
public partial class XpOrb : Area2D
{
    /// <summary>
    /// Ilość punktów doświadczenia przyznawanych graczowi po zebraniu orb.
    /// </summary>
    [Export] public int Value = 1;

    /// <summary>
    /// Metoda wywoływana po dodaniu węzła do drzewa sceny.
    /// Subskrybuje zdarzenie BodyEntered dla wykrywania kolizji z graczem.
    /// </summary>
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    /// <summary>
    /// Wywoływana po wejściu innego węzła w obszar orb.
    /// Jeżeli to gracz, przyznaje mu punkty doświadczenia i usuwa orb.
    /// </summary>
    /// <param name="body">Węzeł, który wszedł w obszar orb.</param>
    private void OnBodyEntered(Node body)
    {
        if (body is Player player)
        {
            player.GainXp(Value);
            QueueFree();
        }
    }
}