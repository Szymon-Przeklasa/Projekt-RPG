using Godot;
using System;

/// <summary>
/// Klasa reprezentująca przeciwnika (Enemy) w grze.
/// Obsługuje ruch w kierunku gracza, otrzymywanie obrażeń, śmierć i drop doświadczenia.
/// Dziedziczy po CharacterBody2D.
/// </summary>
public partial class Enemy : CharacterBody2D
{
    // -- konfiguracja -----------------------------------------------------------------
    
    /// <summary>Statystyki wroga przechowywane w EnemyStats.</summary>
    [Export] public EnemyStats Stats;

    /// <summary>Scena orb-u XP, który zostaje upuszczony po śmierci wroga.</summary>
    [Export] public PackedScene XpOrbScene;

    /// <summary>Scena cząsteczek przy trafieniu wroga.</summary>
    [Export] public PackedScene HitParticle;

    // -- stan -------------------------------------------------------------------------

    private int _health;           // Aktualne zdrowie wroga
    private Player _player;        // Referencja do gracza
    private float _contactCooldown; // Czas do ponownego zadania obrażeń kontaktowych

    /// <summary>Prędkość wroga, może być nadpisana przez Stats.</summary>
    [Export] public float Speed = 140f;

    /// <summary>Maksymalne zdrowie wroga, nadpisywane przez Stats.</summary>
    [Export] public int MaxHealth = 100;

    /// <summary>Ilość doświadczenia przyznawana po zabiciu wroga.</summary>
    [Export] public int XpDrop = 1;

    /// <summary>ID wroga, np. "goblin", "skeleton".</summary>
    [Export] public string MobID = "enemy";

    /// <summary>
    /// Metoda wywoływana po dodaniu wroga do sceny.
    /// Inicjalizuje statystyki, zdrowie i skalę wroga.
    /// </summary>
    public override void _Ready()
    {
        if (Stats != null)
        {
            Speed = Stats.Speed;
            MaxHealth = Stats.MaxHealth;
            XpDrop = Stats.XpDrop;
            MobID = Stats.MobID;
            Scale = new Vector2(Stats.Scale, Stats.Scale);
        }
        _health = MaxHealth;
    }

    // -----------------------------------------------------------------
    
    /// <summary>
    /// Zadaje obrażenia wrogowi.
    /// Tworzy efekt trafienia i wyświetla etykietę obrażeń.
    /// </summary>
    /// <param name="damage">Ilość obrażeń.</param>
    /// <param name="knockback">Wektor odrzutu.</param>
    /// <param name="weaponName">Nazwa źródła obrażeń.</param>
    public void TakeDamage(int damage, Vector2 knockback, string weaponName = "unknown")
    {
        _health -= damage;
        Velocity += knockback;

        if (HitParticle != null)
        {
            var fx = HitParticle.Instantiate<Enemybleed>();
            AddChild(fx);
            fx.Position = Vector2.Zero;
            fx.Emitting = true;
        }

        // Wyświetlenie pływającej etykiety obrażeń
        SpawnDamageLabel(damage, weaponName);

        if (_health <= 0)
            Die();
    }

    /// <summary>
    /// Tworzy pływającą etykietę pokazującą zadane obrażenia.
    /// </summary>
    private void SpawnDamageLabel(int damage, string weaponName)
    {
        var label = new Label
        {
            Text = $"{damage} [{weaponName}]",
            ZIndex = 10,
            Position = new Vector2(-30f, -60f)
        };

        // Styl tekstu
        label.AddThemeColorOverride("font_color", Colors.Yellow);
        label.AddThemeFontSizeOverride("font_size", 12);

        AddChild(label);

        // Animacja: unoszenie i zanikanie
        var tween = CreateTween();
        tween.TweenProperty(label, "position", label.Position + new Vector2(0, -40f), 0.8f);
        tween.Parallel().TweenProperty(label, "modulate:a", 0f, 0.8f);
        tween.TweenCallback(Callable.From(() => label.QueueFree()));
    }

    /// <summary>
    /// Obsługuje śmierć wroga: rejestruje zabójstwo i upuszcza XP.
    /// </summary>
    private void Die()
    {
        GetNode<KillManager>("/root/KillManager").RegisterKill(MobID);
        DropXp();
        QueueFree();
    }

    /// <summary>
    /// Tworzy orb XP w miejscu śmierci wroga.
    /// </summary>
    private void DropXp()
    {
        if (XpOrbScene == null) return;

        var orb = XpOrbScene.Instantiate<XpOrb>();
        orb.GlobalPosition = GlobalPosition;
        orb.Value = XpDrop;

        GetTree().CurrentScene.CallDeferred(Node.MethodName.AddChild, orb);
    }

    /// <summary>
    /// Metoda fizyczna wywoływana co klatkę.
    /// Obsługuje ruch w kierunku gracza i zadawanie obrażeń kontaktowych.
    /// </summary>
    public override void _PhysicsProcess(double delta)
    {
        if (_player == null)
            _player = GetTree().GetFirstNodeInGroup("player") as Player;

        if (_player != null)
        {
            Vector2 dir = (_player.GlobalPosition - GlobalPosition).Normalized();
            Velocity += dir * Speed * (float)delta;
        }

        Velocity = Velocity.Lerp(Vector2.Zero, 0.05f);
        MoveAndSlide();

        HandleContactDamage(delta);
    }

    /// <summary>
    /// Obsługuje obrażenia kontaktowe, gdy wróg dotyka gracza.
    /// </summary>
    private void HandleContactDamage(double delta)
    {
        _contactCooldown -= (float)delta;
        if (_contactCooldown > 0) return;

        if (_player == null) return;

        float dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
        if (dist > 40f) return;

        int dmg = Stats != null ? Stats.ContactDamage : 1;
        //_player.TakeDamage(dmg);
        _contactCooldown = 0.5f;
    }
}