using Godot;
using System;

/// <summary>
/// Klasa reprezentująca broń typu Garlic.
/// Tworzy aurę wokół gracza i zadaje obrażenia wszystkim wrogom znajdującym się w zasięgu.
/// Dziedziczy po klasie Weapon.
/// </summary>
public partial class Garlic : Weapon
{
	/// <summary>
	/// Węzeł reprezentujący aurę działającą wokół gracza.
	/// </summary>
	private Node2D aura;

	/// <summary>
	/// Scena pocisku lub efektu wizualnego, która będzie instancjonowana przy użyciu broni.
	/// </summary>
	[Export] PackedScene ProjectileScene;

	/// <summary>
	/// Metoda wywoływana przy strzale.
	/// Tworzy aurę wokół gracza, jeśli jeszcze nie istnieje, i zadaje obrażenia wszystkim wrogom
	/// znajdującym się w połowie zasięgu statystyki broni.
	/// </summary>
	protected override void Fire()
	{
		if (aura == null)
		{
			aura = ProjectileScene.Instantiate<GpuParticles2D>();
			Player.AddChild(aura);
			aura.Position = Vector2.Zero;
		}

		float radius = GetRange(); // <-- teraz używa AreaMultiplier!

		foreach (Node node in GetTree().GetNodesInGroup("enemies"))
		{
			if (node is Enemy enemy &&
				Player.GlobalPosition.DistanceTo(enemy.GlobalPosition) <= radius)
			{
				enemy.TakeDamage(GetDamage(), Vector2.Zero);
			}
		}
	}
}
