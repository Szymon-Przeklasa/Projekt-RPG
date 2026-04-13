using Godot;

/// <summary>
/// Klasa reprezentująca broń typu Magnet.
/// Zamiast zadawać obrażenia, przyciąga punkty doświadczenia (XP orby)
/// znajdujące się w zasięgu gracza.
/// </summary>
public partial class Magnet : Weapon
{
    [Export] PackedScene ProjectileScene;

    /// <summary>
    /// Metoda wywoływana przy aktywacji broni.
    /// Przy bardzo krótkim cooldownie (0.01s) lerp był nieregularny —
    /// przyciąganie przeniesiono do _PhysicsProcess dla płynności.
    /// Fire() pozostaje puste, logika działa co klatkę.
    /// </summary>
    protected override void Fire() { }

    /// <summary>
    /// Płynne przyciąganie XP orbów co klatkę fizyki.
    /// Działa niezależnie od timera broni.
    /// </summary>
    public override void _PhysicsProcess(double delta)
    {
        if (Player == null) return;

        float range = Stats != null ? Stats.Range * Player.AreaMultiplier : 150f;

        // Prędkość przyciągania w pikselach/s — im bliżej, tym szybciej
        float pullSpeed = 400f;

        foreach (Node node in GetTree().GetNodesInGroup("xp"))
        {
            if (node is Node2D orb)
            {
                float dist = Player.GlobalPosition.DistanceTo(orb.GlobalPosition);
                if (dist > range) continue;

                // Znormalizowany kierunek do gracza
                Vector2 dir = (Player.ShootPoint.GlobalPosition - orb.GlobalPosition).Normalized();

                // Ruch tym szybszy im orb jest bliżej centrum (opcjonalnie płaski)
                orb.GlobalPosition += dir * pullSpeed * (float)delta;
            }
        }
    }
}