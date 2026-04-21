using Godot;

/// <summary>
/// Klasa reprezentująca pojedynczy wpis przeciwnika w interfejsie użytkownika.
/// Wyświetla nazwę przeciwnika, ikonę, rangę (tier), liczbę zabójstw oraz pasek postępu do kolejnego poziomu.
/// </summary>
public partial class MobEntry : HBoxContainer
{
	/// <summary>
	/// Ustawia dane dla wpisu przeciwnika w UI.
	/// Pobiera teksturę ze sceny przeciwnika i aktualizuje statystyki oraz rangę rzymską.
	/// </summary>
	/// <param name="mobID">Identyfikator przeciwnika (nazwa sceny).</param>
	/// <param name="kills">Liczba zabójstw danego przeciwnika.</param>
	public void SetData(string mobID, int kills)
	{
		string scenePath = $"res://Scenes/Enemies/{mobID}.tscn";

		var mobScene = GD.Load<PackedScene>(scenePath);
		if (mobScene != null)
		{
			var mobInstance = mobScene.Instantiate<Node>();
			var sprite = mobInstance.GetNodeOrNull<Sprite2D>("Name");

			if (sprite != null)
			{
				GetNode<TextureRect>("MobIcon").Texture = sprite.Texture;
			}
			else if (mobInstance is Sprite2D rootSprite) 
			{
				GetNode<TextureRect>("MobIcon").Texture = rootSprite.Texture;
			}

			mobInstance.QueueFree();
		}
		
		int tierLevel = GetTierLevel(kills);
		string romanTier = ToRoman(tierLevel);
		
		// Wyświetla nazwę z rangą rzymską (np. "Skeleton II") lub samą nazwę jeśli tier wynosi 0
		string displayName = tierLevel == 0 ? mobID : $"{mobID.Capitalize()} {romanTier}";
		
		GetNode<Label>("MobInfo/MobName").Text = displayName;
		GetNode<Label>("MobInfo/KillCounter/CurrentKills").Text = kills.ToString();
		
		var descLabel = GetNodeOrNull<Label>("MobInfo/MobDescription");
		if (descLabel != null) descLabel.Text = GetMobDescription(mobID);
		
		int nextGoal = GetNextBestiaryGoal(kills);
		var nextTierLabel = GetNode<Label>("MobInfo/KillCounter/NextTierKills");
		var progressBar = GetNode<ProgressBar>("MobInfo/ProgressBar");
		
		if (nextGoal == -1) // Osiągnięto limit bestiariusza
		{
			nextTierLabel.Text = "MAX";
			progressBar.MaxValue = 25000;
			progressBar.Value = 25000;
		}
		else
		{
			nextTierLabel.Text = nextGoal.ToString();
			progressBar.MaxValue = nextGoal;
			progressBar.Value = kills;
		}
	}

	/// <summary>
	/// Oblicza kolejny próg zabójstw wymagany do awansu w bestiariuszu.
	/// </summary>
	/// <param name="kills">Aktualna liczba zabójstw.</param>
	/// <returns>Liczba zabójstw dla następnego progu lub -1, jeśli osiągnięto maksimum.</returns>
	private int GetNextBestiaryGoal(int kills)
	{
		if (kills < 500)   return ((kills / 50) + 1) * 50;   // Skok co 50
		if (kills < 5000)  return ((kills / 500) + 1) * 500; // Skok co 500
		if (kills < 25000) return ((kills / 2500) + 1) * 2500; // Skok co 2500
		return -1; 
	}

	/// <summary>
	/// Oblicza aktualny poziom rangi (tier) na podstawie liczby zabójstw.
	/// </summary>
	/// <param name="kills">Aktualna liczba zabójstw.</param>
	/// <returns>Numer poziomu rangi (0 dla braku rangi).</returns>
	private int GetTierLevel(int kills)
	{
		if (kills < 50) return 0;
		if (kills < 500) return kills / 50;           
		if (kills < 5000) return 10 + (kills / 500) - 1; 
		return 19 + (kills / 2500) - 2;               
	}

	/// <summary>
	/// Konwertuje numer poziomu na format cyfr rzymskich.
	/// </summary>
	/// <param name="number">Numer poziomu do konwersji.</param>
	/// <returns>String z reprezentacją rzymską lub numerem jako tekst dla wartości poza zakresem.</returns>
	private string ToRoman(int number)
	{
		if (number <= 0) return "";
		string[] romans = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", 
							"XI", "XII", "XIII", "XIV", "XV", "XVI", "XVII", "XVIII", "XIX", "XX" };
		
		return (number <= romans.Length) ? romans[number - 1] : number.ToString();
	}
	private string GetMobDescription(string mobID)
	{
		return mobID switch
		{
			"slime"    => "Najsłabszy przeciwnik. (Speed: 120, HP: 30, XP: 1)",
			"vampire"  => "Szybki, ale mało wytrzymały. (Speed: 200, HP: 18, XP: 1)",
			"skeleton" => "Solidny wojownik. (Speed: 140, HP: 80, XP: 3)",
			"demon"    => "Piekielna bestia. (Speed: 165, HP: 140, XP: 5)",
			"golem"    => "Powolny, ale potężny tank. (Speed: 70, HP: 350, XP: 8)",
			_          => "Nieznany przeciwnik."
		};
	}
}
