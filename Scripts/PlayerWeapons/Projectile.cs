using Godot;

/// <summary>
/// Klasa bazowa pocisku w grze.
/// Odpowiada za ruch liniowy, wykrywanie kolizji z wrogami oraz zadawanie obrażeń.
/// Pocisk usuwa się automatycznie po przebyciu odległości równej <see cref="MaxTravelDistance"/>
/// lub po wyczerpaniu <see cref="PierceLeft"/> trafień.
/// Klasy pochodne (np. <see cref="MagicMissileProjectile"/>, <see cref="AxeProjectile"/>)
/// mogą nadpisać <see cref="_PhysicsProcess"/> w celu implementacji niestandardowej trajektorii.
/// </summary>
public partial class Projectile : Area2D
{
    /// <summary>
    /// Znormalizowany kierunek ruchu pocisku.
    /// Może być modyfikowany przez klasy pochodne (np. naprowadzanie).
    /// </summary>
    protected Vector2 Direction;

    /// <summary>
    /// Referencja do statystyk broni, z której pochodzi pocisk.
    /// Używana do odczytu siły odrzutu (<see cref="WeaponStats.Knockback"/>)
    /// i zasięgu (<see cref="WeaponStats.Range"/>).
    /// </summary>
    protected WeaponStats Stats;

    /// <summary>
    /// Pozostała liczba wrogów, przez których pocisk może jeszcze przejść (przebicie).
    /// Zmniejszana przy każdym trafieniu; po osiągnięciu 0 pocisk się usuwa.
    /// </summary>
    protected int PierceLeft;

    /// <summary>
    /// Obrażenia obliczone w momencie wystrzału z uwzględnieniem mnożników gracza.
    /// Przekazywane do <see cref="Enemy.TakeDamage"/> przy każdym trafieniu.
    /// </summary>
    protected int RuntimeDamage;

    /// <summary>
    /// Prędkość pocisku obliczona w momencie wystrzału z uwzględnieniem mnożników gracza (piksele/s).
    /// </summary>
    protected float RuntimeSpeed;

    /// <summary>
    /// Nazwa broni, która wystrzeliła pocisk.
    /// Przekazywana do <see cref="Enemy.TakeDamage"/> w celach logowania i debugowania.
    /// </summary>
    protected string SourceWeapon = "Projectile";

    /// <summary>
    /// Maksymalna odległość (w jednostkach świata), jaką pocisk może przebyć przed usunięciem.
    /// Wyznaczana na podstawie <see cref="WeaponStats.Range"/> przy wywołaniu <see cref="Setup"/>.
    /// </summary>
    protected float MaxTravelDistance;

    /// <summary>
    /// Łączna odległość przebyta przez pocisk od chwili spawnu.
    /// Porównywana z <see cref="MaxTravelDistance"/> po każdym wywołaniu <see cref="Advance"/>.
    /// </summary>
    private float _travelledDistance;

    /// <summary>
    /// Inicjalizuje pocisk z podanymi parametrami.
    /// Powinno być wywoływane bezpośrednio po zainstancjonowaniu, przed dodaniem do sceny.
    /// Umożliwia nadpisanie obrażeń i prędkości (np. po zastosowaniu mnożników gracza).
    /// </summary>
    /// <param name="dir">Znormalizowany kierunek początkowy ruchu pocisku.</param>
    /// <param name="stats">Statystyki broni zawierające bazowe parametry pocisku.</param>
    /// <param name="damage">Obrażenia do użycia; jeśli -1, używane są <see cref="WeaponStats.Damage"/>.</param>
    /// <param name="speed">Prędkość do użycia; jeśli -1, używana jest <see cref="WeaponStats.Speed"/>.</param>
    /// <param name="weaponName">Nazwa broni źródłowej (do logów i identyfikacji).</param>
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
    /// Wywoływana po dodaniu węzła do drzewa sceny.
    /// Subskrybuje sygnał <c>BodyEntered</c>, aby reagować na kolizje z wrogami.
    /// </summary>
    public override void _Ready()
    {
        BodyEntered += OnHit;
    }

    /// <summary>
    /// Aktualizacja fizyki wywoływana każdą klatką.
    /// Przesuwa pocisk liniowo w kierunku <see cref="Direction"/> z prędkością <see cref="RuntimeSpeed"/>.
    /// Klasy pochodne mogą nadpisać tę metodę, aby implementować niestandardową trajektorię.
    /// </summary>
    /// <param name="delta">Czas od poprzedniej klatki fizyki (sekundy).</param>
    public override void _PhysicsProcess(double delta)
    {
        Advance(Direction * RuntimeSpeed * (float)delta);
    }

    /// <summary>
    /// Przesuwa pocisk o podany wektor i aktualizuje przebyty dystans.
    /// Jeśli dystans przekroczy <see cref="MaxTravelDistance"/>, pocisk się usuwa.
    /// </summary>
    /// <param name="movement">Wektor przemieszczenia do zastosowania w bieżącej klatce.</param>
    /// <returns><c>true</c> jeśli pocisk nadal istnieje; <c>false</c> jeśli został usunięty.</returns>
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
    /// Obsługuje kolizję pocisku z innym ciałem.
    /// Jeśli trafiony obiekt to <see cref="Enemy"/>:
    /// <list type="bullet">
    ///   <item><description>Zadaje obrażenia (<see cref="RuntimeDamage"/>) z odrzutem w kierunku lotu.</description></item>
    ///   <item><description>Zmniejsza <see cref="PierceLeft"/> o 1.</description></item>
    ///   <item><description>Usuwa pocisk, gdy <see cref="PierceLeft"/> osiągnie 0.</description></item>
    /// </list>
    /// Klasy pochodne mogą nadpisać tę metodę, aby dodać specjalne efekty przy trafieniu.
    /// </summary>
    /// <param name="body">Węzeł, z którym nastąpiła kolizja fizyczna.</param>
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