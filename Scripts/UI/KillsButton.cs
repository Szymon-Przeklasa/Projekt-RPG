using Godot;
using System;

/// <summary>
/// Klasa reprezentująca przycisk do wyświetlania statystyk zabójstw (KillsUI).
/// Odpowiada za obsługę kliknięcia i efektów wizualnych myszy.
/// </summary>
public partial class KillsButton : TextureButton
{
    /// <summary>
    /// Metoda wywoływana po dodaniu węzła do drzewa sceny.
    /// </summary>
    public override void _Ready()
    {
    }

    /// <summary>
    /// Obsługuje kliknięcie przycisku.
    /// Wywołuje metodę ShowKills() w KillsUI.
    /// </summary>
    private void KillsClicked()
    {
        var ui = GetTree().CurrentScene.GetNode<KillsUI>("KillsUI");
        ui.ShowKills();
    }

    /// <summary>
    /// Metoda wywoływana, gdy kursor myszy znajduje się nad przyciskiem.
    /// Zmienia kolor przycisku na lekko przyciemniony.
    /// </summary>
    private void MouseOn()
    {
        this.Modulate = new Color(0.8f, 0.8f, 0.8f, 1f);
    }

    /// <summary>
    /// Metoda wywoływana, gdy kursor myszy opuszcza przycisk.
    /// Przywraca domyślny kolor przycisku.
    /// </summary>
    private void MouseOff()
    {
        this.Modulate = new Color(1f, 1f, 1f, 1f);
    }

    /// <summary>
    /// Metoda wywoływana co klatkę.
    /// </summary>
    /// <param name="delta">Czas, jaki upłynął od poprzedniej klatki.</param>
    public override void _Process(double delta)
    {
    }
}