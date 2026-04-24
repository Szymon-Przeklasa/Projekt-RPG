using Godot;

/// <summary>
/// Przycisk interfejsu użytkownika odpowiedzialny
/// za otwieranie panelu dostępnych ulepszeń.
///
/// Po kliknięciu wyszukuje obiekt <see cref="PowerUpsUI"/>
/// w aktualnej scenie i wyświetla jego zawartość.
///
/// Klasa dziedziczy po <see cref="TextureButton"/>.
/// </summary>
public partial class PowerUpButton : TextureButton
{
    /// <summary>
    /// Otwiera panel dostępnych ulepszeń.
    /// Jeśli panel <see cref="PowerUpsUI"/> istnieje
    /// w bieżącej scenie, wywołuje metodę
    /// <see cref="PowerUpsUI.ShowPanel"/>.
    /// </summary>
    private void ShowPowerups()
    {
        var ui = GetTree().CurrentScene.GetNodeOrNull<PowerUpsUI>("PowerUpsUI");
        ui?.ShowPanel();
    }

    /// <summary>
    /// Wywoływana po najechaniu kursorem na przycisk.
    /// Przyciemnia przycisk, aby wizualnie zaznaczyć fokus.
    /// </summary>
    private void MouseOn()
    {
        this.Modulate = new Color(0.8f, 0.8f, 0.8f, 1f);
    }

    /// <summary>
    /// Wywoływana po opuszczeniu kursorem przycisku.
    /// Przywraca domyślny kolor przycisku.
    /// </summary>
    private void MouseOff()
    {
        this.Modulate = new Color(1f, 1f, 1f, 1f);
    }
}