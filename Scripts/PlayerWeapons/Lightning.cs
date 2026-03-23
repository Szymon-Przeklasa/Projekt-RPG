using Godot;
using System.Collections.Generic;

/// <summary>
/// Klasa reprezentująca broń typu Lightning.
/// Strzela piorunem, który przeskakuje między najbliższymi wrogami do określonej liczby razy.
/// Dziedziczy po klasie Weapon.
/// </summary>
public partial class Lightning : Weapon
{
	/// <summary>
	/// Scena efektu pioruna (LightningBeam), która będzie instancjonowana przy strzale.
	/// </summary>
	[Export] PackedScene ProjectileScene;

	/// <summary>
	/// Metoda wywoływana przy strzale.
	/// Tworzy efekt pioruna od gracza do najbliższego wroga, przeskakując między kolejnymi wrogami
	/// aż do osiągnięcia limitu łańcuchów (Stats.ProjectileCount).
	/// Każdy trafiony wróg otrzymuje obrażenia obliczone na podstawie Stats.Damage i Player.DamageMultiplier.
	/// </summary>
	protected override void Fire()
	{
		var enemies = GetTree().GetNodesInGroup("enemies");
		if (enemies.Count == 0) return;

		float range = GetRange(); // Wings pośrednio przez SpeedMultiplier wpływa na gameplay,
								  // ale zasięg skalujemy przez AreaMultiplier
		int chainsLeft = Stats.ProjectileCount;
		Node2D current = Player.GetClosestEnemy(range);
		if (current == null) return;

		var hitEnemies = new HashSet<Node2D>();
		Vector2 fromPosition = Player.ShootPoint.GlobalPosition;

		while (current != null && chainsLeft-- > 0)
		{
			if (hitEnemies.Contains(current)) break;

			hitEnemies.Add(current);
			var center = current.GetNode<Marker2D>("Center");
			Vector2 toPosition = center.GlobalPosition;

			((Enemy)current).TakeDamage(GetDamage(), Vector2.Zero); // <-- GetDamage()

			SpawnLightningFX(fromPosition, toPosition);
			fromPosition = toPosition;
			current = GetClosestUnhitEnemy(toPosition, hitEnemies, range);
		}
	}

	Node2D GetClosestUnhitEnemy(Vector2 fromPos, HashSet<Node2D> hitEnemies, float range)
	{
		Node2D closest = null;
		float closestDist = float.MaxValue;

		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Node2D enemy && !hitEnemies.Contains(enemy))
			{
				var center = enemy.GetNode<Marker2D>("Center");
				float dist = fromPos.DistanceTo(center.GlobalPosition);
				if (dist < closestDist && dist <= range)
				{
					closestDist = dist;
					closest = enemy;
				}
			}
		}
		return closest;
	}

	/// <summary>
	/// Zwraca najbliższego wroga od danej pozycji, który nie został jeszcze trafiony.
	/// </summary>
	/// <param name="fromPos">Pozycja, od której szukamy najbliższego wroga.</param>
	/// <param name="hitEnemies">Zbiór wrogów, którzy już zostali trafieni piorunem.</param>
	/// <returns>Najbliższy nie trafiony wróg typu Node2D lub null, jeśli żaden nie pasuje.</returns>
	Node2D GetClosestUnhitEnemy(Vector2 fromPos, HashSet<Node2D> hitEnemies)
	{
		Node2D closest = null;
		float closestDist = float.MaxValue;

		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Node2D enemy && !hitEnemies.Contains(enemy))
			{
				var center = enemy.GetNode<Marker2D>("Center");
				float dist = fromPos.DistanceTo(center.GlobalPosition);

				if (dist < closestDist && dist <= Stats.Range)
				{
					closestDist = dist;
					closest = enemy;
				}
			}
		}

		return closest;
	}

	/// <summary>
	/// Tworzy wizualny efekt pioruna między dwiema pozycjami.
	/// </summary>
	/// <param name="from">Pozycja początkowa pioruna.</param>
	/// <param name="to">Pozycja końcowa pioruna.</param>
	void SpawnLightningFX(Vector2 from, Vector2 to)
	{
		var beam = ProjectileScene.Instantiate<LightningBeam>();
		GetTree().CurrentScene.AddChild(beam);
		beam.Setup(from, to);
	}
}
