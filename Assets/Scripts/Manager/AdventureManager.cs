using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq; 

public enum TypZdarzenia { Walka, Skarb, Doswiadczenie, Potwor } // Dodano 'Potwor'

[System.Serializable]
public class Zdarzenie
{
    public TypZdarzenia typ;
    public GameObject wrogPrefab;
    public int iloscExp;
    public GameObject potworPrefab;
    
    [Tooltip("Im większa wartość, tym większa szansa na wylosowanie")]
    public int waga = 10; // Domyślna waga
}

[System.Serializable]
public class KamienMilowy
{
    [Tooltip("Numer wejścia (etap), np. 1, 10, 20")]
    public int etap; 
    public List<Zdarzenie> mozliweZdarzenia = new List<Zdarzenie>();
}

public class AdventureManager : MonoBehaviour
{
    [Header("UI - Główne")]
    public TMP_Text licznikText; 
    public GameObject ekranNagrody; 
    public TMP_Text opisNagrodyText; 

    [Header("UI - Wybór Skarbu")]
    public GameObject ekranWyboruPrzedmiotu; 
    
    public Image opcja1Image; // Miejsce na grafikę
    public Button opcja1Button;
    public TMP_Text opcja1Text;
    
    public Image opcja2Image; // Miejsce na grafikę[cite: 6]
    public Button opcja2Button;
    public TMP_Text opcja2Text;

    [Header("Progresja i Zdarzenia")]
    public List<KamienMilowy> kamienieMilowe = new List<KamienMilowy>();
    
    [Header("Globalna Pula Przedmiotów")]
    public List<ItemData> pulaPrzedmiotow = new List<ItemData>(); 

    [Header("Spawnowanie Drużyny Gracza")]
    public Transform[] spawnPoints; 

    void Start()
    {
        if (ekranNagrody != null) ekranNagrody.SetActive(false); 
        if (ekranWyboruPrzedmiotu != null) ekranWyboruPrzedmiotu.SetActive(false);
        
        kamienieMilowe = kamienieMilowe.OrderBy(k => k.etap).ToList();

        SpawnujDruzyne(); 
        AktualizujUI(); 
    }

    public void OnExploreClicked()
    {
        if (PlayerManager.Instance.adventureWejscia >= PlayerManager.Instance.maxWejscia) return; 

        PlayerManager.Instance.adventureWejscia++; 
        AktualizujUI(); 
        LosujZdarzenie(PlayerManager.Instance.adventureWejscia);
    }

    private void LosujZdarzenie(int obecnyEtap)
    {
        if (kamienieMilowe.Count == 0) return;

        KamienMilowy milestoneA = null;
        KamienMilowy milestoneB = null;

        foreach (var m in kamienieMilowe)
        {
            if (m.etap <= obecnyEtap) milestoneA = m;
            if (m.etap > obecnyEtap && milestoneB == null) milestoneB = m;
        }

        KamienMilowy wylosowanyMilestone = null;

        if (milestoneA != null && milestoneB != null)
        {
            float szansaNaB = ((float)(obecnyEtap - milestoneA.etap) / (milestoneB.etap - milestoneA.etap)) * 100f;
            wylosowanyMilestone = (Random.Range(0f, 100f) <= szansaNaB) ? milestoneB : milestoneA;
        }
        else if (milestoneA != null) wylosowanyMilestone = milestoneA;
        else if (milestoneB != null) wylosowanyMilestone = milestoneB;

        if (wylosowanyMilestone != null && wylosowanyMilestone.mozliweZdarzenia.Count > 0)
        {
            // --- NOWY SYSTEM WAŻONEGO LOSOWANIA ---
            int sumaWag = 0;
            foreach (var z in wylosowanyMilestone.mozliweZdarzenia) sumaWag += z.waga;

            int wylosowanaWartosc = Random.Range(0, sumaWag);
            int aktualnaSuma = 0;

            foreach (var zdarzenie in wylosowanyMilestone.mozliweZdarzenia)
            {
                aktualnaSuma += zdarzenie.waga;
                if (wylosowanaWartosc < aktualnaSuma)
                {
                    RozpatrzZdarzenie(zdarzenie);
                    break;
                }
            }
        }
    }

   private void RozpatrzZdarzenie(Zdarzenie zdarzenie)
    {
        switch (zdarzenie.typ)
        {
            case TypZdarzenia.Walka:
                PlayerManager.Instance.wybranyPrzeciwnikPrefab = zdarzenie.wrogPrefab;
                SceneManager.LoadScene(1); 
                break;

            case TypZdarzenia.Skarb:
                PokazWyborPrzedmiotu();
                break;

            case TypZdarzenia.Doswiadczenie:
                PokazEkranNagrody($"Zyskujesz {zdarzenie.iloscExp} punktów doświadczenia!");
                RozdajExpDruzynie(zdarzenie.iloscExp);
                break;
                
            case TypZdarzenia.Potwor:
                RozpatrzZdobyciePotwora(zdarzenie.potworPrefab);
                break;
        }
    }

