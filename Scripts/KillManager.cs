using Godot;
using System.Collections.Generic;

public partial class KillManager : Node
{
    public static KillManager Instance;

    private Dictionary<string, int> kills = new();

    [Signal]
    public delegate void KillUpdatedEventHandler(string mobID, int kills);

    public override void _Ready()
    {
        Instance = this;
    }

    public void RegisterKill(string mobID)
    {
        if (!kills.ContainsKey(mobID))
            kills[mobID] = 0;

        kills[mobID]++;

        EmitSignal(SignalName.KillUpdated, mobID, kills[mobID]);
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
}