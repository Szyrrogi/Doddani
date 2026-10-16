using UnityEngine;

public class Stats : MonoBehaviour
{
    public int Health = 100;
    public int MaxHealth = 100;

    public int Atack;
    public int LifeSteal;
    public int Armor;
    public int Regen;
    public int Doge;

    [Header("Energia")]
    public float EnergyMax = 100f;
    public float EnergyStart = 20f;
    public float EnegryPerSecond = 5f;
    public float Energy; 

    public int Fire;
    public int Water;
    public int Earth;
    public int Air;
    public int Magic;
    public float Crite;

    public int Level = 1;
    public int CurrentExp = 0;
    public int SkillPoints = 0;

    void Start()
    {
        Energy = EnergyStart; // Ustawienie początkowej energii
    }
    private float regenTimer = 0f;
    // NOWE: Skarbonka na ułamki HP
    private float skumulowaneLeczenie = 0f; 

    void Update()
    {
        // Pasywna regeneracja energii co klatkę (zostaje bez zmian)
        if (Energy < EnergyMax)
        {
            Energy += (EnegryPerSecond / 2f) * Time.deltaTime;
            if (Energy > EnergyMax)
            {
                Energy = EnergyMax;
            }
        }

        // Regeneracja zachodzi tylko, jeśli postać żyje, ma punkty Regen i nie ma pełnego HP
        if (Health > 0 && Health < MaxHealth && Regen > 0)
        {
            regenTimer += Time.deltaTime;

            if (regenTimer >= 2f) // Wykonaj co 2 sekundy
            {
                regenTimer = 0f;
                
                // Dorzucamy ułamek do skarbonki (np. 0.1 * 8 Regen = 0.8)
                skumulowaneLeczenie += 0.1f * Regen;
                
                // Jeśli w skarbonce jest 1 lub więcej
                if (skumulowaneLeczenie >= 1f)
                {
                    // Wyciągamy same pełne liczby (np. z 1.6 wyciągnie 1)
                    int pelnePunkty = Mathf.FloorToInt(skumulowaneLeczenie);
                    
                    Health += pelnePunkty;
                    skumulowaneLeczenie -= pelnePunkty; // Zostawiamy resztę (np. 1.6 - 1 = 0.6) na następny cykl
                    
                    if (Health > MaxHealth)
                    {
                        Health = MaxHealth;
                        skumulowaneLeczenie = 0f; // Resetujemy resztki, żeby nie przechodziły na kolejną walkę
                    }
                }
            }
        }
        else if (Health >= MaxHealth)
        {
            // Jeśli potwór wyleczy się np. czarem na maksa, czyścimy jego skarbonkę
            skumulowaneLeczenie = 0f;
        }
    }
    // Funkcja pobierająca energię (zwraca true jeśli się udało)
    public bool TrySpendEnergy(float amount)
    {
        if (Energy >= amount)
        {
            Energy -= amount;
            return true;
        }
        return false;
    }
}