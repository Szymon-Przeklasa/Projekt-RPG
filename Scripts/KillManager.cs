using Godot;
using System.Collections.Generic;

/// <summary>
/// Menedżer odpowiedzialny za zliczanie zabójstw przeciwników.
/// Implementuje wzorzec singleton i zapisuje dane do pliku.
/// </summary>
public partial class KillManager : Node
{
	/// <summary>
	/// Statyczna instancja singletona.
	/// </summary>
	public static KillManager Instance;

	/// <summary>
	/// Słownik przechowujący liczbę zabójstw dla każdego typu przeciwnika (mobID).
	/// </summary>
	private Dictionary<string, int> kills = new();

	/// <summary>
	/// Ścieżka do pliku zapisu danych o zabójstwach.
	/// </summary>
	private const string SavePath = "user://kills.json";

	/// <summary>
	/// Sygnał emitowany po zaktualizowaniu liczby zabójstw.
	/// </summary>
	/// <param name="mobID">Identyfikator przeciwnika.</param>
	/// <param name="kills">Aktualna liczba zabójstw dla danego przeciwnika.</param>
	[Signal]
	public delegate void KillUpdatedEventHandler(string mobID, int kills);

	/// <summary>
	/// Metoda wywoływana po inicjalizacji węzła.
	/// Ustawia instancję singletona oraz wczytuje zapisane dane.
	/// </summary>
	public override void _Ready()
	{
		Instance = this;
		LoadKills(); // load saved kills when game starts
	}

	/// <summary>
	/// Rejestruje zabójstwo przeciwnika o podanym identyfikatorze.
	/// Aktualizuje licznik, emituje sygnał oraz zapisuje dane do pliku.
	/// </summary>
	/// <param name="mobID">Identyfikator przeciwnika.</param>
	public void RegisterKill(string mobID)
	{
		if (!kills.ContainsKey(mobID))
			kills[mobID] = 0;

		kills[mobID]++;

		EmitSignal(SignalName.KillUpdated, mobID, kills[mobID]);

		SaveKills(); // save every time a kill happens
	}

	/// <summary>
	/// Zwraca liczbę zabójstw dla danego przeciwnika.
	/// </summary>
	/// <param name="mobID">Identyfikator przeciwnika.</param>
	/// <returns>Liczba zabójstw lub 0, jeśli brak wpisu.</returns>
	public int GetKills(string mobID)
	{
		if (!kills.ContainsKey(mobID))
			return 0;

		return kills[mobID];
	}

	/// <summary>
	/// Zwraca wszystkie zapisane statystyki zabójstw.
	/// </summary>
	/// <returns>Słownik zawierający liczbę zabójstw dla każdego przeciwnika.</returns>
	public Dictionary<string, int> GetAllKills()
	{
		return kills;
	}

	/// <summary>
	/// Zapisuje dane o zabójstwach do pliku w formacie JSON.
	/// </summary>
	private void SaveKills()
	{
		var godotDict = new Godot.Collections.Dictionary();

		foreach (var pair in kills)
		{
			godotDict[pair.Key] = pair.Value;
		}

		var json = Json.Stringify(godotDict);

		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		file.StoreString(json);
	}

	/// <summary>
	/// Wczytuje dane o zabójstwach z pliku JSON.
	/// Jeśli plik nie istnieje, metoda nie wykonuje żadnej akcji.
	/// </summary>
	private void LoadKills()
	{
		if (!FileAccess.FileExists(SavePath))
			return;

		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		var json = file.GetAsText();

		var parsed = Json.ParseString(json);

		if (parsed.VariantType == Variant.Type.Dictionary)
		{
			var dict = parsed.AsGodotDictionary();

			kills.Clear();

			foreach (var key in dict.Keys)
			{
				kills[key.ToString()] = (int)dict[key];
			}
		}
	}
}
