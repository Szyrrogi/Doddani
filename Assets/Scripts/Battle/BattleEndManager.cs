using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleEndManager : MonoBehaviour
{
    [Header("Nowy Ekran Wygranej")]
    public VictoryScreen ekranWygranejSkrypt; // Podpinamy nasz nowy skrypt zamiast starego GameObjectu

    private Stats enemyStats;
    private bool walkaZakonczona = false;

    void Start()
    {
        // Wyłączamy ekran na start
        if (ekranWygranejSkrypt != null)
            ekranWygranejSkrypt.gameObject.SetActive(false);
    }

    void Update()
    {
        if (walkaZakonczona) return; 

        // 1. Sprawdzanie czy przeciwnik umarł
        if (enemyStats == null && BattleManager.Instance != null && BattleManager.Instance.enemies.Count > 0)
        {
            if (BattleManager.Instance.enemies[0] != null)
            {
                enemyStats = BattleManager.Instance.enemies[0].GetComponent<Stats>();
            }
        }

        if (enemyStats != null && enemyStats.Health <= 0)
        {
            ZakonczenieWalki();
            return; // Prerywamy, by nie sprawdzać porażki w tej samej klatce
        }

        // 2. NOWE: Sprawdzanie czy cała nasza drużyna umarła
        if (BattleManager.Instance != null && BattleManager.Instance.players.Count > 0)
        {
            bool wszyscyMartwi = true;
            foreach (var p in BattleManager.Instance.players)
            {
                // Jeśli ktokolwiek ma HP powyżej zera, gramy dalej
                if (p != null && p.playerStats != null && p.playerStats.Health > 0)
                {
                    wszyscyMartwi = false;
                    break;
                }
            }

            if (wszyscyMartwi)
            {
                PrzegranaWalka();
            }
        }
    }

    private void PrzegranaWalka()
    {
        walkaZakonczona = true;
        Debug.Log("<color=red>[PORAŻKA]</color> Cała drużyna poległa!");

        // Karzemy gracza: reset EXP i pełne leczenie
        if (PlayerManager.Instance != null)
        {
            foreach (var potwor in PlayerManager.Instance.activeParty)
            {
                potwor.currentExp = 0;
                potwor.health = potwor.maxHealth; 
            }

            // Opcjonalnie: ucieczka z walki również kosztuje połowę wejść do lasu (jak przyciskiem)
            PlayerManager.Instance.adventureWejscia /= 2;
            if (PlayerManager.Instance.adventureWejscia < 0) PlayerManager.Instance.adventureWejscia = 0;

            if (SaveManager.Instance != null) SaveManager.Instance.ZapiszGre();
        }

        // Ładujemy scenę Menu/Bazy (upewnij się, że 0 to indeks Twojego Menu w Build Settings)
        SceneManager.LoadScene(0); 
    }

    private void ZakonczenieWalki()
    {
        walkaZakonczona = true;
        Debug.Log("Zwycięstwo! Przeciwnik pokonany.");

        if (enemyStats != null)
        {
            Enemy enemyComponent = enemyStats.GetComponent<Enemy>();
            // Pobieramy wartość exp z wroga (lub domyślne 50)[cite: 8]
            int zdobyteExp = (enemyComponent != null) ? enemyComponent.expReward : 50; 

            // Uruchamiamy nasz nowy ekran i przekazujemy mu EXP! On sam zajmie się podziałem i animacją.
            if (ekranWygranejSkrypt != null)
            {
                ekranWygranejSkrypt.PokazEkranWygranej(zdobyteExp);
            }

            // Ukrywamy wroga, żeby zniknął z planszy (zamiast Destroy, co jest bezpieczniejsze dla list)
            enemyStats.gameObject.SetActive(false);
        }
    }

    public void WrocDoPrzygody()
    {
        // Najpierw trwale zapisujemy nagrody, EXP i ewentualne straty HP do pliku JSON
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ZapiszGre();
        }

        // Tę metodę możesz podpiąć pod swój nowy przycisk na ekranie wygranej, gdy animacja się skończy!
        SceneManager.LoadScene(2); 
    }
}