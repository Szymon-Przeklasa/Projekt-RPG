using Godot;

/// <summary>
/// Singleton zarządzający efektami dźwiękowymi.
/// Dodaj jako Autoload w Godot: Project → Project Settings → Autoload
/// Nazwa: SoundManager, Ścieżka: res://Scripts/SoundManager.cs
/// </summary>
public partial class SoundManager : Node
{
    public static SoundManager Instance;

    private AudioStreamPlayer _sfxPlayer1;
    private AudioStreamPlayer _sfxPlayer2;

    // Załadowane streamy
    private AudioStream _hit1;
    private AudioStream _hit2;
    private AudioStream _hurt;
    private AudioStream _levelUp;
    private AudioStream _loot;
    private AudioStream _heal;

    public override void _Ready()
    {
        Instance = this;

        // Dwa playery — żeby hit1/hit2 mogły się nakładać z innymi SFX
        _sfxPlayer1 = new AudioStreamPlayer();
        _sfxPlayer2 = new AudioStreamPlayer();
        AddChild(_sfxPlayer1);
        AddChild(_sfxPlayer2);

        _hit1    = Load("res://Sounds/Effects/hit1.mp3");
        _hit2    = Load("res://Sounds/Effects/hit2.mp3");
        _hurt    = Load("res://Sounds/Effects/hurt.mp3");
        _levelUp = Load("res://Sounds/Effects/levelup.mp3");
        _loot    = Load("res://Sounds/Effects/loot.mp3");
        _heal    = Load("res://Sounds/Effects/heal.mp3");
    }

    private AudioStream Load(string path)
    {
        if (!ResourceLoader.Exists(path))
        {
            GD.PushWarning($"SoundManager: brak pliku {path}");
            return null;
        }
        return ResourceLoader.Load<AudioStream>(path);
    }

    // ── Publiczne metody ─────────────────────────────────────

    /// <summary>Losowo hit1 lub hit2 — dla trafień przeciwnika.</summary>
    public void PlayHit()
    {
        Play(GD.Randi() % 2 == 0 ? _hit1 : _hit2, _sfxPlayer1);
    }

    public void PlayHurt()    => Play(_hurt,    _sfxPlayer2);
    public void PlayLevelUp() => Play(_levelUp, _sfxPlayer2);
    public void PlayLoot()    => Play(_loot,    _sfxPlayer1);
    public void PlayHeal()    => Play(_heal,    _sfxPlayer2);

    // ── Wewnętrzne ───────────────────────────────────────────

    private void Play(AudioStream stream, AudioStreamPlayer player)
    {
        if (stream == null || player == null) return;
        player.Stream = stream;
        player.Play();
    }
}