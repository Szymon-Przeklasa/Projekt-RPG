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
				//GD.Print($"Garlic radius: {radius}, Stats.Range: {Stats.Range}, AreaMultiplier: {Player.AreaMultiplier}");
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
		// Kompensuj skalę rodzica żeby okrąg odpowiadał world-space radius
		Vector2 globalScale = GlobalTransform.Scale;
		float scaleComp = 1f / Mathf.Max(globalScale.X, 0.001f);

		float displayRadius = (Radius + Mathf.Sin(_pulse) * 4f) * scaleComp;

		DrawCircle(Vector2.Zero, displayRadius, new Color(0.5f, 0f, 0.8f, 0.08f));
		DrawArc(Vector2.Zero, displayRadius, 0f, Mathf.Tau, 64,
				new Color(0.7f, 0.1f, 1f, 0.7f), 2f);
		DrawArc(Vector2.Zero, displayRadius * 0.85f, 0f, Mathf.Tau, 48,
				new Color(0.6f, 0f, 0.9f, 0.25f), 1f);
	}
}
