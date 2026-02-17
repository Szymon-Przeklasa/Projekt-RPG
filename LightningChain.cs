using Godot;

public partial class LightningChain : GpuParticles2D
{
	public void Setup(Vector2 from, Vector2 to)
	{
		GlobalPosition = from + (to - from);

		Vector2 dir = to - from;
		Rotation = dir.Angle();

		var mat = (ParticleProcessMaterial)ProcessMaterial;

		mat.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box;
		mat.EmissionBoxExtents = new Vector3(dir.Length(), 1, 0);

		Emitting = true;
	}
}
