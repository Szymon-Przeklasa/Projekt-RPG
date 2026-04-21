using Godot;

/// <summary>
/// Panel autorów wyświetlany w menu pauzy.
/// </summary>
public partial class CreditsUI : HBoxContainer
{
	[Export] public string RepositoryUrl = "https://github.com/Szymon-Przeklasa/Projekt-RPG";

	private TextureButton _githubButton;

	public override void _Ready()
	{
		_githubButton = GetNodeOrNull<TextureButton>("RightColumn/GithubInfo/GithubIcon");
		if (_githubButton == null)
			return;

		_githubButton.TooltipText = RepositoryUrl;
		_githubButton.Pressed += OpenRepository;
	}

	private void OpenRepository()
	{
		if (!string.IsNullOrWhiteSpace(RepositoryUrl))
			OS.ShellOpen(RepositoryUrl);
	}
}
