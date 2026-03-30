using Godot;

/// <summary>
/// Klasa reprezentująca broń typu Garlic.
/// Tworzy aurę wokół gracza, która zadaje obrażenia wszystkim wrogom w zasięgu.
/// Dziedziczy po klasie Weapon.
/// </summary>
public partial class Garlic : Weapon
{
    /// <summary>
    /// Referencja do instancji aury GarlicAura.
    /// Tworzona przy pierwszym wystrzale.
    /// </summary>
    private GarlicAura _aura;

    /// <summary>
    /// Metoda wywoływana przy strzale.
    /// Tworzy aurę, ustawia jej promień i zadaje obrażenia wszystkim wrogom w zasięgu.
    /// </summary>
    protected override void Fire()
    {
        if (_aura == null)
        {
            _aura = new GarlicAura();
            Player.AddChild(_aura);
            _aura.Position = Vector2.Zero;
        }

        // Aktualizuj promień aury przy każdym ticku
        _aura.Radius = GetRange();

        float radius = GetRange();
        foreach (Node node in GetTree().GetNodesInGroup("enemies"))
        {
            if (node is Enemy enemy &&
                Player.GlobalPosition.DistanceTo(enemy.GlobalPosition) <= radius)
            {
                enemy.TakeDamage(GetDamage(), Vector2.Zero, WeaponName);
            }
        }
    }
}

/// <summary>
/// Wizualna aura czosnku, rysowana wokół gracza.
/// Automatycznie skaluje się z promieniem i animuje puls.
/// Dziedziczy po Node2D i używa metody _Draw do rysowania.
/// </summary>
public partial class GarlicAura : Node2D
{
    /// <summary>
    /// Promień aury w jednostkach świata.
    /// </summary>
    public float Radius = 150f;

    /// <summary>
    /// Wewnętrzny licznik używany do animacji pulsowania.
    /// </summary>
    private float _pulse = 0f;

    /// <summary>
    /// Wywoływana co klatkę metoda, animuje puls aury i wymusza rysowanie.
    /// </summary>
    public override void _Process(double delta)
    {
        _pulse += (float)delta * 2.5f;
        QueueRedraw(); // odśwież rysowanie każdą klatkę
    }

    /// <summary>
    /// Rysuje aurę GarlicAura.
    /// Składa się z przezroczystego koła i dwóch łuków dla wizualnego efektu.
    /// </summary>
    public override void _Draw()
    {
        // Kompensuj skalę rodzica, aby promień odpowiadał jednostkom świata
        Vector2 globalScale = GlobalTransform.Scale;
        float scaleComp = 1f / Mathf.Max(globalScale.X, 0.001f);

        float displayRadius = (Radius + Mathf.Sin(_pulse) * 4f) * scaleComp;

        // Półprzezroczyste wypełnienie
        DrawCircle(Vector2.Zero, displayRadius, new Color(0.5f, 0f, 0.8f, 0.08f));

        // Główne obramowanie aury
        DrawArc(Vector2.Zero, displayRadius, 0f, Mathf.Tau, 64,
                new Color(0.7f, 0.1f, 1f, 0.7f), 2f);

        // Dodatkowy wewnętrzny łuk dla efektu pulsowania
        DrawArc(Vector2.Zero, displayRadius * 0.85f, 0f, Mathf.Tau, 48,
                new Color(0.6f, 0f, 0.9f, 0.25f), 1f);
    }
}