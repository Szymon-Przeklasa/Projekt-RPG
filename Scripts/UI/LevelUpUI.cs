using Godot;
using System;
using System.Linq;

/// <summary>
/// Klasa odpowiedzialna za interfejs użytkownika poziomu ulepszeń (Level Up UI).
/// Dziedziczy po CanvasLayer.
/// Pozwala wybrać do 3 dostępnych ulepszeń dla gracza i wstrzymuje grę podczas wyboru.
/// </summary>
public partial class LevelUpUI : CanvasLayer
{
    /// <summary>
    /// Odniesienie do gracza, którego ulepszenia są wyświetlane.
    /// </summary>
    private Player player;

    /// <summary>
    /// Przycisk reprezentujący pierwsze dostępne ulepszenie.
    /// </summary>
    private Button b1;

    /// <summary>
    /// Przycisk reprezentujący drugie dostępne ulepszenie.
    /// </summary>
    private Button b2;

    /// <summary>
    /// Przycisk reprezentujący trzecie dostępne ulepszenie.
    /// </summary>
    private Button b3;

    /// <summary>
    /// Metoda wywoływana po załadowaniu sceny.
    /// Inicjalizuje przyciski i ustawia widoczność interfejsu na false.
    /// </summary>
    public override void _Ready()
    {
        Visible = false;

        b1 = GetNode<Button>("Panel/VBoxContainer/Button");
        b2 = GetNode<Button>("Panel/VBoxContainer/Button2");
        b3 = GetNode<Button>("Panel/VBoxContainer/Button3");
    }

    /// <summary>
    /// Wyświetla interfejs wyboru ulepszeń dla danego gracza.
    /// Losuje do 3 ulepszeń, które gracz może aktualnie odblokować.
    /// </summary>
    /// <param name="p">Gracz, dla którego wyświetlamy dostępne ulepszenia.</param>
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

        // Resetuj wszystkie przyciski przed ponownym użyciem
        ResetButton(b1);
        ResetButton(b2);
        ResetButton(b3);

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

    /// <summary>
    /// Odłącza wszystkie listenery Pressed od przycisku i ukrywa go.
    /// Zapobiega wielokrotnemu aplikowaniu tego samego ulepszenia.
    /// </summary>
    /// <param name="button">Przycisk do zresetowania.</param>
    private void ResetButton(Button button)
    {
        // Odłącz wszystkie podłączone sygnały Pressed
        var connections = button.GetSignalConnectionList("pressed");
        foreach (var connection in connections)
        {
            var callable = (Callable)connection["callable"];
            button.Disconnect("pressed", callable);
        }

        // Wyczyść przez disconnect wszystkich — bezpieczniejszy sposób w Godot 4
        if (button.IsConnected(Button.SignalName.Pressed, new Callable(this, MethodName.Close)))
            button.Pressed -= Close;

        button.Visible = true;
    }

    /// <summary>
    /// Konfiguruje pojedynczy przycisk z danym ulepszeniem.
    /// </summary>
    /// <param name="button">Przycisk do skonfigurowania.</param>
    /// <param name="data">Dane ulepszenia przypisanego do przycisku.</param>
    private void SetupButton(Button button, UpgradeData data)
    {
        button.Visible = true;
        button.Text = $"{data.Name} (Lv {data.Level + 1})";

        // Użyj lokalnej lambdy przypisanej do zmiennej,
        // żeby móc ją bezpiecznie odłączyć przy następnym ShowUpgrades
        void OnPressed()
        {
            data.Apply(player);
            Close();
        }

        button.Pressed += OnPressed;

        // Zachowaj referencję do lambdy w metadanych przycisku,
        // żeby móc ją odłączyć przy następnym wywołaniu ShowUpgrades
        button.SetMeta("_upgrade_handler", Callable.From(OnPressed));
    }

    /// <summary>
    /// Zamknięcie interfejsu wyboru ulepszeń i wznowienie gry.
    /// </summary>
    private void Close()
    {
        // Odłącz handlery upgrade ze wszystkich przycisków przed zamknięciem
        DisconnectUpgradeHandler(b1);
        DisconnectUpgradeHandler(b2);
        DisconnectUpgradeHandler(b3);

        Visible = false;
        GetTree().Paused = false;
    }

    /// <summary>
    /// Odłącza handler ulepszenia zapisany w metadanych przycisku.
    /// </summary>
    /// <param name="button">Przycisk, którego handler ma zostać odłączony.</param>
    private void DisconnectUpgradeHandler(Button button)
    {
        if (!button.HasMeta("_upgrade_handler"))
            return;

        var callable = (Callable)button.GetMeta("_upgrade_handler");
        if (button.IsConnected(Button.SignalName.Pressed, callable))
            button.Disconnect(Button.SignalName.Pressed, callable);

        button.RemoveMeta("_upgrade_handler");
    }
}