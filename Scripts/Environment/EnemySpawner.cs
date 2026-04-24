using Godot;
using System.Collections.Generic;

/// <summary>
/// Klasa odpowiedzialna za zarządzanie pojawianiem się przeciwników w grze.
/// Na podstawie zdefiniowanych fal (<see cref="WaveDefinition"/>) aktywuje
/// odpowiednie typy wrogów w określonym czasie rozgrywki.
/// 
/// System zawiera:
/// <list type="bullet">
/// <item><description>dynamiczne skalowanie trudności wraz z czasem gry,</description></item>
/// <item><description>limit aktywnych przeciwników na scenie,</description></item>
/// <item><description>automatyczne usuwanie nadmiarowych orbów doświadczenia,</description></item>
/// <item><description>buforowanie punktów spawnu w pobliżu gracza.</description></item>
/// </list>
/// 
/// Klasa dziedziczy po <see cref="Node2D"/>.
/// </summary>
public partial class EnemySpawner : Node2D
{
	/// <summary>
	/// Warstwa mapy zawierająca komórki możliwego spawnu przeciwników.
	/// Każda użyta komórka zostaje przekształcona na globalny punkt spawnu.
	/// </summary>
	[Export] public TileMapLayer SpawnMap;
	/// <summary>
	/// Maksymalny promień od gracza, w którym przeciwnicy mogą się pojawić.
	/// </summary>
	[Export] public float SpawnRadius = 1200f;
	/// <summary>
	/// Minimalna odległość od gracza, w której przeciwnik może zostać zrespiony.
	/// Zapobiega pojawianiu się wrogów bezpośrednio obok gracza.
	/// </summary>
	[Export] public float MinPlayerDistance = 340f;
	/// <summary>
	/// Lista wszystkich fal przeciwników aktywowanych podczas gry.
	/// </summary>
	[Export] public Godot.Collections.Array<WaveDefinition> Waves = new();

	/// <summary>
	/// Maksymalna liczba aktywnych przeciwników na scenie jednocześnie.
	/// </summary>
	[Export] public int MaxEnemies = 360;

	/// <summary>
	/// Maksymalna liczba orbów doświadczenia na scenie.
	/// Po przekroczeniu starsze i najdalsze orby są usuwane.
	/// </summary>
	[Export] public int MaxXpOrbs = 300;

	// ── Stan wewnętrzny ───────────────────────────────────────

	private Player _player;
	private float _elapsed;
	private List<Vector2> _spawnPool = new();
	private List<Vector2> _nearbyCache = new();
	private float _cacheTimer = 0f;
	private Dictionary<WaveDefinition, Timer> _waveTimers = new();
	private float _xpCleanupTimer = 0f;

	/// <summary>
	/// Inicjalizuje system spawnów.
	/// Wyszukuje gracza, buduje pulę punktów spawnu oraz uruchamia timer
	/// odpowiedzialny za aktualizację aktywnych fal przeciwników.
	/// </summary>
	public override void _Ready()
	{
		_player = GetTree().GetFirstNodeInGroup("player") as Player;
		BuildSpawnPool();

		var managementTimer = new Timer { WaitTime = 1.0f, Autostart = true };
		managementTimer.Timeout += UpdateActiveWaves;
		AddChild(managementTimer);
	}

	/// <summary>
	/// Aktualizowana co klatkę metoda kontrolująca:
	/// <list type="bullet">
	/// <item><description>czas rozgrywki,</description></item>
	/// <item><description>odświeżanie cache punktów spawnu,</description></item>
	/// <item><description>czyszczenie nadmiarowych orbów XP.</description></item>
	/// </list>
	/// </summary>
	/// <param name="delta">Czas od poprzedniej klatki.</param>
	public override void _Process(double delta)
	{
		if (GetTree().Paused) return;

		_elapsed += (float)delta;

		_cacheTimer += (float)delta;
		if (_cacheTimer >= 2.0f)
		{
			RebuildNearbyCache();
			_cacheTimer = 0f;
		}

		_xpCleanupTimer += (float)delta;
		if (_xpCleanupTimer >= 5.0f)
		{
			CleanupExcessXpOrbs();
			_xpCleanupTimer = 0f;
		}
	}

	/// <summary>
	/// Aktualizuje stan wszystkich fal przeciwników.
	/// Aktywuje, dezaktywuje lub odświeża interwały spawnów
	/// na podstawie aktualnego czasu gry.
	/// </summary>
	private void UpdateActiveWaves()
	{
		if (GetTree().Paused) return;

		float minute = _elapsed / 60f;

		foreach (var wave in Waves)
		{
			bool shouldBeActive = minute >= wave.StartMinute
								  && (wave.EndMinute <= 0f || minute < wave.EndMinute);
			bool isActive = _waveTimers.ContainsKey(wave);

			if (shouldBeActive && !isActive)
				ActivateWave(wave);
			else if (!shouldBeActive && isActive)
				DeactivateWave(wave);
			else if (shouldBeActive && isActive)
				RefreshWaveInterval(wave);
		}
	}

