using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Interfejs wyboru ulepszenia po awansie.
/// Wyświetla 3 losowe opcje (broń/pasywkę) — każda pokazuje nazwę,
/// aktualny poziom i opis kolejnego poziomu, tak jak w Vampire Survivors.
/// </summary>
public partial class LevelUpUI : CanvasLayer
{
    private Player _player;
    private Button _b1;
    private Button _b2;
    private Button _b3;

    public override void _Ready()
    {
        // Always — przyciski muszą reagować podczas pauzy gry
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;

        _b1 = GetNode<Button>("Panel/VBoxContainer/Button");
        _b2 = GetNode<Button>("Panel/VBoxContainer/Button2");
        _b3 = GetNode<Button>("Panel/VBoxContainer/Button3");
    }

    /// <summary>
    /// Pokazuje ekran ulepszenia dla danego gracza.
    /// Wybiera 3 losowe ulepszenia (lub mniej, jeśli dostępnych jest mniej).
    /// </summary>
    public void ShowUpgrades(Player player)
    {
        _player = player;

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

        GetTree().Paused = true;
        Visible = true;
    }

    // ── Przyciski ────────────────────────────────────────────

    private void SetupButton(Button button, UpgradeData data)
    {
        if (data == null)
        {
            button.Visible = false;
            return;
        }

        button.Visible = true;

        // Tekst przycisku: "Nazwa  [Lv X → X+1]\nOpis następnego poziomu"
        int nextLevel = data.Level + 1;
        string levelTag = data.Level == 0
            ? "[NEW]"
            : $"[Lv {data.Level} → {nextLevel}]";

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

    // ── Zamknięcie ───────────────────────────────────────────

    private void Close()
    {
        ClearButton(_b1);
        ClearButton(_b2);
        ClearButton(_b3);

        Visible = false;
        GetTree().Paused = false;
    }
}