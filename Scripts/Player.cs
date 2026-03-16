using Godot;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

public partial class Player : CharacterBody2D
{
	[Export] public PackedScene ProjectileScene;
	[Export] public WeaponStats Weapon;
	[Export] public int Speed = 600;
	[Export] public int Xp = 0;
	[Export] public int XpToLevel = 10;
	[Export] public int Level = 1;

    public List<UpgradeData> AvailableUpgrades = new();

    // Global multipliers (used by weapons)
    public float DamageMultiplier = 1f;
    public float CooldownMultiplier = 1f;
    public float AreaMultiplier = 1f;
    public float SpeedMultiplier = 1f;

    // Weapon + passive limits
    public const int MAX_WEAPONS = 5;
    public const int MAX_PASSIVES = 5;

    public List<Weapon> Weapons = new();
    public List<UpgradeData> PassiveUpgrades = new();



    public Marker2D ShootPoint;


	public override void _Ready()
	{
		SetupUpgrades();
		GD.Print("Upgrades count: ", AvailableUpgrades.Count);

		ShootPoint = GetNode<Marker2D>("ShootPoint");

        foreach (Weapon weapon in GetNode("Weapons").GetChildren())
        {
            weapon.Init(this);
            Weapons.Add(weapon);
        }
    }

    public bool AddWeapon(PackedScene weaponScene)
    {
        if (Weapons.Count >= MAX_WEAPONS)
            return false;

        var weapon = weaponScene.Instantiate<Weapon>();

        GetNode("Weapons").AddChild(weapon);

        weapon.Init(this);

        Weapons.Add(weapon);

        return true;
    }

    public bool AddPassive(UpgradeData upgrade)
    {
        if (PassiveUpgrades.Count >= MAX_PASSIVES)
            return false;

        PassiveUpgrades.Add(upgrade);

        upgrade.Apply(this);

        return true;
    }

    public List<UpgradeData> GetUpgradeChoices(int count = 3)
    {
        List<UpgradeData> valid = new();

        foreach (var upgrade in AvailableUpgrades)
        {
            if (upgrade.CanUpgrade)
                valid.Add(upgrade);
        }

        Shuffle(valid);

        if (valid.Count > count)
            valid.RemoveRange(count, valid.Count - count);

        return valid;
    }

    public void Shuffle<T>(IList<T> list)
    {
        RandomNumberGenerator rng = new RandomNumberGenerator();

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.RandiRange(0, i);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void SetupUpgrades()
    {
        // lightning
        var lightning = GetNodeOrNull<Lightning>("Weapons/Lightning");

        if (lightning != null)
        {
            AvailableUpgrades.Add(new UpgradeData(
                "Lightning Damage +5",
                UpgradeType.Weapon,
                (p) => {
                    lightning.Stats.Damage += 5;
                },
                8
            ));

            AvailableUpgrades.Add(new UpgradeData(
                "Lightning Cooldown -0.1",
                UpgradeType.Weapon,
                (p) => {
                    lightning.Stats.Cooldown =
                        Mathf.Max(0.2f, lightning.Stats.Cooldown - 0.1f);

                    lightning.RefreshStats();
                },
                8
            ));

            AvailableUpgrades.Add(new UpgradeData(
                "Lightning +1 Projectile",
                UpgradeType.Weapon,
                (p) => {
                    lightning.Stats.ProjectileCount += 1;
                },
                8
            ));
        }


        // garlic
        var garlic = GetNodeOrNull<Garlic>("Weapons/Garlic");

        if (garlic != null)
        {
            AvailableUpgrades.Add(new UpgradeData(
                "Garlic Damage +2",
                UpgradeType.Weapon,
                (p) => {
                    garlic.Stats.Damage += 2;
                },
                8
            ));

            AvailableUpgrades.Add(new UpgradeData(
                "Garlic Range +20",
                UpgradeType.Weapon,
                (p) => {
                    garlic.Stats.Range += 20;
                },
                8
            ));
        }


        // global stat upgrade
        AvailableUpgrades.Add(new UpgradeData(
            "Damage +10%",
            UpgradeType.Stat,
            (p) => {
                p.DamageMultiplier += 0.1f;
            },
            5
        ));
    }





    public Node2D GetClosestEnemy(float range)
	{
		Node2D closest = null;
		float best = range;

		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Node2D enemy)
			{
				float d = GlobalPosition.DistanceTo(enemy.GlobalPosition);
				if (d < best)
				{
					best = d;
					closest = enemy;
				}
			}
		}
		return closest;
	}

	public void GainXp(int amount)
	{
		Xp += amount;

		if (Xp >= XpToLevel)
			LevelUp();
	}

	private void LevelUp()
	{
		Level++;
		Xp -= XpToLevel;
		XpToLevel = Mathf.RoundToInt(XpToLevel * 1.4f);

		GD.Print($"LEVEL UP! Level: {Level}");

		var ui = GetTree().CurrentScene.GetNode<LevelUpUI>("LevelUpUI");
		GD.Print(ui.GetType());
        var upgrades = GetUpgradeChoices(3);

        ui.ShowUpgrades(this);
    }

	public void GetInput()
	{
		Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");
		Velocity = inputDirection * Speed;
	}


	public override void _PhysicsProcess(double delta)
	{ 
		if (Input.IsActionJustPressed("pause"))
		{
			if (GetTree().Paused)
			{
                GetTree().Paused = false; // doesnt unpause somehow
            }
			else
			{
                GetTree().Paused = true;
            }
		}

		GetInput();
		MoveAndSlide();
	}
}
