using Godot;

[GlobalClass]
public partial class WeaponStats : Resource
{
    [Export] public float Cooldown = 1f;
    [Export] public int Damage = 5;
    [Export] public float Speed = 600f;
    [Export] public int ProjectileCount = 1;
    [Export] public int Pierce = 1;
    [Export] public float Knockback = 200f;
    [Export] public float SpreadAngle = 15f;
    [Export] public float Range = 500f;
}
