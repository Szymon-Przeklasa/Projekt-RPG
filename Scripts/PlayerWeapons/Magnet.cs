using Godot;

/// <summary>
/// Klasa reprezentująca broń typu Magnet.
/// Przyciąga obiekty typu XP (orb) znajdujące się w zasięgu gracza.
/// Dziedziczy po klasie Weapon.
/// </summary>
public partial class Magnet : Weapon
{
	/// <summary>
	/// Scena pocisku/efektu (nieużywana w tej broni, ale zachowana dla spójności z Weapon).
	/// </summary>
	[Export] PackedScene ProjectileScene;

	/// <summary>
	/// Metoda wywoływana przy strzale.
	/// Przesuwa wszystkie orb-y XP w zasięgu w kierunku punktu strzału gracza.
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
