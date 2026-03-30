/// <summary>
/// Typy pasywnych ulepszeń dostępnych w grze.
/// Każdy typ odpowiada za modyfikację konkretnej statystyki gracza.
/// </summary>
public enum PassiveType
{
    Spinach,       // Zwiększa obrażenia (Damage +)
    Pummarola,     // Skraca czas odnowienia (Cooldown -)
    HollowHeart,   // Zwiększa obszar działania (Area +)
    Bracer,        // Zwiększa prędkość pocisków (Speed +)
    Wings          // Zwiększa prędkość ruchu (MoveSpeed +)
}