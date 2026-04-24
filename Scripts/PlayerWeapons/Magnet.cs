using Godot;

/// <summary>
/// Klasa reprezentująca broń typu Magnet (Magnes).
/// Pasywnie przyciąga orby doświadczenia (XP) w zasięgu gracza każdą klatkę fizyki.
/// Prędkość i zasięg przyciągania skalują się przez mnożniki gracza oraz ulepszenia pasywne.
/// Broń nie posiada aktywnego strzału — metoda <see cref="Fire"/> jest pusta.
/// Dziedziczy po klasie <see cref="Weapon"/>.
/// </summary>
public partial class Magnet : Weapon
{
    /// <summary>
    /// Nieużywana scena pocisku — wymagana przez interfejs eksportu Godot,
    /// ale Magnet nie wystrzeliwuje pocisków.
    /// </summary>
    [Export] PackedScene ProjectileScene;

    /// <summary>
    /// Bazowa prędkość przyciągania orbów XP (piksele/s) przed uwzględnieniem mnożników.
    /// </summary>
    private float _basePullSpeed = 525f;

    /// <summary>
    /// Dodatkowy bonus do prędkości przyciągania, dodawany przez ulepszenia pasywne magnesu.
    /// Kumuluje się z każdym zakupionym ulepszeniem.
    /// </summary>
    public float PullSpeedBonus = 0f;

    /// <summary>
    /// Metoda strzału — celowo pusta, ponieważ Magnet działa pasywnie przez <see cref="_PhysicsProcess"/>.
    /// </summary>
    protected override void Fire() { }

    /// <summary>
    /// Aktualizacja fizyki wywoływana każdą klatką.
    /// Dla każdego orba XP w grupie "xp" oblicza dystans do gracza.
    /// Jeśli orb jest w zasięgu, przesuwa go w kierunku gracza z prędkością
    /// skalowaną przez bliskość do centrum (<c>speedFactor</c>).
    /// Im bliżej środka, tym orb jest szybciej wciągany.
    /// </summary>
    /// <param name="delta">Czas od poprzedniej klatki fizyki (sekundy).</param>
    public override void _PhysicsProcess(double delta)
    {
        if (Player == null) return;

        float range = Stats != null ? Stats.Range * Player.AreaMultiplier : 150f;
        float pullSpeed = (_basePullSpeed + PullSpeedBonus) * Player.ProjectileSpeedMultiplier;

        foreach (Node node in GetTree().GetNodesInGroup("xp"))
        {
            if (node is Node2D orb)
            {
                float dist = Player.GlobalPosition.DistanceTo(orb.GlobalPosition);
                if (dist > range) continue;

                // Tym szybciej ciągnie, im bliżej środka
                float speedFactor = 1f + (1f - dist / range) * 0.5f;
                Vector2 dir = (Player.ShootPoint.GlobalPosition - orb.GlobalPosition).Normalized();
                orb.GlobalPosition += dir * pullSpeed * speedFactor * (float)delta;
            }
        }
    }
}