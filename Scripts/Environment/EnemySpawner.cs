using Godot;

/// <summary>
/// Klasa odpowiedzialna za generowanie przeciwników w grze.
/// Tworzy nowych wrogów wokó³ gracza w okreœlonym promieniu i z czasem skraca odstêp miêdzy spawnami.
/// </summary>
public partial class EnemySpawner : Node2D
{
    /// <summary>
    /// Scena przeciwnika do tworzenia instancji.
    /// </summary>
    [Export] public PackedScene EnemyScene;

    /// <summary>
    /// Promieñ wokó³ gracza, w którym przeciwnicy mog¹ siê pojawiaæ.
    /// </summary>
    [Export] public float SpawnRadius = 100f;

    /// <summary>
    /// Pocz¹tkowy odstêp miêdzy spawnami przeciwników (w sekundach).
    /// </summary>
    [Export] public float SpawnInterval = 1.2f;

    /// <summary>
    /// Minimalny mo¿liwy odstêp miêdzy spawnami.
    /// </summary>
    [Export] public float MinSpawnInterval = 0.05f;

    /// <summary>
    /// Wspó³czynnik zmniejszaj¹cy odstêp miêdzy spawnami po ka¿dym wrogu.
    /// </summary>
    [Export] public float SpawnDecayFactor = 0.95f;

    /// <summary>
    /// Referencja do gracza, wokó³ którego spawnuj¹ siê wrogowie.
    /// </summary>
    private Player player;

    /// <summary>
    /// Timer odpowiedzialny za wywo³ywanie spawnów przeciwników.
    /// </summary>
    private Timer timer;

    /// <summary>
    /// Metoda wywo³ywana po dodaniu wêz³a do drzewa sceny.
    /// Inicjalizuje gracza i timer.
    /// </summary>
    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("player") as Player;

        timer = GetNode<Timer>("SpawnTimer");
        timer.WaitTime = SpawnInterval;
        timer.Timeout += SpawnEnemy;
        timer.Start();
    }

    /// <summary>
    /// Tworzy nowego przeciwnika w losowej pozycji wokó³ gracza.
    /// Skraca odstêp miêdzy spawnami zgodnie ze wspó³czynnikiem SpawnDecayFactor.
    /// </summary>
    private void SpawnEnemy()
    {
        if (player == null) return;

        // Losowa pozycja w promieniu SpawnRadius
        Vector2 direction = Vector2.Right.Rotated(GD.Randf() * Mathf.Tau);
        Vector2 spawnPos = player.GlobalPosition + direction * SpawnRadius;

        // Tworzenie wroga
        var enemy = EnemyScene.Instantiate<Enemy>();
        enemy.GlobalPosition = spawnPos;
        GetTree().CurrentScene.AddChild(enemy);

        // Stopniowe zmniejszanie odstêpu miêdzy spawnami
        timer.WaitTime = Mathf.Max(MinSpawnInterval, timer.WaitTime * SpawnDecayFactor);
        timer.Start(); // restart timera z nowym odstêpem
    }
}