using Godot;

/// <summary>
/// Broń Magic Missile.
/// Wystrzeliwuje pociski, które automatycznie naprowadzają się na najbliższego przeciwnika.
/// Skaluje się z globalnymi statystykami gracza (np. DamageMultiplier, CooldownMultiplier).
/// </summary>
public partial class MagicMissile : Weapon
{
    /// <summary>
    /// Scena pocisku Magic Missile.
    /// </summary>
    [Export] PackedScene ProjectileScene;

    /// <summary>
    /// Metoda wywoływana przy strzale.
    /// Tworzy określoną liczbę pocisków i przypisuje im cel.
    /// </summary>
    protected override void Fire()
    {
        for (int i = 0; i < Stats.ProjectileCount; i++)
        {
            // Znajdź najbliższego przeciwnika w zasięgu
            var enemy = Player.GetClosestEnemy(GetRange());
            if (enemy == null) return;

            // Utwórz pocisk
            var p = ProjectileScene.Instantiate<MagicMissileProjectile>();

            // Ustaw pozycję startową (punkt strzału gracza)
            p.GlobalPosition = Player.ShootPoint.GlobalPosition;

            // Inicjalizacja pocisku z uwzględnieniem statystyk
            p.Setup(
                (enemy.GlobalPosition - Player.GlobalPosition).Normalized(),
                Stats,
                GetDamage(),
                GetSpeed()
            );

            // Przypisanie celu do naprowadzania
            p.Target = enemy;

            // Dodanie do sceny
            GetTree().CurrentScene.AddChild(p);
        }
    }
}