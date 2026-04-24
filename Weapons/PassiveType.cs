/// <summary>
/// Typy pasywnych ulepszeń dostępnych w grze.
/// Każdy typ odpowiada za modyfikację konkretnej statystyki gracza.
/// </summary>
public enum PassiveType
{
    /// <summary>
    /// Spinach – zwiększa obrażenia zadawane przez gracza.
    /// </summary>
    Spinach,

    /// <summary>
    /// Pummarola – skraca czas odnowienia ataków (cooldown).
    /// </summary>
    Pummarola,

    /// <summary>
    /// Hollow Heart – zwiększa obszar działania (area/rozmiar efektów).
    /// </summary>
    HollowHeart,

    /// <summary>
    /// Bracer – zwiększa prędkość pocisków.
    /// </summary>
    Bracer,

    /// <summary>
    /// Wings – zwiększa prędkość poruszania się gracza.
    /// </summary>
    Wings
}