using Godot;
using System.Collections.Generic;

public partial class KillManager : Node
{
    public static KillManager Instance;

    private Dictionary<string, int> kills = new();

    private const string SavePath = "user://kills.json";

    [Signal]
    public delegate void KillUpdatedEventHandler(string mobID, int kills);

    public override void _Ready()
    {
        Instance = this;
        LoadKills(); // load saved kills when game starts
    }

    public void RegisterKill(string mobID)
    {
        if (!kills.ContainsKey(mobID))
            kills[mobID] = 0;

        kills[mobID]++;

        EmitSignal(SignalName.KillUpdated, mobID, kills[mobID]);

        SaveKills(); // save every time a kill happens
    }

    public int GetKills(string mobID)
    {
        if (!kills.ContainsKey(mobID))
            return 0;

        return kills[mobID];
    }

    public Dictionary<string, int> GetAllKills()
    {
        return kills;
    }

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