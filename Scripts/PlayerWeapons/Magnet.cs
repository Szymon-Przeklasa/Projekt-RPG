using Godot;

/// <summary>
/// Klasa reprezentuj¹ca broñ typu Magnet.
/// Przyci¹ga obiekty typu XP (orb) znajduj¹ce siê w zasiêgu gracza.
/// Dziedziczy po klasie Weapon.
/// </summary>
public partial class Magnet : Weapon
{
    /// <summary>
    /// Scena pocisku/efektu (nieu¿ywana w tej broni, ale zachowana dla spójnoœci z Weapon).
    /// </summary>
    [Export] PackedScene ProjectileScene;

    /// <summary>
    /// Metoda wywo³ywana przy strzale.
    /// Przesuwa wszystkie orb-y XP w zasiêgu w kierunku punktu strza³u gracza.
    /// </summary>
    protected override void Fire()
    {
        foreach (Node node in GetTree().GetNodesInGroup("xp"))
        {
            if (node is Node2D orb &&
                Player.GlobalPosition.DistanceTo(orb.GlobalPosition) <= Stats.Range)
            {
                orb.GlobalPosition =
                    orb.GlobalPosition.Lerp(Player.GetNode<Marker2D>("ShootPoint").GlobalPosition, 0.15f);
            }
        }
    }
}