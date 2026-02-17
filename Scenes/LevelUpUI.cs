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
			.OrderBy(x => GD.Randf())
			.Take(3)
			.ToList();

		SetupButton(b1, choices[0]);
		SetupButton(b2, choices[1]);
		SetupButton(b3, choices[2]);

		GetTree().Paused = true;
		Visible = true;
	}

	private void SetupButton(Button button, UpgradeData data)
	{
		button.Text = data.Name;

		button.Pressed += () =>
		{
			data.Apply();
			Close();
		};
	}

	private void Close()
	{
		Visible = false;
		GetTree().Paused = false;
	}
}
