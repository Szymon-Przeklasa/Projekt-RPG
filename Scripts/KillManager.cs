using Godot;
using System.Collections.Generic;

/// <summary>
/// Menedżer odpowiedzialny za zliczanie zabójstw przeciwników.
/// Implementuje wzorzec singleton i zapisuje dane do pliku.
/// kills        — cumulative (persisted to disk across sessions)
/// sessionKills — this run only (reset on game load)
/// </summary>
public partial class KillManager : Node
{
	public static KillManager Instance;

	private Dictionary<string, int> kills        = new();
	private Dictionary<string, int> sessionKills = new();

	private const string SavePath = "user://kills.json";

	[Signal]
	public delegate void KillUpdatedEventHandler(string mobID, int kills);

	public override void _Ready()
	{
		Instance = this;
		LoadKills();
	}

	public void RegisterKill(string mobID)
	{
		if (!kills.ContainsKey(mobID))        kills[mobID]        = 0;
		if (!sessionKills.ContainsKey(mobID)) sessionKills[mobID] = 0;

		kills[mobID]++;
		sessionKills[mobID]++;

		EmitSignal(SignalName.KillUpdated, mobID, kills[mobID]);
		SaveKills();
	}

	public int GetKills(string mobID) => kills.TryGetValue(mobID, out int v) ? v : 0;

	public Dictionary<string, int> GetAllKills() => kills;

	/// <summary>Kills recorded only during this game session (resets each run).</summary>
	public Dictionary<string, int> GetSessionKills() => sessionKills;

	/// <summary>Total kills this session across all mob types.</summary>
	public int GetSessionTotalKills()
	{
		int total = 0;
		foreach (var v in sessionKills.Values) total += v;
		return total;
	}

	private void SaveKills()
	{
		var godotDict = new Godot.Collections.Dictionary();
		foreach (var pair in kills)
			godotDict[pair.Key] = pair.Value;

		var json = Json.Stringify(godotDict);
		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		file.StoreString(json);
	}

	private void LoadKills()
	{
		if (!FileAccess.FileExists(SavePath)) return;

		using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		var json   = file.GetAsText();
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

