using Godot;
using System.Collections.Generic;

/// <summary>
/// Zarządza pojawianiem się przeciwników w grze na podstawie listy fal (WaveDefinition).
/// Interwał spawnów i liczba przeciwników w batchu rośnie z czasem gry, co powoduje progresywną trudność.
/// </summary>
public partial class EnemySpawner : Node2D
{
	// ── Konfiguracja ──────────────────────────────────────────
	
	/// <summary>Mapa TileMap wskazująca możliwe pozycje spawnu.</summary>
	[Export] public TileMapLayer SpawnMap;

	/// <summary>Maksymalny promień spawnu wokół gracza.</summary>
	[Export] public float SpawnRadius = 1200f;

	/// <summary>Minimalna odległość od gracza, żeby nie spawnować wroga bezpośrednio na nim.</summary>
	[Export] public float MinPlayerDistance = 400f;

	/// <summary>Lista fal przypisana w Inspektorze (tablica WaveDefinition .tres).</summary>
	[Export] public Godot.Collections.Array<WaveDefinition> Waves = new();

	// ── Stan wewnętrzny ───────────────────────────────────────

	private Player _player;
	private float _elapsed;                     // czas gry w sekundach
	private List<Vector2> _spawnPool = new();   // wszystkie możliwe pozycje spawnu
	private List<Vector2> _nearbyCache = new(); // pozycje w zasięgu spawnowania od gracza
	private float _cacheTimer = 0f;            // licznik odświeżania cache
	private Dictionary<WaveDefinition, Timer> _waveTimers = new(); // timery dla aktywnych fal

	// ── Inicjalizacja ────────────────────────────────────────

	public override void _Ready()
	{
		_player = GetTree().GetFirstNodeInGroup("player") as Player;

		BuildSpawnPool();

		// Timer zarządzający aktywacją/dezaktywacją fal co sekundę
		var managementTimer = new Timer
		{
			WaitTime = 1.0f,
			Autostart = true
		};
		managementTimer.Timeout += UpdateActiveWaves;
		AddChild(managementTimer);
	}

	public override void _Process(double delta)
	{
		_elapsed += (float)delta;

		_cacheTimer += (float)delta;
		if (_cacheTimer >= 2.0f)
		{
			RebuildNearbyCache();
			_cacheTimer = 0f;
		}
	}

	// ── Zarządzanie falami ────────────────────────────────────

	/// <summary>
	/// Sprawdza, które fale powinny być aktywne i włącza/dezaktywuje je.
	/// Wywoływane przez Timer co sekundę.
	/// </summary>
	private void UpdateActiveWaves()
	{
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

	private void ActivateWave(WaveDefinition wave)
	{
		var t = new Timer
		{
			WaitTime = GetCurrentInterval(wave),
			OneShot = false
		};
		t.Timeout += () => SpawnBatch(wave);
		AddChild(t);
		t.Start();
		_waveTimers[wave] = t;
	}

	private void DeactivateWave(WaveDefinition wave)
	{
		if (_waveTimers.TryGetValue(wave, out var t))
		{
			t.Stop();
			t.QueueFree();
			_waveTimers.Remove(wave);
		}
	}

	private void RefreshWaveInterval(WaveDefinition wave)
	{
		if (_waveTimers.TryGetValue(wave, out var t))
			t.WaitTime = GetCurrentInterval(wave);
	}

	/// <summary>
	/// Oblicza aktualny interwał spawnu w zależności od minuty gry.
	/// Interwał maleje wraz z upływem czasu gry.
	/// </summary>
	private float GetCurrentInterval(WaveDefinition wave)
	{
		float minute = _elapsed / 60f;
		float interval = wave.BaseInterval / (1f + minute * 0.15f);
		return Mathf.Max(0.25f, interval);
	}

	// ── Spawnowanie przeciwników ──────────────────────────────

	private void SpawnBatch(WaveDefinition wave)
	{
		if (wave.EnemyType?.Scene == null || _player == null) return;

		int minute = (int)(_elapsed / 60f);
		int batchSize = wave.BatchSize + minute / 1; // Skalowanie liczby przeciwników z czasem gry

		for (int i = 0; i < batchSize; i++)
		{
			Vector2 pos = GetSpawnPosition();
			if (pos == Vector2.Zero) continue;

			var enemy = wave.EnemyType.Scene.Instantiate<Enemy>();
			enemy.Stats = wave.EnemyType;
			enemy.GlobalPosition = pos;

			// Skalowanie parametrów przeciwnika z upływem czasu
			if (enemy.Stats != null)
			{
				enemy.MaxHealth = Mathf.RoundToInt(wave.EnemyType.MaxHealth * (1f + minute * 0.08f));
				enemy.XpDrop = Mathf.CeilToInt(wave.EnemyType.XpDrop * (1f + minute * 0.05f));
			}

			GetTree().CurrentScene.AddChild(enemy);
		}
	}

	/// <summary>
	/// Losowa pozycja spawnu z pobliskiego cache’u (odległość od gracza w [MinPlayerDistance, SpawnRadius]).
	/// </summary>
	private Vector2 GetSpawnPosition()
	{
		if (_spawnPool.Count == 0 || _player == null) return Vector2.Zero;
		if (_nearbyCache.Count == 0) RebuildNearbyCache();
		if (_nearbyCache.Count == 0) return Vector2.Zero;

		return _nearbyCache[(int)(GD.Randi() % (uint)_nearbyCache.Count)];
	}

	/// <summary>
	/// Przebudowuje cache pozycji spawnów w zasięgu gracza.
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

	// ── Przygotowanie puli spawnu z TileMap ───────────────────

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
