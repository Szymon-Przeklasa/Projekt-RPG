using Godot;

/// <summary>
/// Klasa reprezentująca statystyki broni.
/// Zawiera wszystkie parametry potrzebne do konfiguracji działania broni, takie jak obrażenia, zasięg czy liczba pocisków.
/// Dziedziczy po Resource, dzięki czemu można ją eksportować i edytować w Godot.
/// </summary>
[GlobalClass]
public partial class WeaponStats : Resource
{
    /// <summary>
    /// Bazowe obrażenia przed mnożnikami. Używane do obliczeń skalowania.
    /// </summary>
    [Export] public int BaseDamage = 5;

    /// <summary>
    /// Czas odnowienia broni (w sekundach) między kolejnymi strzałami.
    /// </summary>
    [Export] public float Cooldown = 1f;

	/// <summary>
	/// Obrażenia zadawane przez pojedynczy pocisk lub trafienie.
	/// </summary>
	[Export] public int Damage = 5;

	/// <summary>
	/// Prędkość pocisku lub efektu broni.
	/// </summary>
	[Export] public float Speed = 600f;

	/// <summary>
	/// Liczba pocisków wystrzeliwanych przy jednym strzale.
	/// </summary>
	[Export] public int ProjectileCount = 1;

	/// <summary>
	/// Liczba wrogów, przez których pocisk może przejść (przebicie).
	/// </summary>
	[Export] public int Pierce = 1;

	/// <summary>
	/// Siła odrzutu wywołanego przez pocisk.
	/// </summary>
	[Export] public float Knockback = 200f;

	/// <summary>
	/// Maksymalny kąt rozrzutu pocisków (w stopniach).
	/// </summary>
	[Export] public float SpreadAngle = 15f;

	/// <summary>
	/// Zasięg broni w jednostkach sceny Godot.
	/// </summary>
	[Export] public float Range = 500f;
}
