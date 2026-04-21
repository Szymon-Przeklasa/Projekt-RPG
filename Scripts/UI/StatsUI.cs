using Godot;
using System.Collections.Generic;

/// <summary>
/// Panel statystyk wyświetlany w menu pauzy.
/// Pokazuje level, XP oraz poziomy odblokowanych broni.
/// </summary>
public partial class StatsUI : HBoxContainer
{
	private Label _levelValue;
	private Label _xpValue;
	private Dictionary<string, ProgressBar> _weaponBars;

	public override void _Ready()
	{
		_levelValue = GetNode<Label>("LeftColumn/StatValues/HBoxContainer/LevelValue");
		_xpValue = GetNode<Label>("LeftColumn/StatValues/HBoxContainer2/XPValue");
		_weaponBars = new Dictionary<string, ProgressBar>
		{
			{ "Fire Wand", GetNode<ProgressBar>("RightColumn/FireWand/ProgressBar") },
			{ "Lightning", GetNode<ProgressBar>("RightColumn/Lightning/ProgressBar") },
			{ "Garlic", GetNode<ProgressBar>("RightColumn/Garlic/ProgressBar") },
			{ "Magic Missile", GetNode<ProgressBar>("RightColumn/Magic Missle/ProgressBar") },
			{ "Axe", GetNode<ProgressBar>("RightColumn/Axe/ProgressBar") },
		};
	}

	public void Refresh(Player player)
	{
		if (_levelValue == null || _xpValue == null || _weaponBars == null)
			return;

		if (player == null)
		{
			_levelValue.Text = "-";
			_xpValue.Text = "-";
			foreach (var bar in _weaponBars.Values)
				SetBarState(bar, 0);
			return;
		}

		_levelValue.Text = player.Level.ToString();
		_xpValue.Text = $"{player.Xp}/{player.XpToLevel}";

		foreach (var pair in _weaponBars)
			SetBarState(pair.Value, GetUpgradeLevel(player, pair.Key));
	}

	private int GetUpgradeLevel(Player player, string upgradeName)
	{
		foreach (var upgrade in player.AvailableUpgrades)
		{
			if (upgrade.Type == UpgradeType.Weapon && upgrade.Name == upgradeName)
				return upgrade.Level;
		}

		return 0;
	}

	private void SetBarState(ProgressBar bar, int level)
	{
		bar.Value = level;
		bar.Modulate = level > 0 ? Colors.White : new Color(0.45f, 0.45f, 0.45f, 1f);
	}
}
