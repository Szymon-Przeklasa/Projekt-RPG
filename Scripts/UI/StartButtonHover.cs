using Godot;
using System;

/// <summary>
/// Klasa obs³uguj¹ca efekt wizualny najechania kursorem na przycisk startowy.
/// Zmienia kolor (modulacjê) obiektu w zale¿noœci od interakcji myszy.
/// </summary>
public partial class StartButtonHover : CollisionPolygon2D
{
    /// <summary>
    /// Metoda wywo³ywana po pierwszym dodaniu wêz³a do drzewa sceny.
    /// </summary>
    public override void _Ready()
    {
    }

    /// <summary>
    /// Metoda wywo³ywana, gdy kursor myszy znajduje siê nad elementem.
    /// Zmienia kolor obiektu na lekko przyciemniony.
    /// </summary>
    private void MouseOn()
    {
        this.Modulate = new Color(0.8f, 0.8f, 0.8f, 1f);
    }

    /// <summary>
    /// Metoda wywo³ywana, gdy kursor myszy opuszcza element.
    /// Przywraca domyœlny kolor obiektu.
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