using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System; // Dodane dla wyjątków

public class CityExpeditionUI : MonoBehaviour
{
    [Header("UI - Aktualna Ekspedycja (Góra)")]
    public Button[] expeditionSlots; 

    [Header("UI - Paski Ekspedycji")]
    public Image[] hpBars;
    public Image[] expBars;

    [Header("UI - Magazyn Potworów (Dół)")]
    public GameObject panelWyboruPotworow; 
    public RectTransform rosterContainer;
    public GameObject rosterPrefab; 

    private int lastPartyCount = -1;
    private int lastRosterCount = -1; 
    private bool playerManagerZnaleziony = false;

    void Start()
    {
        Debug.Log("<color=yellow>[MIASTO-UI]</color> Start skryptu CityExpeditionUI. Czy panel wyboru jest podpięty? " + (panelWyboruPotworow != null));
        //if (panelWyboruPotworow != null) panelWyboruPotworow.SetActive(false);
    }

    void Update()
    {
        if (PlayerManager.Instance == null) return;

        if (!playerManagerZnaleziony)
        {
            Debug.Log("<color=green>[MIASTO-UI]</color> Znalazłem PlayerManager.Instance! Rozpoczynam nasłuchiwanie.");
            playerManagerZnaleziony = true;
        }

        try
        {
            int currentPartyCount = PlayerManager.Instance.activeParty != null ? PlayerManager.Instance.activeParty.Count : 0;
            int currentRosterCount = PlayerManager.Instance.playerMonsters != null ? PlayerManager.Instance.playerMonsters.Count : 0;

            // 1. Wymuszone odświeżenie po zmianie
            if (lastPartyCount != currentPartyCount || lastRosterCount != currentRosterCount)
            {
                Debug.Log($"<color=yellow>[MIASTO-UI]</color> Wykryto zmianę danych! Party: {lastPartyCount} -> {currentPartyCount}, Roster: {lastRosterCount} -> {currentRosterCount}");
                OdswiezInterfejs();
            }

            // 2. Płynne odświeżanie pasków (tylko bezpieczne wartości)
            if (expeditionSlots != null && expeditionSlots.Length > 0)
            {
                for (int i = 0; i < expeditionSlots.Length; i++)
                {
                    if (PlayerManager.Instance.activeParty != null && i < PlayerManager.Instance.activeParty.Count)
                    {
                        MonsterSaveData data = PlayerManager.Instance.activeParty[i];
                        
                        if (hpBars != null && i < hpBars.Length && hpBars[i] != null)
                        {
                            float maxH = data.maxHealth > 0 ? data.maxHealth : 1f;
                            hpBars[i].fillAmount = Mathf.Clamp01((float)data.health / maxH);
                        }

                        if (expBars != null && i < expBars.Length && expBars[i] != null)
                        {
                            float maxE = ExperienceManager.WymaganyExp(data.level);
                            maxE = maxE > 0 ? maxE : 1f;
                            expBars[i].fillAmount = Mathf.Clamp01((float)data.currentExp / maxE);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"<color=red>[MIASTO-UI BŁĄD W UPDATE]</color> {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void PrzelaczTrybEdycji()
    {
        if (panelWyboruPotworow != null)
        {
            bool stan = !panelWyboruPotworow.activeSelf;
            panelWyboruPotworow.SetActive(stan);
            Debug.Log($"<color=magenta>[MIASTO-UI]</color> Przycisk edycji kliknięty. Stan panelu: {stan}");
            
            if (stan) OdswiezInterfejs(); 
        }
    }

    public void OdswiezInterfejs()
    {
        Debug.Log("<color=magenta>[MIASTO-UI]</color> Rozpoczynam OdswiezInterfejs()...");

        if (PlayerManager.Instance == null) 
        {
            Debug.LogWarning("<color=orange>[MIASTO-UI]</color> Przerwano OdswiezInterfejs - PlayerManager to null!");
            return;
        }

        if (expeditionSlots == null || expeditionSlots.Length == 0)
        {
            Debug.LogError("<color=red>[MIASTO-UI FATALNE]</color> Tablica expeditionSlots jest pusta! Czy przypisałeś sloty w Inspektorze?");
            return;
        }

        if (rosterContainer == null)
        {
            Debug.LogError("<color=red>[MIASTO-UI FATALNE]</color> rosterContainer nie jest przypisany!");
            return;
        }

        try
        {
            foreach (var activeMon in PlayerManager.Instance.activeParty)
            {
                if (!PlayerManager.Instance.playerMonsters.Contains(activeMon))
                {
                    PlayerManager.Instance.playerMonsters.Add(activeMon);
                    Debug.Log($"<color=magenta>[MIASTO-UI]</color> Dodano potwora {activeMon.monsterName} z Party do ogólnego Rostera.");
                }
            }

            foreach (Transform child in rosterContainer) Destroy(child.gameObject);

            for (int i = 0; i < expeditionSlots.Length; i++)
            {
                Image slotImage = expeditionSlots[i].GetComponent<Image>();

                if (i < PlayerManager.Instance.activeParty.Count)
                {
                    MonsterSaveData data = PlayerManager.Instance.activeParty[i];
                    Sprite art = GetFaceArt(data.monsterName);
                    
                    slotImage.sprite = art;
                    if (art != null) slotImage.color = Color.white;
                    else slotImage.color = new Color(1, 1, 1, 0);

                    if (hpBars != null && i < hpBars.Length && hpBars[i] != null) hpBars[i].gameObject.SetActive(true);
                    if (expBars != null && i < expBars.Length && expBars[i] != null) expBars[i].gameObject.SetActive(true);

                    expeditionSlots[i].onClick.RemoveAllListeners();
                    expeditionSlots[i].onClick.AddListener(() => UsunZEkspedycji(data));
                }
                else
                {
                    slotImage.sprite = null;
                    slotImage.color = new Color(0, 0, 0, 0.2f); 

                    if (hpBars != null && i < hpBars.Length && hpBars[i] != null) hpBars[i].gameObject.SetActive(false);
                    if (expBars != null && i < expBars.Length && expBars[i] != null) expBars[i].gameObject.SetActive(false);

                    expeditionSlots[i].onClick.RemoveAllListeners(); 
                }
            }

            foreach (var monster in PlayerManager.Instance.playerMonsters)
            {
                if (PlayerManager.Instance.activeParty.Contains(monster)) continue;

                if (rosterPrefab != null)
                {
                    GameObject btnObj = Instantiate(rosterPrefab, rosterContainer);
                    Image iconImg = btnObj.GetComponent<Image>();
                    iconImg.sprite = GetFaceArt(monster.monsterName);

                    Button btn = btnObj.GetComponent<Button>();
                    btn.onClick.AddListener(() => DodajDoEkspedycji(monster));
                }
            }

            lastPartyCount = PlayerManager.Instance.activeParty.Count;
            lastRosterCount = PlayerManager.Instance.playerMonsters.Count;
            
            Debug.Log("<color=green>[MIASTO-UI]</color> Zakonczono OdswiezInterfejs sukcesem!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"<color=red>[MIASTO-UI BŁĄD W ODSWIEZ]</color> {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void DodajDoEkspedycji(MonsterSaveData monster)
    {
        if (PlayerManager.Instance.activeParty.Count < 3)
        {
            PlayerManager.Instance.activeParty.Add(monster);
            OdswiezInterfejs();
        }
    }

    private void UsunZEkspedycji(MonsterSaveData monster)
    {
        if (panelWyboruPotworow != null && !panelWyboruPotworow.activeSelf) return;
        PlayerManager.Instance.activeParty.Remove(monster);
        OdswiezInterfejs(); 
    }

    private Sprite GetFaceArt(string monsterName)
    {
        if (string.IsNullOrEmpty(monsterName)) return null;
        string cleanName = monsterName.Replace("(Clone)", "").Trim();
        if (PlayerManager.Instance.bazaWszystkichPotworow != null)
        {
            foreach (var prefab in PlayerManager.Instance.bazaWszystkichPotworow) 
            {
                if (prefab != null && prefab.name.Replace("(Clone)", "").Trim() == cleanName)
                {
                    PlayerBattle pb = prefab.GetComponent<PlayerBattle>();
                    if (pb != null) return pb.faceArt;
                }
            }
        }
        return null; 
    }
}