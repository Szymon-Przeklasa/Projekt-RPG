using Godot;

/// <summary>
/// Panel wyświetlający informacje o autorach projektu
/// w menu pauzy gry.
///
/// Umożliwia użytkownikowi otwarcie repozytorium projektu
/// w domyślnej przeglądarce systemowej.
///
/// Klasa dziedziczy po <see cref="HBoxContainer"/>.
/// </summary>
public partial class CreditsUI : HBoxContainer
{
	/// <summary>
	/// Adres URL repozytorium projektu.
	/// Wyświetlany jako podpowiedź po najechaniu na ikonę GitHub.
	/// </summary>
	[Export] public string RepositoryUrl = "https://github.com/Szymon-Przeklasa/Projekt-RPG";

	/// <summary>
	/// Przycisk ikony GitHub otwierający repozytorium.
	/// </summary>
	private TextureButton _githubButton;

	/// <summary>
	/// Inicjalizuje panel po dodaniu do sceny.
	/// Pobiera referencję do przycisku GitHub oraz
	/// przypisuje zdarzenie otwierające repozytorium.
	/// </summary>
	public override void _Ready()
	{
		_githubButton = GetNodeOrNull<TextureButton>("RightColumn/GithubInfo/GithubIcon");
		if (_githubButton == null)
			return;

		_githubButton.TooltipText = RepositoryUrl;
		_githubButton.Pressed += OpenRepository;
	}

	/// <summary>
	/// Otwiera adres repozytorium w domyślnej przeglądarce systemu.
	/// </summary>
	private void OpenRepository()
	{
		if (!string.IsNullOrWhiteSpace(RepositoryUrl))
			OS.ShellOpen(RepositoryUrl);
	}
}
