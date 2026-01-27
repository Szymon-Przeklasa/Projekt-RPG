using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed = 150f;
    [Export] public int MaxHealth = 30;
    private int _health;

    private CharacterBody2D player;
	
	public override void _Ready()
	{
        _health = MaxHealth;
        player = GetNode<CharacterBody2D>("/root/Game/Player");
	}

    public void TakeDamage(int damage, Vector2 knockback)
    {
        _health -= damage;
        Velocity += knockback;

        if (_health <= 0)
            Die();
    }
    private void Die()
    {
        QueueFree();
    }


    public override void _PhysicsProcess(double delta)
	{
		
		if (player == null)
			return;

		Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
		Velocity = direction * Speed;
		
		if (GlobalPosition.DistanceTo(player.GlobalPosition) < 5f)
			Velocity = Vector2.Zero;

		MoveAndSlide();
	}
}
