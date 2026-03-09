using Godot;

public partial class MobEntry : HBoxContainer
{
	public void SetData(string mobID, int kills)
	{
		GetNode<Label>("MobInfo/MobName").Text = mobID;

		GetNode<Label>("MobInfo/KillCounter/CurrentKills").Text = kills.ToString();

		GetNode<ProgressBar>("MobInfo/ProgressBar").Value = kills;
	}
}
