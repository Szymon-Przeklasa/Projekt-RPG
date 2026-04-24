using Godot;

/// <summary>
/// Miniatura mapy wyświetlana w UI gry jako SubViewport.
/// Śledzi pozycję gracza przez przesuwanie wewnętrznej kamery.
/// Używa tego samego <see cref="World2D"/> co główna scena, dzięki czemu
/// renderuje obiekty ze sceny gry bez ich duplikowania.
/// </summary>
public partial class MiniMap : SubViewport
{
    /// <summary>
    /// Referencja do gracza pobierana z grupy "player".
    /// Kamera minimapy śledzi jego pozycję.
    /// </summary>
    private CharacterBody2D _player;

    /// <summary>
    /// Wewnętrzna kamera minimapy przesuwana co klatkę fizyki do pozycji gracza.
    /// </summary>
    private Camera2D _camera;

    /// <summary>
    /// Inicjalizacja po dodaniu do sceny.
    /// Pobiera gracza i kamerę, a następnie ustawia <see cref="SubViewport.World2D"/>
    /// na współdzielony świat głównej sceny.
    /// </summary>
    public override void _Ready()
    {
        _player = GetTree().GetFirstNodeInGroup("player") as CharacterBody2D;
        _camera = GetNode<Camera2D>("Camera2D");

        var sceneViewport = GetTree().CurrentScene?.GetViewport();
        if (sceneViewport != null)
            World2D = sceneViewport.World2D;
    }

    /// <summary>
    /// Aktualizacja fizyki: przesuwa kamerę minimapy do aktualnej pozycji gracza.
    /// Jeśli gracz nie jest jeszcze dostępny, próbuje go pobrać ponownie z drzewa sceny.
    /// </summary>
    /// <param name="delta">Czas od poprzedniej klatki fizyki (sekundy).</param>
    public override void _PhysicsProcess(double delta)
    {
        if (_player == null)
            _player = GetTree().GetFirstNodeInGroup("player") as CharacterBody2D;

        if (_player == null || _camera == null)
            return;

        _camera.GlobalPosition = _player.GlobalPosition;
    }
}