using Godot;
using System;
using System.Linq;

public partial class LevelUpUI : CanvasLayer
{
    private Player player;
    private Button b1;
    private Button b2;
    private Button b3;

    public override void _Ready()
    {
        // Must be Always so buttons respond while GetTree().Paused == true
        ProcessMode = ProcessModeEnum.Always;

        Visible = false;

        b1 = GetNode<Button>("Panel/VBoxContainer/Button");
        b2 = GetNode<Button>("Panel/VBoxContainer/Button2");
        b3 = GetNode<Button>("Panel/VBoxContainer/Button3");
    }

    public void ShowUpgrades(Player p)
    {
        player = p;

        var choices = player.AvailableUpgrades
            .Where(x => x.CanUpgrade)
            .OrderBy(x => GD.Randf())
            .Take(3)
            .ToList();

        if (choices.Count == 0)
        {
            GD.PrintErr("No upgrades available!");
            return;
        }

        // Odłącz stare handlery przed przypisaniem nowych
        ClearButton(b1);
        ClearButton(b2);
        ClearButton(b3);

        SetupButton(b1, choices[0]);

        if (choices.Count > 1) SetupButton(b2, choices[1]);
        else b2.Visible = false;

        if (choices.Count > 2) SetupButton(b3, choices[2]);
        else b3.Visible = false;

        GetTree().Paused = true;
        Visible = true;
    }

    /// <summary>
    /// Odłącza zapisany handler i chowa przycisk.
    /// </summary>
    private void ClearButton(Button button)
    {
        button.Visible = true;

        if (button.HasMeta("_handler"))
        {
            var callable = (Callable)button.GetMeta("_handler");
            if (button.IsConnected(Button.SignalName.Pressed, callable))
                button.Disconnect(Button.SignalName.Pressed, callable);
            button.RemoveMeta("_handler");
        }
    }

    private void SetupButton(Button button, UpgradeData data)
    {
        button.Visible = true;
        button.Text = $"{data.Name} (Lv {data.Level + 1})";

        Action handler = () =>
        {
            data.Apply(player);
            Close();
        };

        var callable = Callable.From(handler);
        button.Connect(Button.SignalName.Pressed, callable);
        button.SetMeta("_handler", callable);
    }

    private void Close()
    {
        ClearButton(b1);
        ClearButton(b2);
        ClearButton(b3);

        Visible = false;
        GetTree().Paused = false;
    }
}