using Godot;
using System.Collections.Generic;

/// <summary>
/// Klasa bazowa dla wszystkich broni w grze.
/// Odpowiada za zarządzanie statystykami, timerem ataku oraz
/// dostarcza wspólne funkcje pomocnicze dla klas dziedziczących
/// (np. FireWand, Garlic, Lightning).
/// </summary>
public abstract partial class Weapon : Node
{
    /// <summary>
    /// Statystyki broni (obrażenia, cooldown, zasięg, liczba pocisków itd.).
    /// </summary>
    [Export] public WeaponStats Stats;

    /// <summary>
    /// Referencja do gracza, który korzysta z broni.
    /// </summary>
    protected Player Player;

    /// <summary>
    /// Timer odpowiedzialny za cykliczne wywoływanie ataku.
    /// </summary>
    protected Timer timer;

    /// <summary>
    /// Nazwa broni (domyślnie nazwa klasy).
    /// </summary>
    public virtual string WeaponName => GetType().Name;

    /// <summary>
    /// Inicjalizuje broń i podłącza timer ataku.
    /// Ustawia czas odnowienia na podstawie statystyk i modyfikatorów gracza.
    /// </summary>
    /// <param name="player">Gracz używający broni.</param>
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
    /// Aktualizuje parametry broni po zmianie statystyk lub modyfikatorów gracza.
    /// W szczególności odświeża czas odnowienia ataku.
    /// </summary>
    public virtual void RefreshStats()
    {
        if (timer != null)
            timer.WaitTime = Mathf.Max(0.1f, Stats.Cooldown * Player.CooldownMultiplier);
    }

    /// <summary>
    /// Zwraca aktualne obrażenia broni z uwzględnieniem mnożnika gracza.
    /// </summary>
    protected int GetDamage() =>
        Mathf.RoundToInt(Stats.Damage * Player.DamageMultiplier);

    /// <summary>
    /// Zwraca aktualny zasięg broni z uwzględnieniem bonusów gracza.
    /// </summary>
    protected float GetRange() =>
        Stats.Range * Player.AreaMultiplier;

    /// <summary>
    /// Zwraca aktualną prędkość pocisków z uwzględnieniem modyfikatorów gracza.
    /// </summary>
    protected float GetSpeed() =>
        Stats.Speed * Player.ProjectileSpeedMultiplier;

    /// <summary>
    /// Zwraca pozycję trafienia dla przeciwnika.
    /// Preferuje marker "Center", jeśli istnieje.
    /// </summary>
    protected Vector2 GetAimPosition(Node2D target)
    {
        if (target == null)
            return Vector2.Zero;

        var center = target.GetNodeOrNull<Marker2D>("Center");
        return center != null ? center.GlobalPosition : target.GlobalPosition;
    }

    /// <summary>
    /// Zwraca listę najbliższych przeciwników w zadanym zasięgu.
    /// Wynik jest posortowany według odległości od punktu odniesienia.
    /// </summary>
    /// <param name="range">Zasięg wyszukiwania.</param>
    /// <param name="count">Maksymalna liczba przeciwników.</param>
    /// <param name="fromPosition">Punkt odniesienia (domyślnie gracz).</param>
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
            origin.DistanceSquaredTo(GetAimPosition(a))
                .CompareTo(origin.DistanceSquaredTo(GetAimPosition(b))));

        if (count > 0 && candidates.Count > count)
            candidates.RemoveRange(count, candidates.Count - count);

        return candidates;
    }

    /// <summary>
    /// Oblicza wycentrowane przesunięcie dla rozrzutu pocisków.
    /// </summary>
    /// <param name="index">Indeks pocisku.</param>
    /// <param name="total">Liczba pocisków.</param>
    /// <param name="step">Kątowy krok rozrzutu.</param>
    protected float GetCenteredOffset(int index, int total, float step)
    {
        if (total <= 1)
            return 0f;

        return (index - (total - 1) * 0.5f) * step;
    }

    /// <summary>
    /// Metoda abstrakcyjna wywoływana przy każdym ataku.
    /// Każda broń musi implementować własną logikę strzału.
    /// </summary>
    protected abstract void Fire();
}