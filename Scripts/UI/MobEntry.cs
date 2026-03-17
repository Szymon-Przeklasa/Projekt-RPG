using Godot;

/// <summary>
/// Klasa reprezentuj¹ca pojedynczy wpis przeciwnika w interfejsie u¿ytkownika.
/// Wyœwietla nazwê przeciwnika, liczbê zabójstw oraz pasek postêpu.
/// </summary>
public partial class MobEntry : HBoxContainer
{
    /// <summary>
    /// Ustawia dane dla wpisu przeciwnika w UI.
    /// Aktualizuje nazwê, licznik zabójstw oraz pasek postêpu.
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