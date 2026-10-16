using UnityEngine;
using System.Collections.Generic;

public class StarterSelectionUI : MonoBehaviour
{
    [Header("UI Referencje")]
    public GameObject selectionPanel; // Całe okno z wyborem (tło)
    public Transform optionsContainer; // Miejsce (np. Horizontal Layout Group), gdzie pojawią się opcje
    public GameObject starterSlotPrefab; // Prefab z przypiętym skryptem StarterSlotUI

    [Header("Dostępne Startery")]
    public List<GameObject> dostepneStartery; // Przeciągnij tu prefaby z folderu Project (np. Wilk, Śluz, Ognik)

    void Start()
    {
        // Sprawdzamy stan z lekkim opóźnieniem, by upewnić się, że PlayerManager na pewno się załadował
        Invoke(nameof(SprawdzCzyPotrzebnyStarter), 0.1f);
    }

    private void SprawdzCzyPotrzebnyStarter()
    {
        if (PlayerManager.Instance == null) return;

        // Jeśli lista jest pusta, znaczy że to nowa gra bez przypisanych testowych potworów
        if (PlayerManager.Instance.playerMonsters.Count == 0)
        {
            PokazEkranWyboru();
        }
        else
        {
            if (selectionPanel != null) selectionPanel.SetActive(false);
        }
    }

    private void PokazEkranWyboru()
    {
        if (selectionPanel != null) selectionPanel.SetActive(true);

        // Czyszczenie starych opcji
        foreach (Transform child in optionsContainer) Destroy(child.gameObject);

        // Generowanie potworków do wyboru
        foreach (GameObject prefab in dostepneStartery)
        {
            GameObject slotObj = Instantiate(starterSlotPrefab, optionsContainer);
            StarterSlotUI slotUI = slotObj.GetComponent<StarterSlotUI>();

            if (slotUI != null)
            {
                // Ustawianie nazwy
                if (slotUI.nameText != null) 
                    slotUI.nameText.text = prefab.name.Replace("(Clone)", "").Trim();

                // Ustawianie grafiki (pobieranej z PlayerBattle tak jak w podglądzie potwora)
                PlayerBattle pb = prefab.GetComponent<PlayerBattle>();
                if (pb != null && slotUI.iconImage != null) 
                    slotUI.iconImage.sprite = pb.faceArt;

                // Podpinanie akcji pod przycisk
                if (slotUI.chooseButton != null)
                {
                    // Używamy zmiennej lokalnej do lambdy, by uniknąć błędu z pętlą (tzw. closure)
                    GameObject wybranyPrefab = prefab; 
                    slotUI.chooseButton.onClick.AddListener(() => WybierzStartera(wybranyPrefab));
                }
            }
        }
    }

    private void WybierzStartera(GameObject wybranyPrefab)
    {
        Stats staty = wybranyPrefab.GetComponent<Stats>();
        if (staty != null)
        {
            // 1. Tworzymy nowe dane na podstawie wybranego prefabu
            MonsterSaveData noweDane = new MonsterSaveData();
            noweDane.SaveFromStats(staty, wybranyPrefab.name);

            // 2. Dodajemy do ogólnej listy i do aktywnej drużyny
            PlayerManager.Instance.playerMonsters.Add(noweDane);
            PlayerManager.Instance.activeParty.Add(noweDane);

            // 3. Natychmiast zapisujemy grę, by utrwalić wybór
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.ZapiszGre();
            }

            Debug.Log($"<color=green>[STARTER]</color> Wybrano potwora: {wybranyPrefab.name}!");

            // 4. Zamykamy panel
            if (selectionPanel != null) selectionPanel.SetActive(false);
            
            // OPCJONALNIE: Odśwież UI ekwipunku/drużyny, jeśli masz je otwarte w tle
        }
    }
}