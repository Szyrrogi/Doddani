using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MonsterDetailsUI : MonoBehaviour
{
    [Header("Główny Panel")]
    public GameObject mainPanel;

    [Header("Prawa Strona - Lista")]
    public RectTransform rosterContainer; 
    public GameObject rosterPrefab;       

    [Header("Dół - Statystyki")]
    public GameObject statsPanel; 
    public Image wybranikIcon; 
    
    [Header("Teksty Statystyk")]
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text expText;
    public TMP_Text skillPointsText;
    public TMP_Text healthText;
    public TMP_Text attackText;
    public TMP_Text dodgeText;
    public TMP_Text regenText;
    public TMP_Text lifeStealText;
    public TMP_Text critText;
    public TMP_Text waterText;
    public TMP_Text fireText;
    public TMP_Text magicText;
    public TMP_Text earthText;
    public TMP_Text airText;
    public TMP_Text armorText;

    [Header("Ekwipunek - Sloty (Nad Statystykami)")]
    public RectTransform equippedSlotsContainer; 
    public GameObject equippedSlotPrefab; 
    public GameObject MenuExit;

    [Header("Ekwipunek - Magazyn (Góra)")]
    public GameObject inventoryPanel; 
    public RectTransform inventoryContainer; 
    public GameObject inventoryItemPrefab; 

    public MonsterSaveData currentSelectedMonster;
    public SkillTreeManager skillTreeManager; // Podepnij w Unity!



    void Start()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    public void PrzelaczMenu()
    {
        if (mainPanel != null)
        {
            bool stan = !mainPanel.activeSelf;
            mainPanel.SetActive(stan);

            if (stan)
            {
                if (inventoryPanel != null) inventoryPanel.SetActive(false);
                OdswiezListe();
                
                if (PlayerManager.Instance != null && PlayerManager.Instance.playerMonsters.Count > 0)
                    WybierzPotwora(PlayerManager.Instance.playerMonsters[0]);
                else if (statsPanel != null) statsPanel.SetActive(false);
            }
        }
    }

    private void OdswiezListe()
    {
        if (PlayerManager.Instance == null) return;

        foreach (Transform child in rosterContainer) Destroy(child.gameObject);

        foreach (var monster in PlayerManager.Instance.playerMonsters)
        {
            GameObject btnObj = Instantiate(rosterPrefab, rosterContainer);
            btnObj.GetComponent<Image>().sprite = GetFaceArt(monster.monsterName);
            btnObj.GetComponent<Button>().onClick.AddListener(() => WybierzPotwora(monster));
        }
    }

    public void WybierzPotwora(MonsterSaveData monster)
    {
        currentSelectedMonster = monster;
        if (statsPanel != null) statsPanel.SetActive(true);

        // ==========================================
        // 1. ZLICZANIE BONUSÓW Z PRZEDMIOTÓW (Zabezpieczone)
        // ==========================================
        int bMaxHealth = 0, bAtack = 0, bArmor = 0, bMagic = 0;
        int bDodge = 0, bRegen = 0, bLifeSteal = 0, bCrit = 0;
        int bWater = 0, bFire = 0, bEarth = 0, bAir = 0;

        if (monster.equippedItems != null)
        {
            foreach (var item in monster.equippedItems)
            {
                // Zabezpieczenie: JSON mógł wygenerować puste pola zamiast ScriptableObjects
                if (item == null || item.itemStats == null) continue; 

                foreach (var stat in item.itemStats)
                {
                    switch (stat.statName)
                    {
                        case "S.Health":
                        case "S.MaxHealth": bMaxHealth += stat.wartosc; break;
                        case "S.Atack": bAtack += stat.wartosc; break;
                        case "S.Armor": bArmor += stat.wartosc; break;
                        case "S.LifeSteal": bLifeSteal += stat.wartosc; break;
                        case "S.Regen": bRegen += stat.wartosc; break;
                        case "S.Doge": bDodge += stat.wartosc; break;
                        case "S.Fire": bFire += stat.wartosc; break;
                        case "S.Water": bWater += stat.wartosc; break;
                        case "S.Earth": bEarth += stat.wartosc; break;
                        case "S.Air": bAir += stat.wartosc; break;
                        case "S.Magic": bMagic += stat.wartosc; break;
                        case "S.Crite": bCrit += stat.wartosc; break;
                    }
                }
            }
        }

        // ==========================================
        // 2. AKTUALIZACJA TEKSTÓW BAZOWYCH (Zabezpieczone)
        // ==========================================
        string bezpiecznaNazwa = string.IsNullOrEmpty(monster.monsterName) ? "Nieznany Potwór" : monster.monsterName.Replace("(Clone)", "").Trim();
        if (nameText != null) nameText.text = bezpiecznaNazwa;
        
        if (levelText != null) levelText.text = $"Poz. {monster.level}";
        if (expText != null) expText.text = $"EXP: {monster.currentExp} / {ExperienceManager.WymaganyExp(monster.level)}";

        if (skillPointsText != null) skillPointsText.text = $"SP: {monster.skillPoints}";

        // ==========================================
        // 3. WYSWIETLANIE STATYSTYK Z KOLORAMI
        // ==========================================
        if (healthText != null) healthText.text = FormatujZdrowie(monster.health, monster.maxHealth, bMaxHealth);
        if (attackText != null) attackText.text = FormatujStatystyke("ATACK", monster.atack, bAtack);
        if (armorText != null) armorText.text = FormatujStatystyke("ARMOR", monster.armor, bArmor);
        if (magicText != null) magicText.text = FormatujStatystyke("MAGIC", monster.magic, bMagic);
        
        if (dodgeText != null) dodgeText.text = FormatujStatystyke("DODGE", monster.doge, bDodge);
        if (regenText != null) regenText.text = FormatujStatystyke("REGEN", monster.regen, bRegen);
        if (lifeStealText != null) lifeStealText.text = FormatujStatystyke("LIFESTEAL", monster.lifeSteal, bLifeSteal);
        if (critText != null) critText.text = FormatujStatystyke("CRYTE", (int)monster.crite, bCrit); 

        if (waterText != null) waterText.text = FormatujStatystyke("WATHER", monster.water, bWater);
        if (fireText != null) fireText.text = FormatujStatystyke("FIRE", monster.fire, bFire);
        if (earthText != null) earthText.text = FormatujStatystyke("EARTH", monster.earth, bEarth);
        if (airText != null) airText.text = FormatujStatystyke("WIND", monster.air, bAir);

        if (wybranikIcon != null) wybranikIcon.sprite = GetFaceArt(bezpiecznaNazwa);

        OdswiezSlotyZalozone();
    }

    // ==========================================
    // FUNKCJE POMOCNICZE DO FORMATOWANIA
    // ==========================================
    private string FormatujStatystyke(string spriteName, int totalValue, int bonus)
    {
        if (bonus > 0)
            return $"<sprite name=\"{spriteName}\"> <color=#1e9c3b>{totalValue} (+{bonus})</color>";
        else if (bonus < 0)
            return $"<sprite name=\"{spriteName}\"> <color=#FF5555>{totalValue} ({bonus})</color>";
        else
            return $"<sprite name=\"{spriteName}\"> {totalValue}";
    }

    private string FormatujZdrowie(float currentHealth, int totalMaxHealth, int bonus)
    {
        if (bonus > 0)
            return $"<sprite name=\"HEALTH\"> <color=#1e9c3b>{currentHealth}/{totalMaxHealth} (+{bonus})</color>";
        else if (bonus < 0)
            return $"<sprite name=\"HEALTH\"> <color=#FF5555>{currentHealth}/{totalMaxHealth} ({bonus})</color>";
        else
            return $"<sprite name=\"HEALTH\"> {currentHealth}/{totalMaxHealth}";
    }

    // ==========================================
    // SYSTEM PRZEDMIOTÓW
    // ==========================================
    public void OtworzEkwipunek()
    {
        if (currentSelectedMonster == null || inventoryPanel == null) return;
        
        bool stan = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(stan);
        
        if (stan) OdswiezMagazyn();
    }

    private void OdswiezSlotyZalozone()
    {
        foreach (Transform child in equippedSlotsContainer) Destroy(child.gameObject);

        for (int i = 0; i < currentSelectedMonster.itemSlots; i++)
        {
            GameObject slotObj = Instantiate(equippedSlotPrefab, equippedSlotsContainer);
            Image iconImg = slotObj.transform.GetChild(0).GetComponent<Image>(); 
            Button btn = slotObj.GetComponent<Button>();

            if (i < currentSelectedMonster.equippedItems.Count)
            {
                ItemData zalozonyPrzedmiot = currentSelectedMonster.equippedItems[i];
                iconImg.sprite = zalozonyPrzedmiot.itemIcon; 
                iconImg.color = Color.white;
                
                btn.onClick.AddListener(() => ZdejmijPrzedmiot(zalozonyPrzedmiot));
            }
            else
            {
                iconImg.sprite = null;
                iconImg.color = new Color(0, 0, 0, 0); 
                btn.interactable = false; 
            }
        }
    }

    private void OdswiezMagazyn()
    {
        foreach (Transform child in inventoryContainer) Destroy(child.gameObject);

        foreach (var item in PlayerManager.Instance.playerItems)
        {
            GameObject btnObj = Instantiate(inventoryItemPrefab, inventoryContainer);
            btnObj.GetComponent<Image>().sprite = item.itemIcon; 
            
            btnObj.GetComponent<Button>().onClick.AddListener(() => ZalozPrzedmiot(item));
        }
    }

    private void ZalozPrzedmiot(ItemData item)
    {
        if (currentSelectedMonster.ZalozPrzedmiot(item)) 
        {
            PlayerManager.Instance.playerItems.Remove(item); 
            WybierzPotwora(currentSelectedMonster); 
            OdswiezMagazyn(); 
        }
    }

    private void ZdejmijPrzedmiot(ItemData item)
    {
        if (inventoryPanel != null && !inventoryPanel.activeSelf) return;

        currentSelectedMonster.ZdejmijPrzedmiot(item);
        PlayerManager.Instance.playerItems.Add(item); 
        
        WybierzPotwora(currentSelectedMonster); 
        OdswiezMagazyn();
    }

    private Sprite GetFaceArt(string monsterName)
    {
        string cleanName = monsterName.Replace("(Clone)", "").Trim();
        foreach (var prefab in PlayerManager.Instance.bazaWszystkichPotworow) // Zmiana na global
        {
            if (prefab != null && prefab.name.Replace("(Clone)", "").Trim() == cleanName)
            {
                PlayerBattle pb = prefab.GetComponent<PlayerBattle>();
                if (pb != null) return pb.faceArt;
            }
        }
        return null; 
    }

    public void OtworzDrzewkoUmiejetnosci()
    {
        if (currentSelectedMonster == null || skillTreeManager == null) return;

        // Jeśli drzewko było otwarte, zamknij je i przywróć przycisk wyjścia
        if (skillTreeManager.gameObject.activeSelf)
        {
            skillTreeManager.ZamknijDrzewko();
            if (MenuExit != null) MenuExit.SetActive(true); // Przywraca przycisk
            return;
        }

        // Jeśli drzewko było zamknięte, ukryj przycisk i otwórz drzewko
        if (MenuExit != null) MenuExit.SetActive(false); // Ukrywa przycisk
        
        GameObject treePrefab = null;
        string cleanName = currentSelectedMonster.monsterName.Replace("(Clone)", "").Trim();
        
        foreach (var prefab in PlayerManager.Instance.bazaWszystkichPotworow) 
        {
            if (prefab != null && prefab.name.Replace("(Clone)", "").Trim() == cleanName)
            {
                PlayerBattle pb = prefab.GetComponent<PlayerBattle>();
                if (pb != null) treePrefab = pb.skillTreePrefab;
                break;
            }
        }

        skillTreeManager.OtworzDrzewko(currentSelectedMonster, treePrefab);
    }
}