using Godot;
using System.Collections.Generic;

/// <summary>
/// Zarządza pojawianiem się przeciwników na podstawie listy WaveDefinition.
/// Trudność rośnie z minutami gry — interwał spada, batch rośnie.
/// </summary>
public partial class EnemySpawner : Node2D
{
    // ── konfiguracja ──────────────────────────────────────────
    [Export] public TileMapLayer SpawnMap;
    [Export] public float SpawnRadius = 1200f;  // max odległość spawnu od gracza
    [Export] public float MinPlayerDistance = 400f;  // min — żeby nie spawnować na graczu

    /// <summary>Lista fal przypisana w Inspektorze (tablica WaveDefinition .tres).</summary>
    [Export] public Godot.Collections.Array<WaveDefinition> Waves = new();

    // ── stan wewnętrzny ───────────────────────────────────────
    private Player _player;
    private float _elapsed;          // czas gry w sekundach
    private List<Vector2> _spawnPool = new();

    // Jeden timer na falę — słownik Wave → Timer
    private Dictionary<WaveDefinition, Timer> _waveTimers = new();

    // ─────────────────────────────────────────────────────────
    public override void _Ready()
    {
        _player = GetTree().GetFirstNodeInGroup("player") as Player;
        BuildSpawnPool();

        // Uruchom timer sprawdzający co 10 s, które fale aktywować/dezaktywować
        var managementTimer = new Timer();
        managementTimer.WaitTime = 1.0f;
        managementTimer.Autostart = true;
        managementTimer.Timeout += UpdateActiveWaves;
        AddChild(managementTimer);
    }

    private float _cacheTimer = 0f;

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
    // ── zarządzanie falami ────────────────────────────────────

    /// <summary>Co sekundę sprawdza, które fale powinny być aktywne.</summary>
    private void UpdateActiveWaves()
    {
        float minute = _elapsed / 60f;
        GD.Print($"UpdateActiveWaves — elapsed: {_elapsed:F1}s, minute: {minute:F2}, waves count: {Waves.Count}");

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
                RefreshWaveInterval(wave); // aktualizuj interwał co sekundę
        }
    }

    private void ActivateWave(WaveDefinition wave)
    {
        GD.Print($"ActivateWave: {wave}, StartMinute: {wave.StartMinute}");
        var t = new Timer();
        t.WaitTime = GetCurrentInterval(wave);
        t.OneShot = false;
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
    /// Interwał maleje z czasem gry.
    /// Formuła: baseInterval / (1 + minuty * 0.15)  — miękkie skalowanie
    /// Minimum: 0.25 s, żeby nie zafloodować sceny.
    /// </summary>
    private float GetCurrentInterval(WaveDefinition wave)
    {
        float minute = _elapsed / 60f;
        float interval = wave.BaseInterval / (1f + minute * 0.15f);
        return Mathf.Max(0.25f, interval);
    }

    // ── spawn ─────────────────────────────────────────────────

    private void SpawnBatch(WaveDefinition wave)
    {
        if (wave.EnemyType?.Scene == null || _player == null) return;

        int minute = (int)(_elapsed / 60f);
        int batchSize = wave.BatchSize + minute / 1;

        GD.Print($"SpawnBatch: batchSize={batchSize}, nearbyCache={_nearbyCache.Count}");

        for (int i = 0; i < batchSize; i++)
        {
            Vector2 pos = GetSpawnPosition();
            GD.Print($"  pos={pos}");
            if (pos == Vector2.Zero)
            {
                GD.PrintErr("  SKIP: pos is Zero");
                continue;
            }

            var enemy = wave.EnemyType.Scene.Instantiate<Enemy>();
            GD.Print($"  instantiated: {enemy}");

            enemy.Stats = wave.EnemyType;
            enemy.GlobalPosition = pos;

            if (enemy.Stats != null)
            {
                int scaledHp = Mathf.RoundToInt(wave.EnemyType.MaxHealth * (1f + minute * 0.08f));
                enemy.MaxHealth = scaledHp;
                enemy.XpDrop = Mathf.CeilToInt(wave.EnemyType.XpDrop * (1f + minute * 0.05f));
            }

            GD.Print($"  adding to scene...");
            GetTree().CurrentScene.AddChild(enemy);
            GD.Print($"  DONE: Spawned {enemy.Name} at {pos}");
        }
    }

    private List<Vector2> _nearbyCache = new();
    private float _cacheRebuildTimer = 0f;

    private Vector2 GetSpawnPosition()
    {
        if (_spawnPool.Count == 0 || _player == null) return Vector2.Zero;

        // Przebuduj cache co 2 sekundy (gracz się porusza)
        _cacheRebuildTimer -= 0.016f; // przybliżone, można też wywołać w _Process
        if (_nearbyCache.Count == 0 || _cacheRebuildTimer <= 0f)
        {
            RebuildNearbyCache();
            _cacheRebuildTimer = 2.0f;
        }

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

        GD.Print($"NearbyCache: {_nearbyCache.Count} pozycji w zasięgu spawnu.");
    }

    // ── mapa spawnu ───────────────────────────────────────────

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
        GD.Print($"EnemySpawner: {_spawnPool.Count} pozycji spawnu.");
    }
}