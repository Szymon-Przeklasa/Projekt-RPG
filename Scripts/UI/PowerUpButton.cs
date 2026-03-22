using Godot;
using System;

/// <summary>
/// Klasa reprezentujaca przycisk odpowiedzialny za wyswietlanie power-upow.
/// Docelowo bedzie zmieniac scene lub otwierac interfejs ulepszen.
/// </summary>
public partial class PowerUpButton : TextureButton
{
    /// <summary>
    /// Metoda wywolywana po dodaniu wezla do drzewa sceny.
    /// </summary>
    public override void _Ready()
    {
    }

    /// <summary>
    /// Wyswietla ekran power-upow lub zmienia scene na odpowiednia.
    /// </summary>
    /// <remarks>
    /// Metoda nie jest jeszcze zaimplementowana.
    /// Docelowo powinna dzialac analogicznie do przycisku startowego (zmiana sceny).
    /// </remarks>
    private void ShowPowerups()
    {
        //
    }

    /// <summary>
    /// Metoda wywolywana, gdy kursor myszy znajduje sie nad przyciskiem.
    /// Zmienia kolor przycisku na lekko przyciemniony.
    /// </summary>
    private void MouseOn()
    {
        this.Modulate = new Color(0.8f, 0.8f, 0.8f, 1f);
    }

    /// <summary>
    /// Metoda wywolywana, gdy kursor myszy opuszcza przycisk.
    /// Przywraca domyslny kolor przycisku.
    /// </summary>
    private void MouseOff()
    {
        this.Modulate = new Color(1f, 1f, 1f, 1f);
    }

    /// <summary>
    /// Metoda wywolywana co klatke.
    /// </summary>
    /// <param name="delta">Czas, jaki uplynal od poprzedniej klatki.</param>
    public override void _Process(double delta)
    {
    }
}