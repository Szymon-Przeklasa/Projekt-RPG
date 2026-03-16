using Godot;
using System.Linq;

public partial class LevelUpUI : CanvasLayer
{
    private Player player;

    private Button b1;
    private Button b2;
    private Button b3;

    public override void _Ready()
    {
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

        SetupButton(b1, choices[0]);

        if (choices.Count > 1)
            SetupButton(b2, choices[1]);
        else
            b2.Visible = false;

        if (choices.Count > 2)
            SetupButton(b3, choices[2]);
        else
            b3.Visible = false;

        GetTree().Paused = true;
        Visible = true;
    }

    private void SetupButton(Button button, UpgradeData data)
    {
        button.Visible = true;
        button.Text = $"{data.Name} (Lv {data.Level + 1})";

        button.Pressed += () =>
        {
            data.Apply(player);
            Close();
        };
    }

    private void Close()
    {
        Visible = false;
        GetTree().Paused = false;
    }
}