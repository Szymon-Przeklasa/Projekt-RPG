using Godot;
using System.Collections.Generic;

/// <summary>
/// Zarządza pojawianiem się przeciwników w grze na podstawie listy fal (WaveDefinition).
/// Zbalansowane dla 20-minutowej sesji: łatwy start, gwałtowny wzrost trudności od ~10 minuty.
/// Limit aktywnych przeciwników zapobiega za dużej ilości wrogów na ekranie.
/// </summary>
public partial class EnemySpawner : Node2D
{
	// ── Konfiguracja ──────────────────────────────────────────

	[Export] public TileMapLayer SpawnMap;
	[Export] public float SpawnRadius = 1200f;
	[Export] public float MinPlayerDistance = 400f;
	[Export] public Godot.Collections.Array<WaveDefinition> Waves = new();

	/// <summary>Maksymalna liczba aktywnych wrogów na scenie jednocześnie.</summary>
	[Export] public int MaxEnemies = 240;

	/// <summary>Maksymalna liczba XP orbów na scenie (starsze usuwane).</summary>
	[Export] public int MaxXpOrbs = 300;

	// ── Stan wewnętrzny ───────────────────────────────────────

	private Player _player;
	private float _elapsed;
	private List<Vector2> _spawnPool = new();
	private List<Vector2> _nearbyCache = new();
	private float _cacheTimer = 0f;
	private Dictionary<WaveDefinition, Timer> _waveTimers = new();
	private float _xpCleanupTimer = 0f;

	// ── Inicjalizacja ────────────────────────────────────────

	public override void _Ready()
	{
		_player = GetTree().GetFirstNodeInGroup("player") as Player;
		BuildSpawnPool();

		var managementTimer = new Timer { WaitTime = 1.0f, Autostart = true };
		managementTimer.Timeout += UpdateActiveWaves;
		AddChild(managementTimer);
	}

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

	// ── Zarządzanie falami ────────────────────────────────────

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

	private void ActivateWave(WaveDefinition wave)
	{
		var t = new Timer { WaitTime = GetCurrentInterval(wave), OneShot = false };
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
	/// Oblicza interwał spawnu z krzywą trudności:
	/// - 0–5 min: łagodne skalowanie
	/// - 5–10 min: umiarkowane
	/// - 10–20 min: agresywne (minimum 0.2s)
	/// </summary>
	private float GetCurrentInterval(WaveDefinition wave)
	{
		float minute = _elapsed / 60f;
		float multiplier;

		if (minute < 5f)
			multiplier = 1f - minute * 0.04f;           // -4% / min → 80% @ 5min
		else if (minute < 10f)
			multiplier = 0.8f - (minute - 5f) * 0.06f; // dodatkowe -6% / min → 50% @ 10min
		else
			multiplier = 0.5f - (minute - 10f) * 0.025f; // dodatkowe -2.5% / min → 25% @ 20min

		multiplier = Mathf.Max(0.15f, multiplier);
		return Mathf.Max(0.2f, wave.BaseInterval * multiplier);
	}

	// ── Spawnowanie przeciwników ──────────────────────────────

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
				float hpScale = 1f + minute * 0.06f;   // +6%/min → 2.2x @ 20min
				float xpScale = 1f + minute * 0.04f;   // +4%/min → 1.8x @ 20min
				enemy.MaxHealth = Mathf.RoundToInt(wave.EnemyType.MaxHealth * hpScale);
				enemy.XpDrop = Mathf.CeilToInt(wave.EnemyType.XpDrop * xpScale);
			}

			GetTree().CurrentScene.AddChild(enemy);
		}
	}

	/// <summary>
	/// Oblicza rozmiar batcha z wyraźną eskalacją w drugiej połowie gry.
	/// </summary>
	private int CalculateBatchSize(WaveDefinition wave, float minute)
	{
		int extra;
		if (minute < 5f)
			extra = 0;
		else if (minute < 10f)
			extra = Mathf.FloorToInt((minute - 5f) * 0.4f);  // +0.4/min
		else
			extra = 2 + Mathf.FloorToInt((minute - 10f) * 0.8f); // +0.8/min od 10 min

		return wave.BatchSize + extra;
	}

	private Vector2 GetSpawnPosition()
	{
		if (_spawnPool.Count == 0 || _player == null) return Vector2.Zero;
		if (_nearbyCache.Count == 0) RebuildNearbyCache();
		if (_nearbyCache.Count == 0) return Vector2.Zero;
		return _nearbyCache[(int)(GD.Randi() % (uint)_nearbyCache.Count)];
	}

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

	// ── Cleanup XP orbów ─────────────────────────────────────

	/// <summary>
	/// Usuwa nadmiarowe XP orby — te najdalej od gracza są usuwane pierwsze.
	/// Zapobiega gromadzeniu się setek niewidocznych orbów.
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