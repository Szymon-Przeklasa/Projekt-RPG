using Godot;

/// <summary>
/// Klasa reprezentująca pojedynczy wpis przeciwnika w interfejsie użytkownika.
/// Wyświetla nazwę przeciwnika, liczbę zabójstw oraz pasek postępu.
/// </summary>
public partial class MobEntry : HBoxContainer
{
    /// <summary>
    /// Ustawia dane dla wpisu przeciwnika w UI.
    /// Aktualizuje nazwę, licznik zabójstw oraz pasek postępu.
    /// </summary>
    /// <param name="mobID">Identyfikator przeciwnika (nazwa).</param>
    /// <param name="kills">Liczba zabójstw danego przeciwnika.</param>
    public void SetData(string mobID, int kills)
    {
        GetNode<Label>("MobInfo/MobName").Text = mobID;

        GetNode<Label>("MobInfo/KillCounter/CurrentKills").Text = kills.ToString();

        GetNode<ProgressBar>("MobInfo/ProgressBar").Value = kills;
    }
}