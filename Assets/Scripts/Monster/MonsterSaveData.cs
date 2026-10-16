using UnityEngine;
using System.Collections.Generic; // Wymagane do obsługi List<>

[System.Serializable]
public class MonsterSaveData
{
    public string monsterName; 

    [Header("Postęp")]
    public int level = 1; 
    public int currentExp = 0; 
    public int skillPoints = 0; 
    public List<string> unlockedSkills = new List<string>();

    [Header("Statystyki")]
    public int health; 
    public int maxHealth; 
    public int atack; 
    public int lifeSteal; 
    public int armor; 
    public int regen; 
    public int doge; 
    
    public float energyMax; 
    public float energyStart; 
    public float energyPerSecond; 

    public int fire; 
    public int water; 
    public int earth; 
    public int air; 
    public int magic; 
    public float crite; 

    [Header("Ekwipunek")]
    public int itemSlots = 2; // Domyślna ilość slotów na przedmioty
    public List<ItemData> equippedItems = new List<ItemData>();
    // Dodaj to obok statystyk zdrowia, ataku itp.
    public List<StatusData> przygotowaneStatusy = new List<StatusData>();

    // ==========================================
    // ZAPIS I ODCZYT STATYSTYK (Z SCENY DO PAMIĘCI)
    // ==========================================

    public void SaveFromStats(Stats statsObj, string name) 
    {
        monsterName = name; 
        health = statsObj.Health; 
        maxHealth = statsObj.MaxHealth; 
        atack = statsObj.Atack; 
        lifeSteal = statsObj.LifeSteal; 
        armor = statsObj.Armor; 
        regen = statsObj.Regen; 
        doge = statsObj.Doge; 
        energyMax = statsObj.EnergyMax; 
        energyStart = statsObj.EnergyStart; 
        energyPerSecond = statsObj.EnegryPerSecond; 
        fire = statsObj.Fire; 
        water = statsObj.Water; 
        earth = statsObj.Earth; 
        air = statsObj.Air; 
        magic = statsObj.Magic; 
        crite = statsObj.Crite; 

        level = statsObj.Level; 
        currentExp = statsObj.CurrentExp; 
        skillPoints = statsObj.SkillPoints; 
    }

    public void LoadToStats(Stats statsObj) 
    {
        // 1. Zliczamy bonusy z ekwipunku
        int bHealth = 0, bAtack = 0, bArmor = 0, bMagic = 0;
        int bDoge = 0, bRegen = 0, bLifeSteal = 0;
        float bCrite = 0f;
        int bWater = 0, bFire = 0, bEarth = 0, bAir = 0;
        float bEnergyMax = 0f, bEnergyStart = 0f, bEnergyPerSec = 0f;

        foreach (var item in equippedItems)
        {
            // ZABEZPIECZENIE: Ignorujemy puste obiekty wygenerowane przez JSON!
            if (item == null || item.itemStats == null) continue; 

            foreach (var stat in item.itemStats)
            {
                switch (stat.statName)
                {
                    case "S.Health":
                    case "S.MaxHealth": bHealth += stat.wartosc; break;
                    case "S.Atack": bAtack += stat.wartosc; break;
                    // ... reszta Twoich case'ów zostaje bez zmian ...
                    case "S.Armor": bArmor += stat.wartosc; break;
                    case "S.LifeSteal": bLifeSteal += stat.wartosc; break;
                    case "S.Regen": bRegen += stat.wartosc; break;
                    case "S.Doge": bDoge += stat.wartosc; break;
                    case "S.Fire": bFire += stat.wartosc; break;
                    case "S.Water": bWater += stat.wartosc; break;
                    case "S.Earth": bEarth += stat.wartosc; break;
                    case "S.Air": bAir += stat.wartosc; break;
                    case "S.Magic": bMagic += stat.wartosc; break;
                    case "S.Crite": bCrite += stat.wartosc; break;
                    case "S.EnergyMax": bEnergyMax += stat.wartosc; break;
                    case "S.EnergyStart": bEnergyStart += stat.wartosc; break;
                    case "S.EnergyPerSecond": bEnergyPerSec += stat.wartosc; break;
                }
            }
        }

        // 2. Wlewamy bazę + bonusy do Stats
        statsObj.MaxHealth = maxHealth + bHealth; 
        statsObj.Health = health; // Obecne zdrowie zostawiamy bez zmian (nie uleczy się do pełna)
        statsObj.Atack = atack + bAtack; 
        statsObj.LifeSteal = lifeSteal + bLifeSteal; 
        statsObj.Armor = armor + bArmor; 
        statsObj.Regen = regen + bRegen; 
        statsObj.Doge = doge + bDoge; 
        statsObj.EnergyMax = energyMax + bEnergyMax; 
        statsObj.EnergyStart = energyStart + bEnergyStart; 
        statsObj.EnegryPerSecond = energyPerSecond + bEnergyPerSec; 
        statsObj.Fire = fire + bFire; 
        statsObj.Water = water + bWater; 
        statsObj.Earth = earth + bEarth; 
        statsObj.Air = air + bAir; 
        statsObj.Magic = magic + bMagic; 
        statsObj.Crite = crite + bCrite; 

        // 3. Poziomy
        statsObj.Level = level; 
        statsObj.CurrentExp = currentExp; 
        statsObj.SkillPoints = skillPoints; 
    }

