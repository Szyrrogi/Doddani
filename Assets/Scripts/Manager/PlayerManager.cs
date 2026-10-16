using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("Dane Gracza")]
    public int playerLevel = 1;
    public int gold = 0;

    [Header("Postęp Przygody")]
    public int adventureWejscia = 0;
    public int maxWejscia = 30;
    public GameObject wybranyPrzeciwnikPrefab; 
    public List<ItemData> bazaWszystkichPrzedmiotow;

    [Header("Drużyna Gracza")]
    public List<GameObject> bazaWszystkichPotworow = new List<GameObject>();
    public List<MonsterSaveData> playerMonsters = new List<MonsterSaveData>();
    public List<MonsterSaveData> activeParty = new List<MonsterSaveData>();
    
    [Header("Ekwipunek Gracza")]
    public List<ItemData> playerItems = new List<ItemData>(); 
    
    [Header("Startowy Ekwipunek (Nowa Gra)")]
    public List<GameObject> startowePotworyPrefaby; 
    public List<ItemData> startowePrzedmioty = new List<ItemData>(); 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // USUNIĘTO: Automatyczne dodawanie potworów i przedmiotów. Od teraz zarządza tym SaveManager!
        }
        else
        {
            Destroy(gameObject); 
        }
    }

void Start()
    {
        // MORDERSTWO KLONÓW: Jeśli ten skrypt to duplikat, przerywamy Start(), zanim cokolwiek zepsuje!
        if (Instance != this) return; 

        if (SaveManager.Instance != null)
        {
            if (SaveManager.Instance.czyWymuszonoNowaGre)
            {
                InicjalizujStartoweZasoby();
                SaveManager.Instance.ZapiszGre(); 
                SaveManager.Instance.czyWymuszonoNowaGre = false; 
            }
            else if (SaveManager.Instance.wczytaneDaneZPamięci != null)
            {
                ZaladujWczytaneDane(SaveManager.Instance.wczytaneDaneZPamięci);
                SaveManager.Instance.wczytaneDaneZPamięci = null; // Czyścimy po udanym wczytaniu
            }
            else if (playerMonsters.Count == 0)
            {
                // Odpalono grę ze środka, a nie z menu (tryb testowy)
                InicjalizujStartoweZasoby();
            }
        }
    }

    public void ZaladujWczytaneDane(GameSaveData dane)
    {
        adventureWejscia = dane.adventureWejscia;
        maxWejscia = dane.maxWejscia;
        
        // 1. Ładujemy główną listę
        playerMonsters = dane.playerMonsters;
        
        // 2. Odtwarzamy aktywną drużynę po indeksach
        activeParty.Clear();
        foreach (int index in dane.activePartyIndices)
        {
            if (index >= 0 && index < playerMonsters.Count)
            {
                // Dzięki temu activeParty celuje dokładnie w ten sam obiekt, co playerMonsters!
                activeParty.Add(playerMonsters[index]); 
            }
        }

        playerItems.Clear();
        foreach (string nazwa in dane.playerItemNames)
        {
            ItemData znalezionyItem = bazaWszystkichPrzedmiotow.Find(i => i.itemName == nazwa);
            if (znalezionyItem != null) playerItems.Add(znalezionyItem);
        }
        
        Debug.Log($"<color=green>[PLAYER MANAGER]</color> Odebrano dane! Załadowano {playerMonsters.Count} potworów. W drużynie: {activeParty.Count}");
    }

    // NOWA FUNKCJA: Wywoływana tylko przy tworzeniu nowego zapisu gry
    public void InicjalizujStartoweZasoby()
    {
        playerMonsters.Clear();
        activeParty.Clear();
        playerItems.Clear();

        foreach (GameObject prefab in startowePotworyPrefaby)
        {
            Stats statyZPrefabu = prefab.GetComponent<Stats>();
            if (statyZPrefabu != null)
            {
                MonsterSaveData noweDane = new MonsterSaveData();
                noweDane.SaveFromStats(statyZPrefabu, prefab.name);
                
                playerMonsters.Add(noweDane);
                
                if (activeParty.Count < 3)
                {
                    activeParty.Add(noweDane);
                }
            }
        }
        
        if (startowePrzedmioty.Count > 0)
        {
            playerItems.AddRange(startowePrzedmioty);
        }
        
        Debug.Log($"<color=yellow>[PLAYER MANAGER]</color> Zainicjalizowano Nową Grę. Dodano {playerMonsters.Count} potworów i {playerItems.Count} przedmiotów.");
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"Dodano {amount} złota. Aktualne złoto: {gold}");
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            return true;
        }
        return false;
    }
}