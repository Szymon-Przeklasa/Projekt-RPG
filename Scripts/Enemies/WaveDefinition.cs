using Godot;

/// <summary>
/// Definicja pojedynczej fali przeciwników w grze.
/// Dziedziczy po Resource, dzięki czemu można tworzyć instancje w edytorze Godot (.tres/.res).
/// </summary>
[GlobalClass]
public partial class WaveDefinition : Resource
{
    /// <summary>
    /// Minuta gry, od której ta fala zaczyna się pojawiać.
    /// </summary>
    [Export] public float StartMinute = 0f;

    /// <summary>
    /// Minuta gry, po której ta fala przestaje się pojawiać (0 = nigdy nie kończy).
    /// </summary>
    [Export] public float EndMinute = 0f;

    /// <summary>
    /// Typ przeciwnika przypisany do tej fali (odwołanie do EnemyStats).
    /// </summary>
    [Export] public EnemyStats EnemyType;

    /// <summary>
    /// Bazowy interwał spawnu w sekundach między kolejnymi batchami przeciwników tej fali.
    /// </summary>
    [Export] public float BaseInterval = 2.0f;

    /// <summary>
    /// Liczba przeciwników spawnujących się naraz w jednym batchu.
    /// </summary>
    [Export] public int BatchSize = 1;
}