using Godot;
using System;

/// <summary>
/// Klasa reprezentujaca przycisk startowy.
/// Odpowiada za rozpoczecie gry oraz efekty wizualne zwiazane z interakcja myszy.
/// </summary>
public partial class StartButton : TextureButton
{
    /// <summary>
    /// Metoda wywolywana po dodaniu wezla do drzewa sceny.
    /// </summary>
    public override void _Ready()
    {
    }

    /// <summary>
    /// Rozpoczyna gre poprzez zmiane sceny na glowna scene rozgrywki.
    /// </summary>
    private void StartGame()
    {
        GetTree().ChangeSceneToFile("res://Scenes/game.tscn");
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