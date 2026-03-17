using Godot;
using System;

/// <summary>
/// Klasa reprezentuj¹ca przycisk odpowiedzialny za wyœwietlanie power-upów.
/// Docelowo bêdzie zmieniaæ scenê lub otwieraæ interfejs ulepszeñ.
/// </summary>
public partial class PowerUpButton : TextureButton
{
    /// <summary>
    /// Metoda wywo³ywana po dodaniu wêz³a do drzewa sceny.
    /// </summary>
    public override void _Ready()
    {
    }

    /// <summary>
    /// Wyœwietla ekran power-upów lub zmienia scenê na odpowiedni¹.
    /// </summary>
    /// <remarks>
    /// Metoda nie jest jeszcze zaimplementowana.
    /// Docelowo powinna dzia³aæ analogicznie do przycisku startowego (zmiana sceny).
    /// </remarks>
    private void ShowPowerups()
    {
        //
    }

    /// <summary>
    /// Metoda wywo³ywana, gdy kursor myszy znajduje siê nad przyciskiem.
    /// Zmienia kolor przycisku na lekko przyciemniony.
    /// </summary>
    private void MouseOn()
    {
        this.Modulate = new Color(0.8f, 0.8f, 0.8f, 1f);
    }

    /// <summary>
    /// Metoda wywo³ywana, gdy kursor myszy opuszcza przycisk.
    /// Przywraca domyœlny kolor przycisku.
    /// </summary>
    private void MouseOff()
    {
        this.Modulate = new Color(1f, 1f, 1f, 1f);
    }

    /// <summary>
    /// Metoda wywo³ywana co klatkê.
    /// </summary>
    /// <param name="delta">Czas, jaki up³yn¹³ od poprzedniej klatki.</param>
    public override void _Process(double delta)
    {
    }
}