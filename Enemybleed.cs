using Godot;
using System;

public partial class Enemybleed : GpuParticles2D
{
	public void Setup(Vector2 position)
	{
		GlobalPosition = position;
		Emitting = true;
	}
}
