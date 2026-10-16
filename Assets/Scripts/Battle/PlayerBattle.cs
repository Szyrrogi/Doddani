using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class PlayerBattle : MonoBehaviour
{
    [Header("Grafiki Potwora")]
    public SpriteRenderer monsterGraphic; 
    public Sprite frontArt; 
    public Sprite backArt;  
    public Sprite faceArt;

    [Header("Statystyki Postaci")]
    public Stats playerStats; 

    [Header("UI Statystyk (Tekst)")]
    public TMP_Text healthText; 
    public TMP_Text energyText; 
    public TMP_Text levelText; // NOWE: Tekst Levelu

    [Header("UI Paski (Skrypty)")]
    public HealthBar healthBarScript; 
    public HealthBar energyBarScript; 
    public HealthBar expBarScript; // NOWE: Pasek EXP (używamy tego samego skryptu co do zdrowia!)

    [Header("Główne obiekty UI (do ukrywania)")]
    public GameObject energyUIContainer; 

    [Header("Baza do spawnowania")]
    public GameObject baseSpellPrefab; 
    public GameObject skillTreePrefab;

    public List<SpellData> availableSpells; 
    public int spellsToSpawnOnStart = 3;  

    private bool isInBattle = false;
    private bool isDead = false; 

    // Referencja do oryginalnej pamięci potwora, by odczytywać Level i EXP
    private MonsterSaveData mySaveData;

    // NOWA FUNKCJA: Wywoływana przez BattleManagera przy spawnowaniu
    public void WymusPrzypisanieDanych(MonsterSaveData data)
    {
        mySaveData = data;
        Debug.Log($"<color=cyan>[PLAYER BATTLE]</color> Połączono statystyki z zapisu dla: {data.monsterName} (Lvl {data.level})");
    }

    void Start()
    {   
        // NOWE: Tworzymy lokalną kopię listy zaklęć na tę jedną walkę[cite: 8]
        if (availableSpells != null)
        {
            availableSpells = new List<SpellData>(availableSpells);
        }

        if (BattleManager.Instance != null)
        // ... reszta Twojego kodu Start() zostaje bez zmian ...
        {
            // JESTEŚMY W WALCE - dane zostaną wymuszone przez BattleManagera
            isInBattle = true;
            if (monsterGraphic != null && backArt != null) monsterGraphic.sprite = backArt;
            if (energyUIContainer != null) energyUIContainer.SetActive(true);
            SpawnInitialSpells(spellsToSpawnOnStart); 
        }
        else
        {
            // JESTEŚMY W LESIE (Podgląd)
            isInBattle = false;
            if (monsterGraphic != null && frontArt != null) monsterGraphic.sprite = frontArt;
            if (energyUIContainer != null) energyUIContainer.SetActive(false);
            else 
            {
                if (energyText != null) energyText.gameObject.SetActive(false);
                if (energyBarScript != null) energyBarScript.gameObject.SetActive(false);
            }

            // --- NOWE: Skoro nikt nam nie wgra danych z zewnątrz, musimy znaleźć je sami ---
            if (mySaveData == null && PlayerManager.Instance != null)
            {
                string cleanName = gameObject.name.Replace("(Clone)", "").Trim();
                foreach (var data in PlayerManager.Instance.activeParty)
                {
                    if (data.monsterName.Replace("(Clone)", "").Trim() == cleanName)
                    {
                        mySaveData = data;
                        break;
                    }
                }
            }
        }
    }

    void Update()
    {
        if (playerStats == null) return; 

        if (playerStats.Health <= 0 && !isDead)
        {
            Zgin();
        }

        if (!isDead)
        {
            UpdateUI(); 
        }
    }


    // 1. Dodajemy zmienne do zapamiętywania stanu z poprzedniej klatki
    private float poprzednieHP = -1f;
    private float poprzedniaEnergia = -1f;
    private int poprzedniExp = -1;

    private void UpdateUI()
    {
        // 2. UI Zdrowia aktualizuje się TYLKO gdy wartość faktycznie uległa zmianie
        if (playerStats.Health != poprzednieHP)
        {
            poprzednieHP = playerStats.Health;
            // ZMIANA: :F0 ucina wszystko po przecinku, pokazując czyste liczby całkowite
            if (healthText != null) healthText.text = $"{playerStats.Health:F0}/{playerStats.MaxHealth:F0}"; 
            if (healthBarScript != null) healthBarScript.UpdateBar(playerStats.Health, playerStats.MaxHealth);
        }

        // 3. UI Energii (zmienia się często z racji regeneracji)
        if (isInBattle && playerStats.Energy != poprzedniaEnergia)
        {
            poprzedniaEnergia = playerStats.Energy;
            // ZMIANA: Energia również bez miejsc po przecinku
            if (energyText != null) energyText.text = $"{playerStats.Energy:F0}/{playerStats.EnergyMax:F0}"; 
            if (energyBarScript != null) energyBarScript.UpdateBar(playerStats.Energy, playerStats.EnergyMax);
        }

        // 4. UI EXPa (Zostaje bez zmian, aktualizuje sam pasek i tekst levelu)
        if (mySaveData != null && mySaveData.currentExp != poprzedniExp)
        {
            poprzedniExp = mySaveData.currentExp;
            
            playerStats.Level = mySaveData.level;
            playerStats.CurrentExp = mySaveData.currentExp;
            playerStats.SkillPoints = mySaveData.skillPoints;

            if (levelText != null) levelText.text = $"Lvl {mySaveData.level}";
            if (expBarScript != null)
            {
                float maxExp = ExperienceManager.WymaganyExp(mySaveData.level);
                expBarScript.UpdateBar(mySaveData.currentExp, maxExp);
            }
        }
    }

    private void Zgin()
    {
        isDead = true;
        Debug.Log($"<color=red>[ŚMIERĆ]</color> {gameObject.name} został pokonany!");

        if (mySaveData != null)
        {
            // Oznaczamy jako martwego, ale NIE ZERUJEMY tu EXPa.
            // EXP wyzeruje się tylko wtedy, gdy cała drużyna zginie (w BattleEndManager).
            mySaveData.health = 0;     
        }
        
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (mySaveData != null && playerStats != null)
        {
            // Zapisujemy zachowane HP tylko jeśli potwór przetrwał bitwę
            if (!isDead)
            {
                mySaveData.health = (int)playerStats.Health;
            }
            
            // USUNIĘTO NADPISYWANIE POSTĘPU!
            // Teraz to Menedżer Walki i Ekran Wygranej w pełni kontrolują dodawanie SP i resetowanie EXPa.
            // Potwór znikając ze sceny nie będzie już cofał ich pracy.
        }
    }

    // (Tu zostaw funkcje od Spawnowania zaklęć tak jak miałeś)
    public void SpawnInitialSpells(int spellCount) 
    {
        for (int i = 0; i < spellCount; i++) 
        {
            float targetY = 6f + (i * 1.8f); 
            Vector3 spawnPos = new Vector3(0, targetY, 0); 
            SpawnSingleRandomSpell(spawnPos); 
        }
    }

    public void SpawnSingleRandomSpell(Vector3 localPosition) 
    {
        if (availableSpells == null || availableSpells.Count == 0) return; 
        if (baseSpellPrefab == null) return; 

        int randomIndex = Random.Range(0, availableSpells.Count); 
        SpellData selectedData = availableSpells[randomIndex]; 

        // NOWE: Jeśli czar jest jednorazowy, usuwamy go z puli od razu przy dobieraniu!
        // Dzięki temu nigdy nie wylosuje się drugi raz na ekranie.
        if (selectedData.jednorazowy)
        {
            availableSpells.RemoveAt(randomIndex);
        }

        GameObject newSpellObj = Instantiate(baseSpellPrefab, transform); 
        newSpellObj.transform.localPosition = localPosition; 

        Spell spellComponent = newSpellObj.GetComponent<Spell>(); 
        if (spellComponent != null) spellComponent.Initialize(selectedData); 
    }
}