    private void RozpatrzZdobyciePotwora(GameObject prefab)
    {
        if (prefab == null) return;
        string cleanName = prefab.name.Replace("(Clone)", "").Trim();

        // Sprawdzamy, czy potwór o tej nazwie już istnieje w kolekcji gracza
        bool juzPosiada = PlayerManager.Instance.playerMonsters.Any(m => m.monsterName.Replace("(Clone)", "").Trim() == cleanName);

        if (juzPosiada)
        {
            PokazEkranNagrody($"Spotykasz {cleanName}, ale masz go już w drużynie. Zyskujesz 10 EXP!");
            RozdajExpDruzynie(10);
        }
        else
        {
            Stats staty = prefab.GetComponent<Stats>();
            if (staty != null)
            {
                MonsterSaveData noweDane = new MonsterSaveData();
                noweDane.SaveFromStats(staty, cleanName);
                
                PlayerManager.Instance.playerMonsters.Add(noweDane);
                
                // Jeśli mamy miejsce w aktywnej drużynie, od razu go tam dodajemy
                if (PlayerManager.Instance.activeParty.Count < 3)
                {
                    PlayerManager.Instance.activeParty.Add(noweDane);
                }
                
                PokazEkranNagrody($"Znalazłeś nowego towarzysza! {cleanName} dołącza do Twojej drużyny!");
            }
        }
        
        // Zapisujemy stan gry po zdobyciu stworka lub expa
        if (SaveManager.Instance != null) SaveManager.Instance.ZapiszGre();
    }

    private void PokazWyborPrzedmiotu()
    {
        pulaPrzedmiotow.RemoveAll(item => item == null);

        List<ItemData> dostepne = pulaPrzedmiotow.Where(item => !PlayerManager.Instance.playerItems.Contains(item)).ToList();

        foreach (var potwor in PlayerManager.Instance.playerMonsters)
        {
            dostepne.RemoveAll(item => potwor.equippedItems.Contains(item));
        }

        if (dostepne.Count >= 2)
        {
            int i1 = Random.Range(0, dostepne.Count);
            ItemData item1 = dostepne[i1];
            dostepne.RemoveAt(i1); 

            int i2 = Random.Range(0, dostepne.Count);
            ItemData item2 = dostepne[i2];

            UruchomEkranWyboru(item1, item2);
        }
        else if (dostepne.Count == 1)
        {
            // Został 1 przedmiot - pokazujemy go na obu przyciskach
            UruchomEkranWyboru(dostepne[0], dostepne[0]);
        }
        else
        {
            PokazEkranNagrody("Zebrałeś już wszystkie przedmioty ze świata!");
        }
    }

    private void UruchomEkranWyboru(ItemData item1, ItemData item2)
    {
        ekranNagrody.SetActive(false); 
        ekranWyboruPrzedmiotu.SetActive(true);

        opcja1Text.text = item1.itemName;
        if (opcja1Image != null) opcja1Image.sprite = item1.itemIcon; // Przypisanie ikony[cite: 6]
        opcja1Button.onClick.RemoveAllListeners();
        opcja1Button.onClick.AddListener(() => WybierzZNagrody(item1));

        opcja2Text.text = item2.itemName;
        if (opcja2Image != null) opcja2Image.sprite = item2.itemIcon; // Przypisanie ikony[cite: 6]
        opcja2Button.onClick.RemoveAllListeners();
        opcja2Button.onClick.AddListener(() => WybierzZNagrody(item2));
    }

    private void WybierzZNagrody(ItemData wybranyItem)
    {
        NadajPrzedmiot(wybranyItem);
        ekranWyboruPrzedmiotu.SetActive(false);
        PokazEkranNagrody($"Zabrałeś ze sobą: {wybranyItem.itemName}");
    }

    private void NadajPrzedmiot(ItemData item)
    {
        PlayerManager.Instance.playerItems.Add(item);
    }

    private void PokazEkranNagrody(string tresc)
    {
        if (opisNagrodyText != null) opisNagrodyText.text = tresc;
        ekranNagrody.SetActive(true);
    }

