using Godot;

/// <summary>
/// Klasa reprezentująca broń typu Magic Missile (Magiczny Pocisk).
/// Wystrzeliwuje samonaprowadzające się pociski (<see cref="MagicMissileProjectile"/>)
/// w kierunku najbliższych wrogów. Każdy pocisk otrzymuje przypisany cel
/// i automatycznie koryguje tor lotu, aby go dogonić.
/// Dziedziczy po klasie <see cref="Weapon"/>.
/// </summary>
public partial class MagicMissile : Weapon
{
    /// <summary>
    /// Scena pocisku (<see cref="MagicMissileProjectile"/>) używana przy każdym strzale.
    /// Musi być przypisana w inspektorze Godot.
    /// </summary>
    [Export] public PackedScene ProjectileScene;

    /// <summary>
    /// Metoda wywoływana przy każdym strzale.
    /// Pobiera listę najbliższych wrogów (z lekko powiększonym zasięgiem dla lepszego namierzania),
    /// a następnie dla każdego pocisku wyznacza cel, oblicza kierunek startowy z rozrzutem,
    /// tworzy instancję <see cref="MagicMissileProjectile"/> i przypisuje jej cel.
    /// Pocisk samodzielnie koryguje trajektorię podczas lotu.
    /// </summary>
    protected override void Fire()
    {
        if (ProjectileScene == null) return;

        int projectileCount = Mathf.Max(1, Stats.ProjectileCount);
        var targets = GetClosestEnemies(GetRange() * 1.15f, Mathf.Max(1, projectileCount), Player.ShootPoint.GlobalPosition);
        if (targets.Count == 0) return;

        for (int i = 0; i < projectileCount; i++)
        {
            var enemy = targets[i % targets.Count];
            Vector2 toTarget = (GetAimPosition(enemy) - Player.ShootPoint.GlobalPosition).Normalized();
            float angleOffset = GetCenteredOffset(i, projectileCount, 7f);
            Vector2 launchDirection = toTarget.Rotated(Mathf.DegToRad(angleOffset));
            Vector2 spawnOffset = launchDirection.Orthogonal() * GetCenteredOffset(i, projectileCount, 10f);
            var p = ProjectileScene.Instantiate<MagicMissileProjectile>();
            p.GlobalPosition = Player.ShootPoint.GlobalPosition + spawnOffset;
            p.Setup(
                launchDirection,
                Stats,
                GetDamage(),
                GetSpeed(),
                WeaponName
            );
            p.Target = enemy;
            GetTree().CurrentScene.AddChild(p);
        }
    }
}