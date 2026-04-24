using Godot;
using System;

/// <summary>
/// Główna klasa sceny gry, zarządzająca odliczaniem czasu rundy i warunkiem zwycięstwa.
/// Timer jest uruchamiany automatycznie po załadowaniu sceny i zatrzymywany po pauzie lub śmierci gracza.
/// Po osiągnięciu czasu <see cref="WinTime"/> wywoływana jest metoda <see cref="OnWin"/>,
/// która dezaktywuje spawner wrogów i wyświetla ekran wyników (<see cref="WinScreen"/>).
/// </summary>
public partial class Game : Node2D
{
    /// <summary>
    /// Etykieta UI wyświetlająca aktualny czas rozgrywki w formacie MM:SS.
    /// </summary>
    private Label _timerLabel;

    /// <summary>
    /// Całkowity czas (w sekundach), który upłynął od początku rundy.
    /// </summary>
    private double _elapsedTime = 0;

    /// <summary>
    /// Flaga wskazująca, czy timer jest aktywnie odliczany.
    /// Ustawiany na <c>false</c> po pauzie lub zakończeniu gry.
    /// </summary>
    private bool _timerRunning = false;

    /// <summary>
    /// Flaga wskazująca, czy gra dobiegła końca (wygraną lub przgraną).
    /// Zapobiega wielokrotnemu wywołaniu <see cref="OnWin"/>.
    /// </summary>
    private bool _gameOver = false;

    /// <summary>
    /// Referencja do ekranu wyników wyświetlanego po zwycięstwie gracza.
    /// Tworzony dynamicznie w <see cref="_Ready"/>.
    /// </summary>
    private WinScreen _winScreen;

    /// <summary>
    /// Czas trwania rundy w sekundach, po którym gracz wygrywa (16 minut).
    /// </summary>
    private const double WinTime = 16 * 60;

    /// <summary>
    /// Metoda wywoływana po dodaniu węzła do drzewa sceny.
    /// Pobiera etykietę timera, uruchamia odliczanie i tworzy instancję <see cref="WinScreen"/>.
    /// </summary>
    public override void _Ready()
    {
        _timerLabel = GetNode<Label>("CanvasLayer/Timer");
        _timerRunning = true;

        _winScreen = new WinScreen();
        AddChild(_winScreen);
    }

    /// <summary>
    /// Metoda wywoływana każdą klatkę logiki.
    /// Aktualizuje licznik czasu i etykietę UI.
    /// Jeśli gra jest pauzowana lub zakończona, metoda wraca natychmiast.
    /// Po osiągnięciu <see cref="WinTime"/> wywołuje <see cref="OnWin"/>.
    /// </summary>
    /// <param name="delta">Czas od poprzedniej klatki (sekundy).</param>
    public override void _Process(double delta)
    {
        if (!_timerRunning || GetTree().Paused || _gameOver) return;

        _elapsedTime += delta;

        double displayTime = Math.Min(_elapsedTime, WinTime);
        int minutes = (int)displayTime / 60;
        int seconds = (int)displayTime % 60;
        _timerLabel.Text = $"{minutes:00}:{seconds:00}";

        if (_elapsedTime >= WinTime)
        {
            _gameOver = true;
            OnWin();
        }
    }

    /// <summary>
    /// Obsługuje zwycięstwo gracza po przetrwaniu pełnego czasu rundy.
    /// Zatrzymuje timer, dezaktywuje <see cref="EnemySpawner"/> i wyświetla
    /// ekran wyników z poziomem gracza i czasem przetrwania.
    /// </summary>
    private void OnWin()
    {
        _timerRunning = false;

        var spawner = GetTree().CurrentScene.GetNodeOrNull<EnemySpawner>("EnemySpawner");
        if (spawner != null) spawner.ProcessMode = ProcessModeEnum.Disabled;

        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        int playerLevel = player?.Level ?? 1;

        _winScreen.ShowResults(playerLevel, _elapsedTime);
    }

    /// <summary>
    /// Zatrzymuje timer gry.
    /// Wywoływana zewnętrznie, np. przez ekran śmierci lub inne systemy kończące rundę.
    /// </summary>
    public void StopTimer() => _timerRunning = false;
}