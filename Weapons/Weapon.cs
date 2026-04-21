using Godot;
using System.Collections.Generic;

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

	public virtual string WeaponName => GetType().Name;

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
	public virtual void RefreshStats()
	{
		if (timer != null)
			timer.WaitTime = Mathf.Max(0.1f, Stats.Cooldown * Player.CooldownMultiplier);
	}

	/// <summary>
	/// Oblicza aktualne obrażenia z uwzględnieniem mnożnika gracza.
	/// </summary>
	protected int GetDamage() => Mathf.RoundToInt(Stats.Damage * Player.DamageMultiplier);

	/// <summary>
	/// Oblicza aktualny zasięg z uwzględnieniem mnożnika obszaru gracza.
	/// </summary>
	protected float GetRange() => Stats.Range * Player.AreaMultiplier;

	/// <summary>
	/// Oblicza aktualną prędkość pocisków z uwzględnieniem mnożnika.
	/// </summary>
	protected float GetSpeed() => Stats.Speed * Player.ProjectileSpeedMultiplier;

	/// <summary>
	/// Zwraca pozycję celu dla przeciwnika. Używa markera Center jeśli istnieje.
	/// </summary>
	protected Vector2 GetAimPosition(Node2D target)
	{
		if (target == null)
			return Vector2.Zero;

		var center = target.GetNodeOrNull<Marker2D>("Center");
		return center != null ? center.GlobalPosition : target.GlobalPosition;
	}

	/// <summary>
	/// Zwraca kilku najbliższych przeciwników w zasięgu, posortowanych po dystansie.
	/// </summary>
	protected List<Node2D> GetClosestEnemies(float range, int count, Vector2? fromPosition = null)
	{
		var origin = fromPosition ?? Player.GlobalPosition;
		var candidates = new List<Node2D>();

		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is not Node2D enemy)
				continue;

			if (origin.DistanceTo(GetAimPosition(enemy)) <= range)
				candidates.Add(enemy);
		}

		candidates.Sort((a, b) =>
			origin.DistanceSquaredTo(GetAimPosition(a)).CompareTo(origin.DistanceSquaredTo(GetAimPosition(b))));

		if (count > 0 && candidates.Count > count)
			candidates.RemoveRange(count, candidates.Count - count);

		return candidates;
	}

	/// <summary>
	/// Zwraca wycentrowane przesunięcie dla wachlarza pocisków.
	/// </summary>
	protected float GetCenteredOffset(int index, int total, float step)
	{
		if (total <= 1)
			return 0f;

		return (index - (total - 1) * 0.5f) * step;
	}

	/// <summary>
	/// Metoda abstrakcyjna wywoływana przy każdym strzale.
	/// Każda broń powinna nadpisać tę metodę, implementując własną logikę strzału.
	/// </summary>
	protected abstract void Fire();
}