	/// <summary>
	/// Aktywuje falę przeciwników tworząc timer,
	/// który cyklicznie wywołuje metodę <see cref="SpawnBatch"/>.
	/// </summary>
	/// <param name="wave">Fala do aktywacji.</param>

	private void ActivateWave(WaveDefinition wave)
	{
		var t = new Timer { WaitTime = GetCurrentInterval(wave), OneShot = false };
		t.Timeout += () => SpawnBatch(wave);
		AddChild(t);
		t.Start();
		_waveTimers[wave] = t;
	}

	/// <summary>
	/// Dezaktywuje aktywną falę przeciwników
	/// i usuwa przypisany do niej timer.
	/// </summary>
	/// <param name="wave">Fala do dezaktywacji.</param>
	private void DeactivateWave(WaveDefinition wave)
	{
		if (_waveTimers.TryGetValue(wave, out var t))
		{
			t.Stop();
			t.QueueFree();
			_waveTimers.Remove(wave);
		}
	}

	/// <summary>
	/// Aktualizuje częstotliwość spawnu aktywnej fali
	/// zgodnie z aktualnym poziomem trudności.
	/// </summary>
	/// <param name="wave">Fala do aktualizacji.</param>
	private void RefreshWaveInterval(WaveDefinition wave)
	{
		if (_waveTimers.TryGetValue(wave, out var t))
			t.WaitTime = GetCurrentInterval(wave);
	}

	/// <summary>
	/// Oblicza aktualny interwał spawnu przeciwników
	/// na podstawie czasu rozgrywki.
	/// Trudność wzrasta stopniowo wraz z postępem gry.
	/// </summary>
	/// <param name="wave">Definicja fali.</param>
	/// <returns>Nowy czas pomiędzy spawnami.</returns>
	private float GetCurrentInterval(WaveDefinition wave)
	{
		float minute = _elapsed / 60f;
		float multiplier;

		if (minute < 5f)
			multiplier = 1f - minute * 0.05f;              // 75% @ 5min
		else if (minute < 10f)
			multiplier = 0.75f - (minute - 5f) * 0.05f;    // 50% @ 10min
		else
			multiplier = 0.50f - (minute - 10f) * 0.022f;  // 28% @ 20min

		multiplier = Mathf.Max(0.22f, multiplier);
		return Mathf.Max(0.25f, wave.BaseInterval * multiplier);
	}

	/// <summary>
	/// Tworzy grupę przeciwników dla wskazanej fali.
	/// Uwzględnia:
	/// <list type="bullet">
	/// <item><description>limit przeciwników,</description></item>
	/// <item><description>skalowanie zdrowia i XP,</description></item>
	/// <item><description>szansę na elitarnych przeciwników.</description></item>
	/// </list>
	/// </summary>
	/// <param name="wave">Fala przeciwników do wygenerowania.</param>
	private void SpawnBatch(WaveDefinition wave)
	{
		if (wave.EnemyType?.Scene == null || _player == null) return;

		// Ogranicz całkowitą liczbę wrogów
		int currentEnemies = GetTree().GetNodesInGroup("enemies").Count;
		if (currentEnemies >= MaxEnemies) return;

		float minute = _elapsed / 60f;
		int batchSize = CalculateBatchSize(wave, minute);

		// Nie przekracz limitu
		batchSize = Mathf.Min(batchSize, MaxEnemies - currentEnemies);
		if (batchSize <= 0) return;

		for (int i = 0; i < batchSize; i++)
		{
			Vector2 pos = GetSpawnPosition();
			if (pos == Vector2.Zero) continue;

			var enemy = wave.EnemyType.Scene.Instantiate<Enemy>();
			enemy.Stats = wave.EnemyType;
			enemy.GlobalPosition = pos;

			if (enemy.Stats != null)
			{
				// Skalowanie HP i XP z czasem — łagodniejsza krzywa
				float hpScale = 1f + minute * 0.04f;   // +4%/min → 1.8x @ 20min
				float xpScale = 1f + minute * 0.06f;   // +6%/min → 2.2x @ 20min
				enemy.MaxHealth = Mathf.RoundToInt(wave.EnemyType.MaxHealth * hpScale);
				enemy.XpDrop = Mathf.CeilToInt(wave.EnemyType.XpDrop * xpScale);
			}

			float eliteRoll  = GD.Randf();
			float eliteChance = GetEliteChance();
			if (eliteRoll < eliteChance * 0.08f)         // ~8% of elite slots → Legendary
				enemy.Rank = EliteRank.Legendary;
			else if (eliteRoll < eliteChance)
				enemy.Rank = EliteRank.Elite;

			GetTree().CurrentScene.AddChild(enemy);
		}
	}

