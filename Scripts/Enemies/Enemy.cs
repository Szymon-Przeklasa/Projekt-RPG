using Godot;
using System;

/// <summary>
/// Klasa reprezentuj¹ca przeciwnika w grze.
/// Obs³uguje ruch w kierunku gracza, otrzymywanie obra¿eñ, œmieræ i drop doœwiadczenia.
/// </summary>
public partial class Enemy : CharacterBody2D
{
    /// <summary>
    /// Prêdkoœæ poruszania siê przeciwnika.
    /// </summary>
    [Export] public float Speed = 140f;

    /// <summary>
    /// Maksymalna iloœæ punktów ¿ycia przeciwnika.
    /// </summary>
    [Export] public int MaxHealth = 100;

    /// <summary>
    /// Scena pocisku doœwiadczenia (XP), któr¹ przeciwnik zrzuca po œmierci.
    /// </summary>
    [Export] public PackedScene XpOrbScene;

    /// <summary>
    /// Iloœæ doœwiadczenia, które przeciwnik zostawia po œmierci.
    /// </summary>
    [Export] public int XpDrop = 1;

    /// <summary>
    /// Efekt cz¹steczkowy wywo³ywany przy otrzymaniu obra¿eñ.
    /// </summary>
    [Export] public PackedScene HitParticle;

    /// <summary>
    /// Unikalny identyfikator przeciwnika (np. nazwa w KillManager).
    /// </summary>
    [Export] public string MobID = "Red slime";

    /// <summary>
    /// Aktualna iloœæ punktów ¿ycia przeciwnika.
    /// </summary>
    private int _health;

    /// <summary>
    /// Referencja do gracza, do którego przeciwnik siê porusza.
    /// </summary>
    Player player;

    /// <summary>
    /// Metoda wywo³ywana po dodaniu wêz³a do drzewa sceny.
    /// Inicjalizuje punkty ¿ycia przeciwnika.
    /// </summary>
    public override void _Ready()
    {
        _health = MaxHealth;
    }

    /// <summary>
    /// Zadaje obra¿enia przeciwnikowi i aplikuje knockback.
    /// Tworzy efekt cz¹steczkowy przy trafieniu i obs³uguje œmieræ przeciwnika.
    /// </summary>
    /// <param name="damage">Iloœæ zadanych obra¿eñ.</param>
    /// <param name="knockback">Wektor knockbacku, który zostanie dodany do Velocity.</param>
    public void TakeDamage(int damage, Vector2 knockback)
    {
        _health -= damage;
        Velocity += knockback;

        if (HitParticle != null)
        {
            var fx = HitParticle.Instantiate<Enemybleed>();
            AddChild(fx);               // attach to enemy
            fx.Position = Vector2.Zero; // local position
            fx.Emitting = true;
        }

        if (_health <= 0)
        {
            // œmieræ przeciwnika
            GetNode<KillManager>("/root/KillManager").RegisterKill(MobID);
            DropXp();
            QueueFree();
        }
    }

    /// <summary>
    /// Tworzy orb doœwiadczenia w miejscu œmierci przeciwnika.
    /// </summary>
    private void DropXp()
    {
        if (XpOrbScene == null)
            return;

        var orb = XpOrbScene.Instantiate<XpOrb>();
        orb.GlobalPosition = GlobalPosition;
        orb.Value = XpDrop;

        GetTree().CurrentScene.CallDeferred(Node.MethodName.AddChild, orb);
    }

    /// <summary>
    /// Metoda fizyczna wywo³ywana co klatkê.
    /// Obs³uguje ruch przeciwnika w kierunku gracza oraz spowalnianie Velocity.
    /// </summary>
    /// <param name="delta">Czas od ostatniej klatki.</param>
    public override void _PhysicsProcess(double delta)
    {
        if (player == null)
            player = GetTree().GetFirstNodeInGroup("player") as Player;

        if (player != null)
        {
            Vector2 dir = (player.GlobalPosition - GlobalPosition).Normalized();
            Velocity += dir * Speed * (float)delta;
        }

        Velocity = Velocity.Lerp(Vector2.Zero, 0.05f);
        MoveAndSlide();
    }
}