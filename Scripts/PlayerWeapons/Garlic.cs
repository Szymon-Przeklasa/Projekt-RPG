using Godot;

public partial class Garlic : Weapon
{
    private GarlicAura _aura;

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
/// Wizualny okrąg aury czosnku — rysuje się przez CanvasItem.Draw.
/// Automatycznie skaluje się z promieniem.
/// </summary>
public partial class GarlicAura : Node2D
{
    public float Radius = 150f;

    // Animacja pulsu
    private float _pulse = 0f;

    public override void _Process(double delta)
    {
        _pulse += (float)delta * 2.5f;
        QueueRedraw(); // odśwież rysowanie każdą klatkę
    }

    public override void _Draw()
    {
        // Pulsujący promień — ±4px
        float displayRadius = Radius + Mathf.Sin(_pulse) * 4f;

        // Wypełnienie — półprzezroczyste fioletowe
        DrawCircle(Vector2.Zero, displayRadius, new Color(0.5f, 0f, 0.8f, 0.08f));

        // Obwódka — bardziej widoczna
        DrawArc(
            center: Vector2.Zero,
            radius: displayRadius,
            startAngle: 0f,
            endAngle: Mathf.Tau,
            pointCount: 64,
            color: new Color(0.7f, 0.1f, 1f, 0.7f),
            width: 2f
        );

        // Drugi pierścień wewnętrzny dla głębi
        DrawArc(
            center: Vector2.Zero,
            radius: displayRadius * 0.85f,
            startAngle: 0f,
            endAngle: Mathf.Tau,
            pointCount: 48,
            color: new Color(0.6f, 0f, 0.9f, 0.25f),
            width: 1f
        );
    }
}