    private void RozdajExpDruzynie(int iloscExp)
    {
        if (PlayerManager.Instance.activeParty.Count == 0) return;
        
        int expNaGlowe = iloscExp / PlayerManager.Instance.activeParty.Count;
        Debug.Log($"<color=cyan>[PRZYGODA - EXP]</color> Rozdaję łącznie {iloscExp} EXP. Na głowę przypada: {expNaGlowe} EXP.");
        
        foreach (var potwor in PlayerManager.Instance.activeParty) 
        {
            potwor.currentExp += expNaGlowe;
            Debug.Log($"<color=cyan>[PRZYGODA - EXP]</color> {potwor.monsterName} zyskuje exp. Stan przed awansem: {potwor.currentExp} / {ExperienceManager.WymaganyExp(potwor.level)}");
            
            // Pętla awansu
            while (potwor.currentExp >= ExperienceManager.WymaganyExp(potwor.level))
            {
                int wymagany = ExperienceManager.WymaganyExp(potwor.level);
                
                // Odejmujemy "zużyty" exp
                potwor.currentExp -= wymagany;
                potwor.level++;
                
                // ==========================================
                // NOWE: Integracja z systemem LevelUpRewards
                // ==========================================
                GameObject prefab = ZnajdzPrefab(potwor.monsterName);
                if (prefab != null)
                {
                    LevelUpRewards nagrody = prefab.GetComponent<LevelUpRewards>();
                    if (nagrody != null)
                    {
                        // Przekazujemy "null" w miejscu Stats, ponieważ na scenie przygody
                        // potwory nie prowadzą walki. Najważniejsze, że zapis (potwor) 
                        // zyskuje SP i bazowe statystyki!
                        nagrody.PrzyznajNagrody(potwor, null);
                    }
                    else
                    {
                        Debug.LogWarning($"<color=orange>[LEVEL UP]</color> Prefab {potwor.monsterName} nie ma skryptu LevelUpRewards!");
                    }
                }
                
                Debug.Log($"<color=yellow>[LEVEL UP!]</color> {potwor.monsterName} awansuje na {potwor.level} poziom! " +
                          $"Reszta EXP przechodząca na kolejny poziom: {potwor.currentExp}");
            }
        }

        // Zapisujemy stan gry po rozdaniu expa, awansach i nagrodach!
        if (SaveManager.Instance != null) 
        {
            SaveManager.Instance.ZapiszGre();
            Debug.Log("<color=green>[PRZYGODA - ZAPIS]</color> Zapisano grę po przydzieleniu doświadczenia.");
        }
    }

    public void ZamknijEkranNagrody() 
    {
        ekranNagrody.SetActive(false); 
    }

    private void SpawnujDruzyne() 
    {
        if (PlayerManager.Instance == null) return; 

        for (int i = 0; i < PlayerManager.Instance.activeParty.Count; i++) 
        {
            if (i >= spawnPoints.Length) break;  
            MonsterSaveData savedData = PlayerManager.Instance.activeParty[i]; 
            GameObject prefab = ZnajdzPrefab(savedData.monsterName); 

            if (prefab != null) 
            {
                GameObject potwor = Instantiate(prefab, spawnPoints[i].position, Quaternion.identity); 
                savedData.LoadToStats(potwor.GetComponent<Stats>()); 
            }
        }
    }

    private GameObject ZnajdzPrefab(string nazwa) 
    {
        string szukanaNazwa = nazwa.Replace("(Clone)", "").Trim(); 
        foreach (var p in PlayerManager.Instance.bazaWszystkichPotworow) // Zmiana na global
        {
            if (p == null) continue; 
            if (p.name.Replace("(Clone)", "").Trim() == szukanaNazwa) return p; 
        }
        return null; 
    }

    private void AktualizujUI() 
    {
        if (PlayerManager.Instance != null && licznikText != null) 
            licznikText.text = $"Wejścia: {PlayerManager.Instance.adventureWejscia} / {PlayerManager.Instance.maxWejscia}"; 
    }
    // ==========================================
    // SYSTEM POWROTU DO BAZY
    // ==========================================
    public void WrocDoBazy()
    {
        if (PlayerManager.Instance != null)
        {
            // Odejmujemy połowę wejść. Jeśli masz 7 wejść, 7 / 2 = 3. 
            // Zostaną Ci 3 wejścia, a stracisz 4.
            PlayerManager.Instance.adventureWejscia /= 2;

            // Zabezpieczenie przed ujemnymi wartościami (gdyby gracz wycofał się z zera wejść)
            if (PlayerManager.Instance.adventureWejscia < 0)
            {
                PlayerManager.Instance.adventureWejscia = 0;
            }

            // Zapisujemy nowy stan, żeby kara była permanentna
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.ZapiszGre();
            }

            Debug.Log($"<color=orange>[PRZYGODA]</color> Wycofano się z lasu. Pozostałe wejścia: {PlayerManager.Instance.adventureWejscia}");
        }

        // Zmień "0" na indeks sceny Twojego Głównego Menu (sprawdź w Build Settings)
        SceneManager.LoadScene(0); 
    }
}