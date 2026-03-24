using Godot;
[GlobalClass]

public partial class EnemyStats : Resource
{
	[Export] public string MobID = "slime";
	[Export] public string DisplayName = "Slime";
	[Export] public float Speed = 120f;
	[Export] public int MaxHealth = 30;
	[Export] public int XpDrop = 1;
	[Export] public int ContactDamage = 10;
	[Export] public float Scale = 1f;
	[Export] public PackedScene Scene;
}
