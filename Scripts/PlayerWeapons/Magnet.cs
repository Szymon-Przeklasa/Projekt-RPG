using Godot;

/// <summary>
/// Klasa reprezentująca broń typu Magnet.
/// Zamiast zadawać obrażenia, przyciąga punkty doświadczenia (XP orby)
/// znajdujące się w zasięgu gracza.
/// </summary>
public partial class Magnet : Weapon
{
	/// <summary>
	/// Scena pocisku/efektu.
	/// Nie jest używana w tej broni, ale pozostaje dla spójności z klasą bazową Weapon.
	/// </summary>
	[Export] PackedScene ProjectileScene;

	/// <summary>
	/// Metoda wywoływana przy aktywacji broni (strzale).
	/// Wyszukuje wszystkie orb-y XP i przyciąga te znajdujące się w zasięgu gracza.
	/// </summary>
	protected override void Fire()
	{
		foreach (Node node in GetTree().GetNodesInGroup("xp"))
		{
			// Sprawdzenie, czy node jest orbem (Node2D)
			if (node is Node2D orb &&
				Player.GlobalPosition.DistanceTo(orb.GlobalPosition) <= Stats.Range)
			{
				// Płynne przesunięcie orba w stronę punktu strzału gracza
				orb.GlobalPosition =
					orb.GlobalPosition.Lerp(
						Player.GetNode<Marker2D>("ShootPoint").GlobalPosition,
						0.15f
					);
			}
		}
	}
}