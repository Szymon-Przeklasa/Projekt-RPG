using Godot;
using System.Collections.Generic;

/// <summary>
/// Pasek ekwipunku na dole ekranu.
/// Wyświetla posiadane bronie (lewo) i pasywki (prawo) z ich poziomami.
/// </summary>
public partial class EquipmentUI : HBoxContainer
{
	// Węzły wewnętrzne
	private HBoxContainer _weaponsContainer;
	private HBoxContainer _passivesContainer;

	// Nazwy skrócone broni do wyświetlania
	private static readonly Dictionary<string, string> WeaponShort = new()
	{
		{ "FireWand",     "🔥FW" },
		{ "Lightning",    "⚡LT" },
		{ "Garlic",       "🧄GR" },
		{ "Magnet",       "🧲MG" },
		{ "MagicMissile", "✨MM" },
		{ "Axe",          "🪓AX" },
	};

	private static readonly Dictionary<string, string> PassiveShort = new()
	{
		{ "Spinach",      "🌿SP" },
		{ "Pummarola",    "❤️PM" },
		{ "Hollow Heart", "💜HH" },
		{ "Bracer",       "💨BR" },
		{ "Wings",        "🦅WG" },
	};

	public override void _Ready()
	{
		_weaponsContainer  = GetNode<HBoxContainer>("WeaponsGroup");
		_passivesContainer = GetNode<HBoxContainer>("PassivesGroup");
	}

	/// <summary>Odświeża wyświetlane ikony na podstawie stanu gracza.</summary>
	public void Refresh(Player player)
	{
		BuildWeapons(player);
		BuildPassives(player);
	}

	private void BuildWeapons(Player player)
	{
		ClearChildren(_weaponsContainer);

		// Sloty broni
		for (int i = 0; i < Player.MAX_WEAPONS; i++)
		{
			var slot = MakeSlot();
			if (i < player.Weapons.Count)
			{
				var w = player.Weapons[i];
				string typeName = w.GetType().Name;
				string label = WeaponShort.TryGetValue(typeName, out var s) ? s : typeName[..2];

				// Znajdź poziom w AvailableUpgrades
				int level = 1;
				foreach (var upg in player.AvailableUpgrades)
				{
					if (upg.Type == UpgradeType.Weapon && upg.Name.Replace(" ", "") == typeName.Replace(" ", ""))
					{
						level = upg.Level;
						break;
					}
				}

				SetSlotActive(slot, label, level);
			}
			_weaponsContainer.AddChild(slot);
		}
	}

	private void BuildPassives(Player player)
	{
		ClearChildren(_passivesContainer);

		var ownedPassives = new List<KeyValuePair<string, int>>();
		foreach (var upg in player.AvailableUpgrades)
		{
			if (upg.Type == UpgradeType.Passive && upg.Level > 0)
				ownedPassives.Add(new KeyValuePair<string, int>(upg.Name, upg.Level));
		}

		for (int i = 0; i < Player.MAX_PASSIVES; i++)
		{
			var slot = MakeSlot();
			if (i < ownedPassives.Count)
			{
				string name = ownedPassives[i].Key;
				string label = PassiveShort.TryGetValue(name, out var s) ? s : name[..2];
				SetSlotActive(slot, label, ownedPassives[i].Value);
			}
			_passivesContainer.AddChild(slot);
		}
	}

	// ── Helpers ───────────────────────────────────────────────

	private Panel MakeSlot()
	{
		var panel = new Panel();
		panel.CustomMinimumSize = new Vector2(52, 52);

		// Szare puste tło
		var style = new StyleBoxFlat();
		style.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
		style.BorderColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
		style.SetBorderWidthAll(2);
		style.SetCornerRadiusAll(4);
		panel.AddThemeStyleboxOverride("panel", style);

		return panel;
	}

	private void SetSlotActive(Panel slot, string shortName, int level)
	{
		// Tło aktywnego slotu
		var style = new StyleBoxFlat();
		style.BgColor = new Color(0.15f, 0.12f, 0.25f, 0.85f);
		style.BorderColor = new Color(0.6f, 0.3f, 1f, 0.9f);
		style.SetBorderWidthAll(2);
		style.SetCornerRadiusAll(4);
		slot.AddThemeStyleboxOverride("panel", style);

		// Kontener pionowy: nazwa + poziom
		var vbox = new VBoxContainer();
		vbox.AnchorRight = 1; vbox.AnchorBottom = 1;
		vbox.OffsetLeft = 2; vbox.OffsetRight = -2;
		vbox.OffsetTop = 2; vbox.OffsetBottom = -2;
		vbox.Alignment = BoxContainer.AlignmentMode.Center;

		var nameLabel = new Label();
		nameLabel.Text = shortName;
		nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
		nameLabel.AddThemeFontSizeOverride("font_size", 10);
		nameLabel.AddThemeColorOverride("font_color", Colors.White);

		var lvLabel = new Label();
		lvLabel.Text = $"Lv{level}";
		lvLabel.HorizontalAlignment = HorizontalAlignment.Center;
		lvLabel.AddThemeFontSizeOverride("font_size", 9);
		lvLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.6f, 1f));

		vbox.AddChild(nameLabel);
		vbox.AddChild(lvLabel);
		slot.AddChild(vbox);
	}

	private void ClearChildren(Control container)
	{
		foreach (Node child in container.GetChildren())
			child.QueueFree();
	}
}
