using Godot;
using System;

public partial class KillsUI : CanvasLayer
{
	// Called when the node enters the scene tree for the first time.

	[Export]
	public PackedScene MobEntryScene;

	public override void _Ready()
	{
		Visible = false;

		var killManager = GetNode<KillManager>("/root/KillManager");

		killManager.KillUpdated += OnKillUpdated;
	}

	private void OnKillUpdated(string mobID, int kills)
	{
		GD.Print($"{mobID} kills: {kills}");
	}

	public void ShowKills()
	{
		Visible = true;
		GetTree().Paused = true;

		var mobgroup = GetNode<VBoxContainer>("Panel/VBoxContainer/MobGroup");

		foreach (Node child in mobgroup.GetChildren())
			child.QueueFree();

		foreach (var pair in KillManager.Instance.GetAllKills())
		{
			var entry = MobEntryScene.Instantiate<MobEntry>();

			entry.SetData(pair.Key, pair.Value);

			mobgroup.AddChild(entry);
		}
	}
	private void OnBackgroundClicked(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent &&
			mouseEvent.Pressed &&
			mouseEvent.ButtonIndex == MouseButton.Left)
		{
			Close();
		}
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void Close()
	{
		Visible = false;
		GetTree().Paused = false;
	}
}
