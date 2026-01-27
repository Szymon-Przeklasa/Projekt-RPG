using Godot;

public abstract partial class Weapon : Node
{
	[Export] public WeaponStats Stats;
	protected Player Player;

	public virtual void Init(Player player)
	{
		Player = player;

		Timer timer = new Timer();
		timer.WaitTime = Stats.Cooldown;
		timer.Timeout += Fire;
		AddChild(timer);
		timer.Start();
	}

	protected abstract void Fire();
}
