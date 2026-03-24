using Godot;

/// <summary>
/// Jedna fala: od której minuty aktywna, jaki typ przeciwnika i co ile sekund spawnuje.
/// </summary>
[GlobalClass]
public partial class WaveDefinition : Resource
{
    /// <summary>Minuta gry, od której ta fala zaczyna się pojawiać.</summary>
    [Export] public float StartMinute = 0f;

    /// <summary>Minuta gry, po której ta fala przestaje się pojawiać (0 = nigdy).</summary>
    [Export] public float EndMinute = 0f;

    [Export] public EnemyStats EnemyType;

    /// <summary>Bazowy interwał spawnu w sekundach dla tej fali.</summary>
    [Export] public float BaseInterval = 2.0f;

    /// <summary>Ile wrogów tego typu spawnuje naraz.</summary>
    [Export] public int BatchSize = 1;
}