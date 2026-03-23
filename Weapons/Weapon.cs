using Godot;

/// <summary>
/// Klasa bazowa dla wszystkich broni.
/// Zarządza statystykami broni, czasem odnowienia oraz inicjalizacją.
/// Dziedziczą po niej klasy takie jak FireWand, Garlic, Lightning itp.
/// </summary>
public abstract partial class Weapon : Node
{
	/// <summary>
	/// Statystyki broni (obrażenia, zasięg, liczba pocisków, czas odnowienia itd.).
	/// </summary>
	[Export] public WeaponStats Stats;

	/// <summary>
	/// Odniesienie do gracza, który używa broni.
	/// </summary>
	protected Player Player;

	/// <summary>
	/// Timer odpowiadający za wywoływanie strzałów co określony czas.
	/// </summary>
	protected Timer timer;

	/// <summary>
	/// Inicjalizuje broń dla danego gracza.
	/// Ustawia timer na podstawie czasu odnowienia i mnożnika gracza.
	/// </summary>
	/// <param name="player">Gracz, który używa broni.</param>
	public virtual void Init(Player player)
	{
		Player = player;

		timer = new Timer();
		timer.WaitTime = Stats.Cooldown * Player.CooldownMultiplier;
		timer.OneShot = false;
		timer.Timeout += Fire;
		AddChild(timer);
		timer.Start();
	}

	/// <summary>
	/// Odświeża statystyki broni, aktualizując czas odnowienia w timerze.
	/// Powinno być wywoływane po zmianie Stats.Cooldown lub Player.CooldownMultiplier.
	/// </summary>
	public void RefreshStats()
	{
		if (timer != null)
			timer.WaitTime = Stats.Cooldown * Player.CooldownMultiplier;
	}

	/// <summary>
	/// Metoda abstrakcyjna wywoływana przy każdym strzale.
	/// Każda broń powinna nadpisać tę metodę, implementując własną logikę strzału.
	/// </summary>
	protected abstract void Fire();
}
