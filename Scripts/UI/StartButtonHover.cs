using Godot;
using System;

/// <summary>
/// Klasa obslugujaca efekt wizualny najechania kursorem na przycisk startowy.
/// Zmienia kolor (modulacje) obiektu w zaleznosci od interakcji myszy.
/// </summary>
public partial class StartButtonHover : CollisionPolygon2D
{
    /// <summary>
    /// Metoda wywolywana po pierwszym dodaniu wezla do drzewa sceny.
    /// </summary>
    public override void _Ready()
    {
    }

    /// <summary>
    /// Metoda wywolywana, gdy kursor myszy znajduje sie nad elementem.
    /// Zmienia kolor obiektu na lekko przyciemniony.
    /// </summary>
    private void MouseOn()
    {
        this.Modulate = new Color(0.8f, 0.8f, 0.8f, 1f);
    }

    /// <summary>
    /// Metoda wywolywana, gdy kursor myszy opuszcza element.
    /// Przywraca domyslny kolor obiektu.
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