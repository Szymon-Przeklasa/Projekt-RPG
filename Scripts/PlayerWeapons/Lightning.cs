using Godot;
using System.Collections.Generic;

/// <summary>
/// Klasa reprezentująca broń typu Lightning.
/// Strzela piorunem, który przeskakuje między wrogami do określonej liczby razy.
/// Dziedziczy po klasie Weapon.
/// </summary>
public partial class Lightning : Weapon
{
    /// <summary>
    /// Scena efektu pioruna (LightningBeam), instancjonowana przy strzale.
    /// </summary>
    [Export] PackedScene ProjectileScene;

    /// <summary>
    /// Wywoływana przy strzale metoda Fire.
    /// Tworzy efekt pioruna od gracza do najbliższego wroga,
    /// przeskakując między kolejnymi wrogami aż do limitu łańcuchów.
    /// Każdy trafiony wróg otrzymuje obrażenia.
    /// </summary>
    protected override void Fire()
    {
        var enemies = GetTree().GetNodesInGroup("enemies");
        if (enemies.Count == 0) return;

        float range = GetRange();
        int chainsLeft = Stats.ProjectileCount;
        Node2D current = Player.GetClosestEnemy(range);
        if (current == null) return;

        var hitEnemies = new HashSet<Node2D>();
        Vector2 fromPosition = Player.ShootPoint.GlobalPosition;

        while (current != null && chainsLeft-- > 0)
        {
            if (hitEnemies.Contains(current)) break;

            hitEnemies.Add(current);
            var center = current.GetNode<Marker2D>("Center");
            Vector2 toPosition = center.GlobalPosition;

            ((Enemy)current).TakeDamage(GetDamage(), Vector2.Zero, WeaponName);

            SpawnLightningFX(fromPosition, toPosition);
            fromPosition = toPosition;
            current = GetClosestUnhitEnemy(toPosition, hitEnemies, range);
        }
    }

    /// <summary>
    /// Znajduje najbliższego wroga od danej pozycji, który nie został jeszcze trafiony.
    /// </summary>
    /// <param name="fromPos">Pozycja startowa poszukiwania wroga.</param>
    /// <param name="hitEnemies">Zbiór wrogów już trafionych piorunem.</param>
    /// <param name="range">Maksymalny zasięg wyszukiwania wroga.</param>
    /// <returns>Najbliższy nie trafiony wróg typu Node2D lub null.</returns>
    Node2D GetClosestUnhitEnemy(Vector2 fromPos, HashSet<Node2D> hitEnemies, float range)
    {
        Node2D closest = null;
        float closestDist = float.MaxValue;

        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Node2D enemy && !hitEnemies.Contains(enemy))
            {
                var center = enemy.GetNode<Marker2D>("Center");
                float dist = fromPos.DistanceTo(center.GlobalPosition);
                if (dist < closestDist && dist <= range)
                {
                    closestDist = dist;
                    closest = enemy;
                }
            }
        }
        return closest;
    }

    /// <summary>
    /// Tworzy wizualny efekt pioruna między dwiema pozycjami.
    /// </summary>
    /// <param name="from">Pozycja startowa pioruna.</param>
    /// <param name="to">Pozycja końcowa pioruna.</param>
    void SpawnLightningFX(Vector2 from, Vector2 to)
    {
        var beam = ProjectileScene.Instantiate<LightningBeam>();
        GetTree().CurrentScene.AddChild(beam);
        beam.Setup(from, to);
    }
}