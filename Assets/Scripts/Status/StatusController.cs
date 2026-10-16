using System.Collections.Generic;
using UnityEngine;

public class StatusController : MonoBehaviour
{
    [Header("Interfejs UI")]
    public Transform ikonyContainer; 
    public GameObject statusIconPrefab; 

    public List<AktywnyStatus> aktywneStatusy = new List<AktywnyStatus>();
    private Stats statystyki; 

    void Start()
    {
        statystyki = GetComponent<Stats>();
    }

    void Update()
    {
        for (int i = aktywneStatusy.Count - 1; i >= 0; i--)
        {
            AktywnyStatus status = aktywneStatusy[i];

            if (status.typCzasu == TypCzasu.Sekundy)
            {
                status.pozostalyCzasSekundy -= Time.deltaTime;
                
                if (status.skryptUI != null)
                {
                    status.skryptUI.Aktualizuj(status.poziom, status.pozostalyCzasSekundy, status.maxCzasSekundy);
                }

                if (status.pozostalyCzasSekundy <= 0)
                {
                    UsunStatus(status);
                    continue;
                }
            }

            status.tickTimer += Time.deltaTime;
            if (status.tickTimer >= 4f)
            {
                PrzetworzTick(status);
                status.tickTimer = 0f; 
            }
        }
    }

    public void NalozStatus(StatusData nowyStatus)
    {
        Debug.Log($"<color=cyan>[STATUS DEBUG]</color> Otrzymano żądanie nałożenia: {nowyStatus.typ} (Poz: {nowyStatus.poziom}) na obiekt {gameObject.name}.");
        
        AktywnyStatus istniejacy = aktywneStatusy.Find(s => s.typ == nowyStatus.typ && s.statystyka == nowyStatus.statystyka);

        if (istniejacy != null)
        {
            Debug.Log($"<color=cyan>[STATUS DEBUG]</color> Status {nowyStatus.typ} już istnieje! Sumuję poziom i resetuję czas.");
            istniejacy.poziom += nowyStatus.poziom;
            istniejacy.pozostalyCzasSekundy = nowyStatus.wartoscCzasu;
            istniejacy.maxCzasSekundy = nowyStatus.wartoscCzasu; 
            istniejacy.pozostaleWalki = (int)nowyStatus.wartoscCzasu;
            
            BalansujSzczescie();
        }
        else
        {
            Debug.Log($"<color=cyan>[STATUS DEBUG]</color> Tworzenie zupełnie nowego statusu {nowyStatus.typ}...");
            AktywnyStatus nowy = new AktywnyStatus(nowyStatus);
            
            if (statusIconPrefab != null && ikonyContainer != null)
            {
                Debug.Log($"<color=cyan>[STATUS DEBUG]</color> Generowanie obiektu UI dla statusu...");
                nowy.obiektUI = Instantiate(statusIconPrefab, ikonyContainer);
                nowy.skryptUI = nowy.obiektUI.GetComponent<StatusIconUI>();
                
                if (nowy.skryptUI != null)
                {
                    if (nowy.skryptUI.iconImage != null) 
                        nowy.skryptUI.iconImage.sprite = nowyStatus.ikonaZrodlowa;
                    else
                        Debug.LogWarning("<color=orange>[STATUS DEBUG]</color> Komponent Image (tło ikony) w UI nie jest podpięte!");

                    nowy.skryptUI.Aktualizuj(nowy.poziom, nowy.pozostalyCzasSekundy, nowy.maxCzasSekundy);
                    Debug.Log($"<color=cyan>[STATUS DEBUG]</color> UI wygenerowano z sukcesem.");
                }
                else
                {
                    Debug.LogError("<color=red>[STATUS DEBUG]</color> Prefab ikony nie posiada przypiętego skryptu StatusIconUI!");
                }
            }
            else
            {
                Debug.LogWarning($"<color=orange>[STATUS DEBUG]</color> Brakuje referencji UI na obiekcie {gameObject.name}! " +
                                 $"IkonyContainer: {(ikonyContainer != null ? "OK" : "BRAK")}, " +
                                 $"StatusIconPrefab: {(statusIconPrefab != null ? "OK" : "BRAK")}");
            }

            aktywneStatusy.Add(nowy);
            BalansujSzczescie();
            Debug.Log($"<color=cyan>[STATUS DEBUG]</color> Pomyślnie dodano status do listy. Aktywnych statusów: {aktywneStatusy.Count}");
        }
    }
 

