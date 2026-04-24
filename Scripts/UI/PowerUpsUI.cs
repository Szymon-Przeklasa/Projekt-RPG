using Godot;

/// <summary>
/// Interfejs użytkownika wyświetlający podgląd dostępnych power-upów
/// z poziomu menu głównego.
///
/// Panel może zostać zamknięty:
/// <list type="bullet">
/// <item><description>przyciskiem zamknięcia,</description></item>
/// <item><description>klawiszem anulowania,</description></item>
/// <item><description>kliknięciem w tło panelu.</description></item>
/// </list>
///
/// Klasa dziedziczy po <see cref="CanvasLayer"/>.
/// </summary>
public partial class PowerUpsUI : CanvasLayer
{
    /// <summary>
    /// Przycisk zamykający panel.
    /// </summary>
    private Button _closeButton;

    /// <summary>
    /// Inicjalizuje panel po dodaniu do sceny.
    /// Ustawia początkową niewidoczność interfejsu
    /// oraz podłącza zdarzenie zamknięcia.
    /// </summary>
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;

        _closeButton = GetNode<Button>("Panel/VBoxContainer/CloseButton");
        _closeButton.Pressed += Close;
    }

    /// <summary>
    /// Wyświetla panel power-upów
    /// i ustawia fokus na przycisku zamknięcia.
    /// </summary>
    public void ShowPanel()
    {
        Visible = true;
        _closeButton.GrabFocus();
    }

    /// <summary>
    /// Obsługuje wejście użytkownika podczas wyświetlania panelu.
    /// Zamknięcie następuje po użyciu akcji <c>ui_cancel</c>.
    /// </summary>
    /// <param name="event">Zdarzenie wejściowe.</param>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible)
            return;

        if (@event.IsActionPressed("ui_cancel"))
        {
            Close();
            GetViewport().SetInputAsHandled();
        }
    }

    /// <summary>
    /// Obsługuje kliknięcie w tło panelu.
    /// Kliknięcie lewym przyciskiem myszy zamyka okno.
    /// </summary>
    /// <param name="event">Zdarzenie wejściowe myszy.</param>
    private void OnBackgroundInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent &&
            mouseEvent.Pressed &&
            mouseEvent.ButtonIndex == MouseButton.Left)
        {
            Close();
        }
    }

    /// <summary>
    /// Ukrywa panel power-upów.
    /// </summary>
    private void Close()
    {
        Visible = false;
    }
}