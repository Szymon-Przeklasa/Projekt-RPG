using Godot;

/// <summary>
/// Klasa przechowująca statystyki broni.
/// Definiuje wszystkie parametry wpływające na zachowanie broni,
/// takie jak obrażenia, cooldown, zasięg czy liczba pocisków.
/// 
/// Dziedziczy po <see cref="Resource"/>, dzięki czemu może być
/// edytowana w Inspectorze Godot i wielokrotnie używana jako asset.
/// </summary>
[GlobalClass]
public partial class WeaponStats : Resource
{
    /// <summary>
    /// Bazowe obrażenia broni przed zastosowaniem modyfikatorów.
    /// Wykorzystywane jako punkt wyjściowy do skalowania obrażeń.
    /// </summary>
    [Export] public int BaseDamage = 5;

    /// <summary>
    /// Czas odnowienia broni (w sekundach).
    /// Określa odstęp między kolejnymi atakami.
    /// </summary>
    [Export] public float Cooldown = 1f;

    /// <summary>
    /// Końcowe obrażenia zadawane przez broń lub pocisk.
    /// </summary>
    [Export] public int Damage = 5;

    /// <summary>
    /// Prędkość poruszania się pocisku lub efektu broni.
    /// </summary>
    [Export] public float Speed = 600f;

    /// <summary>
    /// Liczba pocisków wystrzeliwanych jednocześnie przy ataku.
    /// </summary>
    [Export] public int ProjectileCount = 1;

    /// <summary>
    /// Liczba przeciwników, przez których pocisk może przelecieć (przebicie).
    /// </summary>
    [Export] public int Pierce = 1;

    /// <summary>
    /// Siła odrzutu (knockback) nakładana na trafionych przeciwników.
    /// </summary>
    [Export] public float Knockback = 200f;

    /// <summary>
    /// Maksymalny kąt rozrzutu pocisków (w stopniach).
    /// Określa losowe odchylenie trajektorii.
    /// </summary>
    [Export] public float SpreadAngle = 15f;

    /// <summary>
    /// Zasięg działania broni w jednostkach świata gry.
    /// </summary>
    [Export] public float Range = 500f;
}