    private void BalansujSzczescie()
    {
        AktywnyStatus szczescie = aktywneStatusy.Find(s => s.typ == TypStatusu.Szczescie);
        AktywnyStatus pech = aktywneStatusy.Find(s => s.typ == TypStatusu.Pech);

        if (szczescie != null && pech != null)
        {
            if (szczescie.poziom > pech.poziom)
            {
                szczescie.poziom -= pech.poziom;
                UsunStatus(pech);
            }
            else if (pech.poziom > szczescie.poziom)
            {
                pech.poziom -= szczescie.poziom;
                UsunStatus(szczescie);
            }
            else
            {
                UsunStatus(szczescie);
                UsunStatus(pech);
            }
        }
    }

    public int ObliczSzczescie()
    {
        AktywnyStatus szczescie = aktywneStatusy.Find(s => s.typ == TypStatusu.Szczescie);
        if (szczescie != null) return szczescie.poziom;

        AktywnyStatus pech = aktywneStatusy.Find(s => s.typ == TypStatusu.Pech);
        if (pech != null) return -pech.poziom;

        return 0;
    }

    public float PobierzMnoznikLeczenia()
    {
        AktywnyStatus oslabienie = aktywneStatusy.Find(s => s.typ == TypStatusu.Oslabienie);
        if (oslabienie != null)
        {
            float redukcja = oslabienie.poziom * 0.1f; 
            return Mathf.Clamp(1f - redukcja, 0f, 1f); 
        }
        return 1f; 
    }

    private void UsunStatus(AktywnyStatus status)
    {
        if (status.obiektUI != null)
        {
            Destroy(status.obiektUI);
        }
        aktywneStatusy.Remove(status);
    }

    private void PrzetworzTick(AktywnyStatus status)
    {
        if (status.typ == TypStatusu.Trucizna)
        {
            ZadajObrazenia(status.poziom, status.typ); // Przekazujemy typ do koloru
            status.poziom -= 1;
            if (status.poziom <= 0) UsunStatus(status);
            else if (status.skryptUI != null) status.skryptUI.Aktualizuj(status.poziom, status.pozostalyCzasSekundy, status.maxCzasSekundy);
        }
        else if (status.typ == TypStatusu.Podpalenie)
        {
            ZadajObrazenia(status.poziom, status.typ); // Przekazujemy typ do koloru
            status.poziom = Mathf.CeilToInt(status.poziom / 2f); 
            if (status.poziom <= 0) UsunStatus(status);
            else if (status.skryptUI != null) status.skryptUI.Aktualizuj(status.poziom, status.pozostalyCzasSekundy, status.maxCzasSekundy);
        }
    }

    private void ZadajObrazenia(int ilosc, TypStatusu typ)
    {
        if (statystyki != null)
        {
            statystyki.Health -= ilosc;
            
            Debug.Log($"<color=purple>[STATUS]</color> {gameObject.name} otrzymuje {ilosc} obrażeń od {typ}!");

            // Wybieramy kolor w zależności od statusu
            Color kolorTekstu = (typ == TypStatusu.Podpalenie) ? new Color(1f, 0.4f, 0f) : new Color(0.6f, 0f, 1f); 
            
            // Wywołujemy publiczną funkcję z Decode.cs!
            Decode.ShowFloatingText(transform, $"-{ilosc}", kolorTekstu, false);

            // statystyki.CheckDeath(); // Odkomentuj/dodaj swoją funkcję sprawdzającą śmierć
        }
    }
}