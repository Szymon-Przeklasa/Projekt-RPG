using Godot;

/// <summary>
/// Klasa reprezentujaca przycisk odpowiedzialny za wyswietlanie power-upow.
/// </summary>
public partial class PowerUpButton : TextureButton
{
    private void ShowPowerups()
    {
        var ui = GetTree().CurrentScene.GetNodeOrNull<PowerUpsUI>("PowerUpsUI");
        ui?.ShowPanel();
    }

    private void MouseOn()
    {
        this.Modulate = new Color(0.8f, 0.8f, 0.8f, 1f);
    }

    private void MouseOff()
    {
        this.Modulate = new Color(1f, 1f, 1f, 1f);
    }
}
