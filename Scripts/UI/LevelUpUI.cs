using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Interfejs użytkownika odpowiedzialny za wybór ulepszenia po awansie gracza.
///
/// Po osiągnięciu nowego poziomu:
/// <list type="bullet">
/// <item><description>zatrzymuje rozgrywkę,</description></item>
/// <item><description>losuje maksymalnie 3 dostępne ulepszenia,</description></item>
/// <item><description>wyświetla je jako przyciski wyboru,</description></item>
/// <item><description>po wyborze stosuje ulepszenie i zamyka panel.</description></item>
/// </list>
///
/// Klasa dziedziczy po <see cref="CanvasLayer"/>.
/// </summary>
public partial class LevelUpUI : CanvasLayer
{
	/// <summary>
	/// Referencja do aktualnego gracza wybierającego ulepszenie.
	/// </summary>
	private Player _player;

	/// <summary>
	/// Pierwszy przycisk opcji ulepszenia.
	/// </summary>
	private Button _b1;

	/// <summary>
	/// Drugi przycisk opcji ulepszenia.
	/// </summary>
	private Button _b2;

	/// <summary>
	/// Trzeci przycisk opcji ulepszenia.
	/// </summary>
	private Button _b3;

	/// <summary>
	/// Inicjalizuje interfejs po dodaniu do drzewa sceny.
	/// Pobiera referencje do przycisków i ustawia panel jako ukryty.
	/// </summary>
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Visible = false;

		_b1 = GetNode<Button>("Panel/VBoxContainer/Button");
		_b2 = GetNode<Button>("Panel/VBoxContainer/Button2");
		_b3 = GetNode<Button>("Panel/VBoxContainer/Button3");
	}

	/// <summary>
	/// Wyświetla panel wyboru ulepszeń dla wskazanego gracza.
	/// Losuje maksymalnie 3 dostępne opcje z listy ulepszeń.
	/// </summary>
	/// <param name="player">Gracz otrzymujący ulepszenie.</param>
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
			Close();
			return;
		}

		ClearButton(_b1);
		ClearButton(_b2);
		ClearButton(_b3);

		SetupButton(_b1, choices.Count > 0 ? choices[0] : null);
		SetupButton(_b2, choices.Count > 1 ? choices[1] : null);
		SetupButton(_b3, choices.Count > 2 ? choices[2] : null);
	}

	/// <summary>
	/// Konfiguruje przycisk dla pojedynczego ulepszenia.
	/// Ustawia tekst oraz przypisuje akcję wyboru.
	/// </summary>
	/// <param name="button">Przycisk do skonfigurowania.</param>
	/// <param name="data">Dane ulepszenia.</param>
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

	/// <summary>
	/// Usuwa poprzednie połączenie sygnału z przycisku
	/// i resetuje jego stan.
	/// </summary>
	/// <param name="button">Przycisk do wyczyszczenia.</param>
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

	/// <summary>
	/// Zamyka panel ulepszeń i przywraca stan gry.
	/// Wznawia rozgrywkę oraz ponownie aktywuje bronie gracza.
	/// </summary>
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
