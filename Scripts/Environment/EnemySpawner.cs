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
    [Export] public float SpawnRadius = 9000f;  // max odległość spawnu od gracza
    [Export] public float MinPlayerDistance = 5f;  // min — żeby nie spawnować na graczu

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

    public override void _Process(double delta)
    {
        _elapsed += (float)delta;
    }

    // ── zarządzanie falami ────────────────────────────────────

    /// <summary>Co sekundę sprawdza, które fale powinny być aktywne.</summary>
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
                RefreshWaveInterval(wave); // aktualizuj interwał co sekundę
        }
    }

    private void ActivateWave(WaveDefinition wave)
    {
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

        // Batch rośnie co 5 minut
        int minute = (int)(_elapsed / 60f);
        int batchSize = wave.BatchSize + minute / 1;

        for (int i = 0; i < batchSize; i++)
        {
            Vector2 pos = GetSpawnPosition();
            if (pos == Vector2.Zero) continue;

            var enemy = wave.EnemyType.Scene.Instantiate<Enemy>();
            enemy.Stats = wave.EnemyType;
            enemy.GlobalPosition = pos;

            // Skalowanie HP z czasem gry: +8% HP na minutę
            if (enemy.Stats != null)
            {
                int scaledHp = Mathf.RoundToInt(wave.EnemyType.MaxHealth * (1f + minute * 0.08f));
                enemy.MaxHealth = scaledHp;

                // XP rośnie wolniej niż HP — +5% na minutę, zaokrąglone w górę
                enemy.XpDrop = Mathf.CeilToInt(wave.EnemyType.XpDrop * (1f + minute * 0.05f));
            }

            GetTree().CurrentScene.AddChild(enemy);
            GD.Print($"Spawned {enemy.Name} at {pos}");
        }
    }

    private Vector2 GetSpawnPosition()
    {
        if (_spawnPool.Count == 0) return Vector2.Zero;

        // Szukamy miejsca w odpowiedniej odległości od gracza
        const int Attempts = 15;
        for (int i = 0; i < Attempts; i++)
        {
            var pos = _spawnPool[(int)(GD.Randi() % (uint)_spawnPool.Count)];
            float dist = _player.GlobalPosition.DistanceTo(pos);

            if (dist >= MinPlayerDistance && dist <= SpawnRadius)
                return pos;
        }

        // Fallback — losowy kafelek poza min-odległością
        var fallback = _spawnPool[(int)(GD.Randi() % (uint)_spawnPool.Count)];
        return fallback;
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