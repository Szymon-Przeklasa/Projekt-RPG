using Godot;

/// <summary>
/// Klasa reprezentująca pocisk topora (<see cref="Axe"/>) z łukową trajektorią lotu.
/// Symuluje grawitację oraz unoszenie się pocisku, dzięki czemu porusza się po krzywej balistycznej.
/// Pocisk obraca się wokół własnej osi przez cały czas lotu.
/// Dziedziczy po klasie <see cref="Projectile"/>.
/// </summary>
public partial class AxeProjectile : Projectile
{
	/// <summary>
	/// Wewnętrzna prędkość pocisku, modyfikowana przez grawitację każdej klatki.
	/// </summary>
	private Vector2 _velocity;

	/// <summary>
	/// Flaga inicjalizacji prędkości startowej — obliczana tylko raz przy pierwszej klatce fizyki.
	/// </summary>
	private bool _initialized;

	/// <summary>
	/// Siła grawitacji działająca na pocisk (jednostki/s²).
	/// Zwiększa składową Y prędkości z każdą klatką.
	/// </summary>
	private const float GravityForce = 850f;

	/// <summary>
	/// Współczynnik unoszenia pocisku w osi Y przy wystrzale.
	/// Nadaje mu łukowy tor lotu zamiast prostoliniowego.
	/// </summary>
	private const float LiftFactor = 0.35f;

	/// <summary>
	/// Prędkość kątowa obrotu topora wokół własnej osi (radiany/s).
	/// </summary>
	private const float SpinSpeed = 10f;

	/// <summary>
	/// Aktualizacja fizyki pocisku wywoływana każdą klatką.
	/// Przy pierwszym wywołaniu inicjalizuje prędkość startową z uwzględnieniem unoszenia.
	/// Następnie stosuje grawitację do składowej pionowej, aktualizuje kierunek lotu
	/// i obraca pocisk wokół własnej osi. Na końcu przesuwa pocisk metodą
	/// <see cref="Projectile.Advance"/>.
	/// </summary>
	/// <param name="delta">Czas od poprzedniej klatki fizyki (sekundy).</param>
	public override void _PhysicsProcess(double delta)
	{
		if (!_initialized)
		{
			_velocity = Direction * RuntimeSpeed;
			_velocity.Y -= RuntimeSpeed * LiftFactor;
			_initialized = true;
		}

		_velocity.Y += GravityForce * (float)delta;
		if (_velocity.LengthSquared() > 0.001f)
			Direction = _velocity.Normalized();

		Rotation += SpinSpeed * (float)delta;
		Advance(_velocity * (float)delta);
	}
}