	/// <summary>
	/// Oblicza szansę na pojawienie się elitarnego przeciwnika
	/// na podstawie czasu rozgrywki.
	/// </summary>
	/// <returns>Szansa z zakresu 0.0 - 0.20.</returns>
	private float GetEliteChance()
	{
		float minute = _elapsed / 60;
		return Mathf.Min(0.02f + (minute * 0.005f), 0.20f); // 5% od 5min, max 20%
	}

	/// <summary>
	/// Oblicza liczbę przeciwników w pojedynczym batchu
	/// zależnie od czasu rozgrywki.
	/// </summary>
	/// <param name="wave">Fala przeciwników.</param>
	/// <param name="minute">Aktualny czas gry w minutach.</param>
	/// <returns>Liczba przeciwników do stworzenia.</returns>
	private int CalculateBatchSize(WaveDefinition wave, float minute)
	{
		int extra;
		if (minute < 2.5f)
			extra = 0;
		else if (minute < 5f)
			extra = 1;
		else if (minute < 8f)
			extra = 1 + Mathf.FloorToInt((minute - 5f) * 0.35f);
		else if (minute < 14f)
			extra = 2 + Mathf.FloorToInt((minute - 8f) * 0.5f);
		else
			extra = 5 + Mathf.FloorToInt((minute - 14f) * 0.65f);

		return wave.BatchSize + extra;
	}

	/// <summary>
	/// Zwraca losową pozycję spawnu znajdującą się
	/// w poprawnym zakresie odległości od gracza.
	/// </summary>
	/// <returns>Pozycja spawnu lub <see cref="Vector2.Zero"/>.</returns>
	private Vector2 GetSpawnPosition()
	{
		if (_spawnPool.Count == 0 || _player == null) return Vector2.Zero;
		if (_nearbyCache.Count == 0) RebuildNearbyCache();
		if (_nearbyCache.Count == 0) return Vector2.Zero;
		return _nearbyCache[(int)(GD.Randi() % (uint)_nearbyCache.Count)];
	}

	/// <summary>
	/// Odbudowuje cache punktów spawnu znajdujących się
	/// w odpowiedniej odległości od gracza.
	/// </summary>
	private void RebuildNearbyCache()
	{
		_nearbyCache.Clear();
		float minSq = MinPlayerDistance * MinPlayerDistance;
		float maxSq = SpawnRadius * SpawnRadius;

		foreach (Vector2 pos in _spawnPool)
		{
			float distSq = _player.GlobalPosition.DistanceSquaredTo(pos);
			if (distSq >= minSq && distSq <= maxSq)
				_nearbyCache.Add(pos);
		}
	}

	/// <summary>
	/// Usuwa nadmiarowe orby doświadczenia.
	/// Najpierw usuwane są te najdalej położone od gracza.
	/// </summary>
	private void CleanupExcessXpOrbs()
	{
		if (_player == null) return;

		var orbs = GetTree().GetNodesInGroup("xp");
		if (orbs.Count <= MaxXpOrbs) return;

		// Posortuj od najdalszych do gracza
		var orbList = new List<(Node2D orb, float distSq)>();
		foreach (Node node in orbs)
		{
			if (node is Node2D orb2d)
			{
				float dSq = _player.GlobalPosition.DistanceSquaredTo(orb2d.GlobalPosition);
				orbList.Add((orb2d, dSq));
			}
		}

		orbList.Sort((a, b) => b.distSq.CompareTo(a.distSq)); // najdalsze pierwsze

		int toRemove = orbs.Count - MaxXpOrbs;
		for (int i = 0; i < toRemove && i < orbList.Count; i++)
			orbList[i].orb.QueueFree();
	}

	/// <summary>
	/// Buduje listę wszystkich możliwych punktów spawnu
	/// na podstawie komórek warstwy <see cref="SpawnMap"/>.
	/// </summary>
	private void BuildSpawnPool()
	{
		_spawnPool.Clear();
		if (SpawnMap == null) { GD.PushWarning("EnemySpawner: brak SpawnMap!"); return; }

		foreach (Vector2I cell in SpawnMap.GetUsedCells())
		{
			Vector2 localPos = SpawnMap.MapToLocal(cell);
			Vector2 globalPos = SpawnMap.ToGlobal(localPos);
			_spawnPool.Add(globalPos);
		}
	}
}
