using Godot;
using System.Collections.Generic;

/// <summary>
/// Klasa reprezentująca broń typu Lightning (Piorun).
/// Atakuje najbliższego wroga i skacze łańcuchowo między kolejnymi celami.
/// Liczba skoków określona jest przez <see cref="WeaponStats.ProjectileCount"/>.
/// Dziedziczy po klasie <see cref="Weapon"/>.
/// </summary>
public partial class Lightning : Weapon
{
	/// <summary>
	/// Scena efektu wizualnego pioruna (<see cref="LightningBeam"/>).
	/// Musi być przypisana w inspektorze Godot.
	/// </summary>
	[Export] public PackedScene ProjectileScene;

	/// <summary>
	/// Metoda wywoływana przy każdym strzale.
	/// Wyszukuje najbliższego wroga w zasięgu, a następnie skacze łańcuchowo
	/// między kolejnymi celami, zadając obrażenia i tworząc efekty wizualne.
	/// Każdy trafiony wróg jest dodawany do zestawu <c>hitEnemies</c>,
	/// aby uniknąć wielokrotnego trafienia tego samego celu.
	/// </summary>
	protected override void Fire()
	{
		var enemies = GetTree().GetNodesInGroup("enemies");
		if (enemies.Count == 0) return;

		float range = GetRange();
		int chainsLeft = Stats.ProjectileCount;
		Node2D current = Player.GetClosestEnemy(range);
		if (current == null) return;

		var hitEnemies = new HashSet<Node2D>();
		Vector2 fromPosition = Player.ShootPoint.GlobalPosition;

		while (current != null && chainsLeft-- > 0)
		{
			if (hitEnemies.Contains(current)) break;

			hitEnemies.Add(current);
			Vector2 toPosition = GetAimPosition(current);

			((Enemy)current).TakeDamage(GetDamage(), Vector2.Zero, WeaponName);

			SpawnLightningFX(fromPosition, toPosition);
			fromPosition = toPosition;
			current = GetClosestUnhitEnemy(toPosition, hitEnemies, range);
		}
	}

	/// <summary>
	/// Wyszukuje najbliższego wroga, który nie został jeszcze trafiony w tej serii.
	/// Przeszukuje wszystkich wrogów w grupie "enemies" i filtruje tych już w <paramref name="hitEnemies"/>.
	/// </summary>
	/// <param name="fromPos">Pozycja, od której mierzony jest dystans do kolejnych celów.</param>
	/// <param name="hitEnemies">Zbiór wrogów już trafionych w bieżącej serii skoków.</param>
	/// <param name="range">Maksymalny zasięg skoku pioruna.</param>
	/// <returns>Najbliższy niepodbity wróg lub <c>null</c>, jeśli brak kandydatów w zasięgu.</returns>
	Node2D GetClosestUnhitEnemy(Vector2 fromPos, HashSet<Node2D> hitEnemies, float range)
	{
		Node2D closest = null;
		float closestDist = float.MaxValue;

		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Node2D enemy && !hitEnemies.Contains(enemy))
			{
				float dist = fromPos.DistanceTo(GetAimPosition(enemy));
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
	/// Tworzy wizualny efekt pioruna (<see cref="LightningBeam"/>) między dwoma punktami.
	/// Efekt jest dodawany bezpośrednio do bieżącej sceny i usuwa się samoczynnie po animacji.
	/// </summary>
	/// <param name="from">Punkt startowy efektu (np. poprzedni cel lub gracz).</param>
	/// <param name="to">Punkt końcowy efektu (aktualny cel).</param>
	void SpawnLightningFX(Vector2 from, Vector2 to)
	{
		if (ProjectileScene == null) return;
		var beam = ProjectileScene.Instantiate<LightningBeam>();
		GetTree().CurrentScene.AddChild(beam);
		beam.Setup(from, to);
	}
}
