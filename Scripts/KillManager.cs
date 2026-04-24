using Godot;
using System.Collections.Generic;

/// <summary>
/// Menedżer odpowiedzialny za zliczanie zabójstw przeciwników.
/// Implementuje wzorzec Singleton i zapisuje dane do pliku.
///
/// Przechowuje dwa typy danych:
/// <list type="bullet">
/// <item><description><b>kills</b> — całkowita liczba zabójstw (zapisywana między sesjami),</description></item>
/// <item><description><b>sessionKills</b> — zabójstwa tylko z bieżącej rozgrywki.</description></item>
/// </list>
/// </summary>
public partial class KillManager : Node
{
	/// <summary>Globalna instancja singletona.</summary>
	public static KillManager Instance;

	/// <summary>
	/// Całkowita liczba zabójstw zapisanych na dysku.
	/// </summary>
	private Dictionary<string, int> kills = new();

	/// <summary>
	/// Zabójstwa z bieżącej sesji gry (resetowane po uruchomieniu gry).
	/// </summary>
	private Dictionary<string, int> sessionKills = new();

	/// <summary>Ścieżka zapisu danych killów.</summary>
	private const string SavePath = "user://kills.json";

	/// <summary>
	/// Sygnał wywoływany po zarejestrowaniu zabójstwa.
	/// </summary>
	/// <param name="mobID">Identyfikator przeciwnika.</param>
	/// <param name="kills">Nowa całkowita liczba zabójstw tego przeciwnika.</param>
	[Signal]
	public delegate void KillUpdatedEventHandler(string mobID, int kills);

	/// <summary>
	/// Inicjalizuje singleton i ładuje zapisane dane z dysku.
	/// </summary>
	public override void _Ready()
	{
		Instance = this;
		LoadKills();
	}

	/// <summary>
	/// Rejestruje zabójstwo przeciwnika.
	/// Aktualizuje zarówno dane sesyjne, jak i globalne,
	/// a następnie zapisuje dane do pliku.
	/// </summary>
	/// <param name="mobID">Identyfikator przeciwnika.</param>
	public void RegisterKill(string mobID)
	{
		if (!kills.ContainsKey(mobID)) kills[mobID] = 0;
		if (!sessionKills.ContainsKey(mobID)) sessionKills[mobID] = 0;

		kills[mobID]++;
		sessionKills[mobID]++;

		EmitSignal(SignalName.KillUpdated, mobID, kills[mobID]);
		SaveKills();
	}

	/// <summary>
	/// Zwraca całkowitą liczbę zabójstw danego przeciwnika.
	/// </summary>
	/// <param name="mobID">Identyfikator przeciwnika.</param>
	/// <returns>Liczba zabójstw lub 0 jeśli brak danych.</returns>
	public int GetKills(string mobID) =>
		kills.TryGetValue(mobID, out int v) ? v : 0;

	/// <summary>
	/// Zwraca wszystkie zapisane zabójstwa (globalne).
	/// </summary>
	public Dictionary<string, int> GetAllKills() => kills;

	/// <summary>
	/// Zwraca zabójstwa tylko z bieżącej sesji gry.
	/// </summary>
	public Dictionary<string, int> GetSessionKills() => sessionKills;

	/// <summary>
	/// Zwraca łączną liczbę zabójstw z bieżącej sesji.
	/// </summary>
	public int GetSessionTotalKills()
	{
		int total = 0;
		foreach (var v in sessionKills.Values)
			total += v;

		return total;
	}

	/// <summary>
	/// Zapisuje dane zabójstw do pliku JSON w katalogu użytkownika.
	/// </summary>
	private void SaveKills()
	{
		var godotDict = new Godot.Collections.Dictionary();

		foreach (var pair in kills)
			godotDict[pair.Key] = pair.Value;

		var json = Json.Stringify(godotDict);

		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		file.StoreString(json);
	}

	/// <summary>
	/// Ładuje zapisane zabójstwa z pliku JSON.
	/// Jeśli plik nie istnieje, metoda nic nie robi.
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
				kills[key.ToString()] = (int)dict[key];
		}
	}
}
