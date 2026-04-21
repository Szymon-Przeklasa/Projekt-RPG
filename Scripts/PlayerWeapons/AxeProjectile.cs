using Godot;

public partial class AxeProjectile : Projectile
{
    private Vector2 _velocity;
    private bool _initialized;
    private const float GravityForce = 850f;
    private const float LiftFactor = 0.35f;
    private const float SpinSpeed = 10f;

    public override void _PhysicsProcess(double delta)
    {
        if (!_initialized)
        {
            _velocity = Direction * RuntimeSpeed;
            _velocity.Y -= RuntimeSpeed * LiftFactor;
            _initialized = true;
        }

        _velocity.Y += GravityForce * (float)delta;
        if (_velocity.LengthSquared() > 0.001f)
            Direction = _velocity.Normalized();

        Rotation += SpinSpeed * (float)delta;
        Advance(_velocity * (float)delta);
    }
}
