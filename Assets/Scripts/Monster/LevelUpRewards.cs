using UnityEngine;
using System.Collections.Generic;

// 1. Klasa pomocnicza dla pojedynczej statystyki (np. "S.Atack" -> 10)
[System.Serializable]
public class StatReward
{
    [Tooltip("Wpisz np. S.Atack, S.MaxHealth, S.EnergyMax itp.")]
    public string statName;
    public int wartosc;
}

// 2. Klasa nagrody za konkretny poziom
[System.Serializable]
public class LevelReward
{
    public int targetLevel; // Poziom, na którym przyznajemy nagrodę
    
    [Header("Bonusy do statystyk (po stringach)")]
    public List<StatReward> statRewards = new List<StatReward>();
    
    [Header("Inne")]
    public int addSkillPoints;
    // public SpellData newSpell; // Miejsce na ewentualne nowe zaklęcie
}

public class LevelUpRewards : MonoBehaviour
{
    public List<LevelReward> rewards;

    public void PrzyznajNagrody(MonsterSaveData data, Stats statsNaScenie)
    {
        foreach (var nagroda in rewards)
        {
            if (nagroda.targetLevel == data.level)
            {
                // 1. Dodajemy punkty umiejętności
                data.skillPoints += nagroda.addSkillPoints;

                // 2. Tłumaczymy wszystkie stringi na bonusy do statystyk
                foreach (var stat in nagroda.statRewards)
                {
                    ZastosujNagrode(data, stat.statName, stat.wartosc);
                }

                // 3. Jeśli potwór stoi na planszy, wgrywamy mu nowe statystyki do modelu
                if (statsNaScenie != null)
                {
                    data.LoadToStats(statsNaScenie);
                }

                Debug.Log($"<color=cyan>AWANS!</color> {data.monsterName} osiągnął {data.level} poziom!");
            }
        }
    }

    private void ZastosujNagrode(MonsterSaveData data, string statName, int wartosc)
    {
        switch (statName)
        {
            case "S.Health":
            case "S.MaxHealth":
                data.maxHealth += wartosc;
                data.health += wartosc; // Zawsze warto podleczyć o zyskane MaxHP
                break;
            case "S.Atack":
                data.atack += wartosc;
                break;
            case "S.Armor":
                data.armor += wartosc;
                break;
            case "S.LifeSteal":
                data.lifeSteal += wartosc;
                break;
            case "S.Regen":
                data.regen += wartosc;
                break;
            case "S.Doge":
                data.doge += wartosc;
                break;
            case "S.Fire":
                data.fire += wartosc;
                break;
            case "S.Water":
                data.water += wartosc;
                break;
            case "S.Earth":
                data.earth += wartosc;
                break;
            case "S.Air":
                data.air += wartosc;
                break;
            case "S.Magic":
                data.magic += wartosc;
                break;
            case "S.EnergyMax":
                data.energyMax += wartosc;
                break;
            case "S.EnergyStart":
                data.energyStart += wartosc;
                break;
            case "S.EnergyPerSecond":
                data.energyPerSecond += wartosc;
                break;
            default:
                Debug.LogWarning($"<color=red>BŁĄD:</color> Nie rozpoznano statystyki o nazwie: '{statName}'");
                break;
        }
    }
}