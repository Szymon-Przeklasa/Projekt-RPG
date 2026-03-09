using Godot;
using System.Collections.Generic;

public partial class KillManager : Node
{
	private Dictionary<string, int> kills = new();

	[Signal]
	public delegate void KillUpdatedEventHandler(string mobID, int kills);

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
}