    // ==========================================
    // SYSTEM EKWIPUNKU
    // ==========================================

    // Funkcja wywoływana, gdy chcemy założyć przedmiot
    public bool ZalozPrzedmiot(ItemData newItem)
    {
        if (equippedItems.Count >= itemSlots)
        {
            Debug.LogWarning($"<color=orange>Brak wolnych slotów!</color> {monsterName} nie może założyć: {newItem.itemName}");
            return false; // Przerwij, jeśli nie ma miejsca
        }

        equippedItems.Add(newItem);
        
        // Dodajemy statystyki z przedmiotu do potwora
        foreach (var stat in newItem.itemStats)
        {
            ZmienStatystyke(stat.statName, stat.wartosc);
        }
        
        Debug.Log($"<color=green>ZALOŻONO:</color> {newItem.itemName} na {monsterName}.");
        return true; // Sukces
    }

    // Funkcja wywoływana, gdy chcemy zdjąć przedmiot
    public void ZdejmijPrzedmiot(ItemData itemToRemove)
    {
        if (equippedItems.Contains(itemToRemove))
        {
            equippedItems.Remove(itemToRemove);
            
            // Odejmujemy statystyki z przedmiotu (przekazując wartość z minusem)
            foreach (var stat in itemToRemove.itemStats)
            {
                ZmienStatystyke(stat.statName, -stat.wartosc);
            }
            
            Debug.Log($"<color=yellow>ZDJĘTO:</color> {itemToRemove.itemName} z {monsterName}.");
        }
    }

    // Prywatny mechanizm zmieniający statystyki na podstawie nazwy
    private void ZmienStatystyke(string statName, int wartosc)
    {
        switch (statName)
        {
            case "S.Health":
            case "S.MaxHealth":
                maxHealth += wartosc;
                health += wartosc; 
                // Zabezpieczenie: jeśli zdejmujemy przedmiot dodający życie, HP nie może spaść poniżej 1
                if (health <= 0) health = 1; 
                break;
            case "S.Atack": atack += wartosc; break;
            case "S.Armor": armor += wartosc; break;
            case "S.LifeSteal": lifeSteal += wartosc; break;
            case "S.Regen": regen += wartosc; break;
            case "S.Doge": doge += wartosc; break;
            case "S.Fire": fire += wartosc; break;
            case "S.Water": water += wartosc; break;
            case "S.Earth": earth += wartosc; break;
            case "S.Air": air += wartosc; break;
            case "S.Magic": magic += wartosc; break;
            default:
                Debug.LogWarning($"Nie rozpoznano statystyki z przedmiotu: {statName}");
                break;
        }
    }
}