using Godot;

/// <summary>
/// Proste okno podglądu dostępnych power-upów z menu głównego.
/// </summary>
public partial class PowerUpsUI : CanvasLayer
{
	private Button _closeButton;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Visible = false;

		_closeButton = GetNode<Button>("Panel/VBoxContainer/CloseButton");
		_closeButton.Pressed += Close;
	}

	public void ShowPanel()
	{
		Visible = true;
		_closeButton.GrabFocus();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!Visible)
			return;

		if (@event.IsActionPressed("ui_cancel"))
		{
			Close();
			GetViewport().SetInputAsHandled();
		}
	}

	private void OnBackgroundInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent &&
			mouseEvent.Pressed &&
			mouseEvent.ButtonIndex == MouseButton.Left)
		{
			Close();
		}
	}

	private void Close()
	{
		Visible = false;
	}
}
