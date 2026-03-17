using Godot;
using System;

/// <summary>
/// Klasa reprezentuj¹ca przycisk startowy.
/// Odpowiada za rozpoczêcie gry oraz efekty wizualne zwi¹zane z interakcj¹ myszy.
/// </summary>
public partial class StartButton : TextureButton
{
    /// <summary>
    /// Metoda wywo³ywana po dodaniu wêz³a do drzewa sceny.
    /// </summary>
    public override void _Ready()
    {
    }

    /// <summary>
    /// Rozpoczyna grê poprzez zmianê sceny na g³ówn¹ scenê rozgrywki.
    /// </summary>
    private void StartGame()
    {
        GetTree().ChangeSceneToFile("res://Scenes/game.tscn");
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