using Godot;

/// <summary>
/// Klasa bazowa pocisku w grze.
/// Odpowiada za ruch, kolizję oraz zadawanie obrażeń przeciwnikom.
/// </summary>
public partial class Projectile : Area2D
{
	/// <summary>
	/// Kierunek ruchu pocisku (znormalizowany wektor).
	/// </summary>
	protected Vector2 Direction;

	/// <summary>
	/// Referencja do statystyk broni, z której pochodzi pocisk.
	/// </summary>
	protected WeaponStats Stats;

	/// <summary>
	/// Liczba przeciwników, przez których pocisk może jeszcze przejść.
	/// </summary>
	protected int PierceLeft;

	/// <summary>
	/// Obrażenia obliczone w momencie wystrzału (runtime).
	/// </summary>
	protected int RuntimeDamage;

	/// <summary>
	/// Prędkość pocisku obliczona w momencie wystrzału (runtime).
	/// </summary>
	protected float RuntimeSpeed;

	/// <summary>
	/// Nazwa broni, która wystrzeliła pocisk (do debugowania / logów).
	/// </summary>
	protected string SourceWeapon = "Projectile";

	/// <summary>
	/// Maksymalny dystans, jaki pocisk może przebyć.
	/// </summary>
	protected float MaxTravelDistance;

	private float _travelledDistance;

	/// <summary>
	/// Inicjalizuje pocisk.
	/// Pozwala nadpisać obrażenia i prędkość (np. po uwzględnieniu mnożników gracza).
	/// </summary>
	/// <param name="dir">Kierunek ruchu pocisku.</param>
	/// <param name="stats">Statystyki broni.</param>
	/// <param name="damage">Opcjonalne obrażenia (jeśli -1 → użyj bazowych).</param>
	/// <param name="speed">Opcjonalna prędkość (jeśli -1 → użyj bazowej).</param>
	/// <param name="weaponName">Nazwa źródłowej broni.</param>
	public void Setup(Vector2 dir, WeaponStats stats, int damage = -1, float speed = -1, string weaponName = "Projectile")
	{
		Direction = dir;
		Stats = stats;
		_travelledDistance = 0f;

		PierceLeft = stats.Pierce;

		RuntimeDamage = damage < 0 ? stats.Damage : damage;
		RuntimeSpeed = speed < 0 ? stats.Speed : speed;
		MaxTravelDistance = Mathf.Max(40f, stats.Range);

		SourceWeapon = weaponName;
	}

	/// <summary>
	/// Rejestruje zdarzenie kolizji po utworzeniu pocisku.
	/// </summary>
	public override void _Ready()
	{
		BodyEntered += OnHit;
	}

	/// <summary>
	/// Aktualizuje pozycję pocisku w każdej klatce fizyki.
	/// </summary>
	public override void _PhysicsProcess(double delta)
	{
		Advance(Direction * RuntimeSpeed * (float)delta);
	}

	protected bool Advance(Vector2 movement)
	{
		GlobalPosition += movement;
		_travelledDistance += movement.Length();

		if (_travelledDistance >= MaxTravelDistance)
		{
			QueueFree();
			return false;
		}

		return true;
	}

	/// <summary>
	/// Obsługuje trafienie w obiekt.
	/// Jeśli trafiony obiekt to przeciwnik:
	/// - zadaje obrażenia,
	/// - aplikuje knockback,
	/// - zmniejsza liczbę przebić (pierce),
	/// - usuwa pocisk, jeśli nie ma już przebić.
	/// </summary>
	/// <param name="body">Obiekt, z którym nastąpiła kolizja.</param>
	protected virtual void OnHit(Node body)
	{
		if (body is Enemy enemy)
		{
			enemy.TakeDamage(RuntimeDamage, Direction * Stats.Knockback, SourceWeapon);

			PierceLeft--;

			if (PierceLeft <= 0)
				QueueFree();
		}
	}
}
