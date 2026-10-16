using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Referencje UI")]
    public Transform contentContainer; // Gdzie mają tworzyć się sloty
    public GameObject slotPrefab;      // Prefab pojedynczego przycisku z przedmiotem

    [Header("Podgląd (do leczenia)")]
    public MonsterDetailsUI monsterDetails; // Skrypt zarządzający podglądem potwora (aby wiedzieć, na kim użyć itemu)

    public void OdswiezEkwipunek()
    {
        // 1. Czyszczenie starych slotów
        foreach (Transform child in contentContainer) Destroy(child.gameObject);

        // 2. Grupowanie przedmiotów (zlicza ile mamy danej mikstury/materiału)
        Dictionary<ItemData, int> zgrupowanePrzedmioty = new Dictionary<ItemData, int>();
        
        foreach (var item in PlayerManager.Instance.playerItems)
        {
            // Ignorujemy zbroje/bronie, bo one mają osobny system (Ekwipunek)
            if (item.typ != TypPrzedmiotu.Ekwipunek)
            {
                if (zgrupowanePrzedmioty.ContainsKey(item)) zgrupowanePrzedmioty[item]++;
                else zgrupowanePrzedmioty[item] = 1;
            }
        }

        // 3. Rysowanie przycisków
        foreach (var wpis in zgrupowanePrzedmioty)
        {
            ItemData dane = wpis.Key;
            int ilosc = wpis.Value;

            GameObject slot = Instantiate(slotPrefab, contentContainer);
            
            // Podpinanie grafiki i tekstu (zakładając, że masz w prefabie Image i TextMeshPro)
            slot.transform.Find("Icon").GetComponent<Image>().sprite = dane.itemIcon;
            slot.transform.Find("NameText").GetComponent<TMP_Text>().text = $"{dane.itemName} x{ilosc}";

            // Jeśli to mikstura, dodajemy funkcję do przycisku
            if (dane.typ == TypPrzedmiotu.Mikstura)
            {
                Button btn = slot.GetComponent<Button>();
                btn.onClick.AddListener(() => UzyjMikstury(dane));
            }
        }
    }

    private void UzyjMikstury(ItemData mikstura)
    {
        if (monsterDetails == null || monsterDetails.currentSelectedMonster == null)
        {
            Debug.LogWarning("Wybierz potwora, na którym chcesz użyć mikstury!");
            return;
        }

        MonsterSaveData cel = monsterDetails.currentSelectedMonster;

        // 1. Leczenie
        if (mikstura.przywrocenieHP > 0)
        {
            cel.health += mikstura.przywrocenieHP;
            if (cel.health > cel.maxHealth) cel.health = cel.maxHealth;
            Debug.Log($"Wyleczono {cel.monsterName} o {mikstura.przywrocenieHP} HP!");
        }

        // 2. Permamentne dodanie statystyk (np. Eliksir Siły)
        foreach (var stat in mikstura.itemStats)
        {
            DodajStatystyke(cel, stat);
        }

        // 3. Dodanie buffów na start walki
        if (mikstura.statusyPrzedWalka.Count > 0)
        {
            cel.przygotowaneStatusy.AddRange(mikstura.statusyPrzedWalka);
            Debug.Log($"Potwór wypił miksturę. Zacznie z dodatkowym statusem!");
        }

        // Usuwamy dokładnie JEDNĄ sztukę z fizycznego plecaka
        PlayerManager.Instance.playerItems.Remove(mikstura);
        
        // Odświeżamy UI żeby ilość "x3" spadła na "x2"
        OdswiezEkwipunek();
        
        // POPRAWKA: Odświeżamy podgląd za pomocą dostępnej funkcji
        monsterDetails.WybierzPotwora(cel); 
    }

    private void DodajStatystyke(MonsterSaveData monster, StatReward stat)
    {
        string u = stat.statName.ToUpper();
        
        // POPRAWKA: Używamy "wartosc" zamiast "value" lub "amount"
        if (u.Contains("ATACK")) monster.atack += stat.wartosc; 
        else if (u.Contains("MAGIC")) monster.magic += stat.wartosc; 
        
        // Możesz tutaj dopisać kolejne "else if" dla ARMOR, HP itp.
    }
}