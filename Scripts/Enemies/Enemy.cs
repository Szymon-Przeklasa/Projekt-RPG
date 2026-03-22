using Godot;
using System;

/// <summary>
/// Klasa reprezentująca przeciwnika w grze.
/// Obsługuje ruch w kierunku gracza, otrzymywanie obrażeń, śmierć i drop doświadczenia.
/// </summary>
public partial class Enemy : CharacterBody2D
{
    /// <summary>
    /// Prędkość poruszania się przeciwnika.
    /// </summary>
    [Export] public float Speed = 140f;

    /// <summary>
    /// Maksymalna ilość punktów życia przeciwnika.
    /// </summary>
    [Export] public int MaxHealth = 100;

    /// <summary>
    /// Scena pocisku doświadczenia (XP), którą przeciwnik zrzuca po śmierci.
    /// </summary>
    [Export] public PackedScene XpOrbScene;

    /// <summary>
    /// Ilość doświadczenia, które przeciwnik zostawia po śmierci.
    /// </summary>
    [Export] public int XpDrop = 1;

    /// <summary>
    /// Efekt cząsteczkowy wywoływany przy otrzymaniu obrażeń.
    /// </summary>
    [Export] public PackedScene HitParticle;

    /// <summary>
    /// Unikalny identyfikator przeciwnika (np. nazwa w KillManager).
    /// </summary>
    [Export] public string MobID = "Red slime";

    /// <summary>
    /// Aktualna ilość punktów życia przeciwnika.
    /// </summary>
    private int _health;

    /// <summary>
    /// Referencja do gracza, do którego przeciwnik się porusza.
    /// </summary>
    Player player;

    /// <summary>
    /// Metoda wywoływana po dodaniu węzła do drzewa sceny.
    /// Inicjalizuje punkty życia przeciwnika.
    /// </summary>
    public override void _Ready()
    {
        _health = MaxHealth;
    }

    /// <summary>
    /// Zadaje obrażenia przeciwnikowi i aplikuje knockback.
    /// Tworzy efekt cząsteczkowy przy trafieniu i obsługuje śmierć przeciwnika.
    /// </summary>
    /// <param name="damage">Ilość zadanych obrażeń.</param>
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
            // śmierć przeciwnika
            GetNode<KillManager>("/root/KillManager").RegisterKill(MobID);
            DropXp();
            QueueFree();
        }
    }

    /// <summary>
    /// Tworzy orb doświadczenia w miejscu śmierci przeciwnika.
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
    /// Metoda fizyczna wywoływana co klatkę.
    /// Obsługuje ruch przeciwnika w kierunku gracza oraz spowalnianie Velocity.
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