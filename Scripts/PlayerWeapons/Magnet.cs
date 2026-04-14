using Godot;

/// <summary>
/// Klasa reprezentująca broń typu Magnet.
/// Przyciąga XP orby w zasięgu gracza.
/// Ulepsza się przez zwiększenie zasięgu i prędkości przyciągania.
/// </summary>
public partial class Magnet : Weapon
{
	[Export] PackedScene ProjectileScene;

	/// <summary>Bazowa prędkość przyciągania (piksele/s).</summary>
	private float _basePullSpeed = 400f;

	/// <summary>Bonus do prędkości przyciągania z ulepszeń.</summary>
	public float PullSpeedBonus = 0f;

	protected override void Fire() { }

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