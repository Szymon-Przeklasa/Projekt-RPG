using Godot;

public abstract partial class Weapon : Node
{
    [Export] public WeaponStats Stats;
    protected Player Player;
    protected Timer timer;

    public virtual void Init(Player player)
    {
        Player = player;

        timer = new Timer();
        timer.WaitTime = Stats.Cooldown;
        timer.OneShot = false;
        timer.Timeout += Fire;
        AddChild(timer);
        timer.Start();
    }

    public void RefreshStats()
    {
        if (timer != null)
            timer.WaitTime = Stats.Cooldown;
    }

    protected abstract void Fire();
}
