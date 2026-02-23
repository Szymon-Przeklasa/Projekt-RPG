using Godot;

public partial class LightningChain : GpuParticles2D
{
	private Vector2 _from;
	private Vector2 _to;

	public void Setup(Vector2 from, Vector2 to)
	{
		_from = from;
		_to = to;
	}

	public override void _Ready()
	{
		Animate();
	}

	private async void Animate()
	{
		int explosions = 10;
		float explosionInterval = (float)Lifetime / explosions;

		var mat = (ParticleProcessMaterial)ProcessMaterial;
		mat.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Sphere;
		mat.EmissionSphereRadius = 5f;

		Emitting = false;

		float elapsed = 0f;

		while (elapsed < Lifetime)
		{
			float t = elapsed / (float)Lifetime;
			GlobalPosition = _from.Lerp(_to, t);

			if (Mathf.IsZeroApprox(elapsed % explosionInterval))
			{
				Restart();
				Emitting = true;
				await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
				Emitting = false;
			}

			await ToSignal(GetTree().CreateTimer(0.016f), "timeout"); // ~60 FPS
			elapsed += 0.016f;
		}

		QueueFree();
	}
}
