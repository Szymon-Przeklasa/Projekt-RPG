using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Interfejs wyboru ulepszenia po awansie.
/// Wyświetla 3 losowe opcje (broń/pasywkę).
/// Bronie nieodblokowane pokazują "UNLOCK" zamiast poziomu.
/// </summary>
public partial class LevelUpUI : CanvasLayer
{
	private Player _player;
	private Button _b1;
	private Button _b2;
	private Button _b3;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Visible = false;

		_b1 = GetNode<Button>("Panel/VBoxContainer/Button");
		_b2 = GetNode<Button>("Panel/VBoxContainer/Button2");
		_b3 = GetNode<Button>("Panel/VBoxContainer/Button3");
	}

	public void ShowUpgrades(Player player)
	{
		_player = player;
		_player.IsInLevelUp = true;
		GetTree().Paused = true;
		_player.SetWeaponsProcessMode(ProcessModeEnum.Disabled);
		Visible = true;

		List<UpgradeData> choices = player.AvailableUpgrades
			.Where(u => u.CanUpgrade)
			.OrderBy(_ => GD.Randf())
			.Take(3)
			.ToList();

		if (choices.Count == 0)
		{
			GD.PrintErr("LevelUpUI: brak dostępnych ulepszeń!");
			return;
		}

		ClearButton(_b1);
		ClearButton(_b2);
		ClearButton(_b3);

		SetupButton(_b1, choices.Count > 0 ? choices[0] : null);
		SetupButton(_b2, choices.Count > 1 ? choices[1] : null);
		SetupButton(_b3, choices.Count > 2 ? choices[2] : null);
	}

	private void SetupButton(Button button, UpgradeData data)
	{
		if (data == null)
		{
			button.Visible = false;
			return;
		}

		button.Visible = true;

		string levelTag;
		if (data.Level == 0)
			levelTag = "[NEW ✨]";
		else
		{
			int nextLevel = data.Level + 1;
			levelTag = $"[Lv {data.Level} → {nextLevel}]";
		}

		button.Text = $"{data.Name}  {levelTag}\n{data.NextLevelDescription}";

		Action handler = () =>
		{
			data.Apply(_player);
			Close();
		};

		var callable = Callable.From(handler);
		button.Connect(Button.SignalName.Pressed, callable);
		button.SetMeta("_handler", callable);
	}

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

	private void Close()
	{
		ClearButton(_b1);
		ClearButton(_b2);
		ClearButton(_b3);

		if (_player != null)
		{
			_player.IsInLevelUp = false;
			_player.SetWeaponsProcessMode(ProcessModeEnum.Pausable);
		}

		GetTree().Paused = false;
		Visible = false;
	}
}