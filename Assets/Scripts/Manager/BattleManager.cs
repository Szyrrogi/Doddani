using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    // Singleton
    public static BattleManager Instance { get; private set; }

    [Header("Uczestnicy walki (Automatyczne)")]
    public List<PlayerBattle> players = new List<PlayerBattle>(); //[cite: 3]
    public List<Enemy> enemies = new List<Enemy>(); //[cite: 3]

    [Header("Spawnowanie Drużyny i Wroga")]
    public Transform[] playerSpawnPoints; // Miejsca dla Twoich 3 potworów
    public Transform enemySpawnPoint;     // Miejsce dla przeciwnika

    void Awake()
    {
        if (Instance == null) Instance = this; //[cite: 3]
        else Destroy(gameObject); //[cite: 3]
    }

    void Start()
    {
        // Na start sceny ładujemy postacie!
        SpawnujDruzyne();
        SpawnujWroga();
    }


   private void SpawnujDruzyne()
    {
        Debug.Log("<color=magenta>--- ROZPOCZYNAM SPAWNOWANIE DRUŻYNY W WALCE ---</color>");

        if (PlayerManager.Instance == null)
        {
            Debug.LogError("<color=red>[BATTLE]</color> PlayerManager.Instance to NULL! Prawdopodobnie uruchomiłeś grę prosto ze sceny 'Walka'. Uruchom grę od sceny Menu/Adventure, gdzie znajduje się obiekt PlayerManager.");
            return;
        }

        Debug.Log($"<color=cyan>[BATTLE]</color> Znaleziono PlayerManager. Liczba potworów w activeParty: {PlayerManager.Instance.activeParty.Count}");

        if (playerSpawnPoints == null || playerSpawnPoints.Length == 0)
        {
            Debug.LogError("<color=red>[BATTLE]</color> BŁĄD: Lista 'Player Spawn Points' w Inspektorze jest PUSTA (Length 0). Skrypt nie ma gdzie postawić potworów!");
            return;
        }
        
        Debug.Log($"<color=cyan>[BATTLE]</color> Liczba dostępnych miejsc spawnu (Spawn Points): {playerSpawnPoints.Length}");

        for (int i = 0; i < PlayerManager.Instance.activeParty.Count; i++)
        {
            if (i >= playerSpawnPoints.Length)
            {
                Debug.LogWarning($"<color=orange>[BATTLE]</color> Brak punktów spawnu dla potwora nr {i}! Przewidziano za mało miejsc na planszy.");
                break;
            }

            MonsterSaveData savedData = PlayerManager.Instance.activeParty[i];
            Debug.Log($"<color=yellow>[BATTLE]</color> Próba zespawnowania potwora z zapisu o nazwie: <b>'{savedData.monsterName}'</b>");

            GameObject prefab = ZnajdzPrefab(savedData.monsterName);

           if (prefab != null)
            {
                GameObject potwor = Instantiate(prefab, playerSpawnPoints[i].position, Quaternion.identity);
                Stats staty = potwor.GetComponent<Stats>();

                if (staty != null)
                {
                    Debug.Log($"<color=orange>[BATTLE]</color> Przed LoadToStats -> HP na scenie: {staty.Health}, Lvl: {staty.Level}");
                    savedData.LoadToStats(staty);
                    Debug.Log($"<color=green>[BATTLE]</color> Po LoadToStats -> HP na scenie: {staty.Health}, Lvl: {staty.Level}, Atak: {staty.Atack}");
                }

                PlayerBattle pb = potwor.GetComponent<PlayerBattle>();
                if (pb != null)
                {
                    // Wymuszamy stuprocentowe połączenie UI z walczącym potworem
                    pb.WymusPrzypisanieDanych(savedData);
                    players.Add(pb);
                }
            }
            else
            {
                Debug.LogError($"<color=red>[BATTLE]</color> Zwrócono pusty prefab dla '{savedData.monsterName}'. Szukanie nie powiodło się!");
            }
        }
    }

    private GameObject ZnajdzPrefab(string nazwa)
    {
        string szukanaNazwa = nazwa.Replace("(Clone)", "").Trim();
        Debug.Log($"<color=grey>[BATTLE - ZnajdzPrefab]</color> Szukam nazwy: '{szukanaNazwa}'. Baza wszystkich potworów ma rozmiar: {PlayerManager.Instance.bazaWszystkichPotworow.Count}");

        foreach (var p in PlayerManager.Instance.bazaWszystkichPotworow) // Zmiana na global
        {
            if (p == null) 
            {
                Debug.LogWarning("<color=orange>[BATTLE - ZnajdzPrefab]</color> Jedno z miejsc w liście 'Baza Wszystkich Potworow' w Inspektorze jest puste!");
                continue;
            }
            
            string nazwaWPuli = p.name.Replace("(Clone)", "").Trim();
            Debug.Log($"<color=grey>[BATTLE - ZnajdzPrefab]</color> Porównuję z prefabem w bazie: '{nazwaWPuli}'...");

            if (nazwaWPuli == szukanaNazwa)
            {
                Debug.Log($"<color=green>[BATTLE - ZnajdzPrefab]</color> Znaleziono pasujący prefab!");
                return p;
            }
        }
        
        Debug.LogError($"<color=red>[BATTLE - ZnajdzPrefab]</color> BŁĄD: Nie znaleziono dopasowania dla '{szukanaNazwa}' w liście prefabów w PlayerManager!");
        return null;
    }

    private void SpawnujWroga()
    {
        Debug.Log("<color=magenta>--- ROZPOCZYNAM SPAWNOWANIE WROGA ---</color>");

        // 1. Sprawdzamy czy istnieje PlayerManager
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("<color=red>[BATTLE]</color> BŁĄD: PlayerManager jest NULL! Odpal grę od sceny Przygody.");
            return;
        }

        // 2. Sprawdzamy, czy w pamięci zapisano jakiegoś przeciwnika
        if (PlayerManager.Instance.wybranyPrzeciwnikPrefab == null)
        {
            Debug.LogError("<color=red>[BATTLE]</color> BŁĄD: 'wybranyPrzeciwnikPrefab' jest pusty (NULL)! Albo AdventureManager go nie wylosował, albo odpaliłeś scenę walki z pominięciem eksploracji.");
            return;
        }

        Debug.Log($"<color=cyan>[BATTLE]</color> Próba spawnu wroga: <b>{PlayerManager.Instance.wybranyPrzeciwnikPrefab.name}</b>");
        
        // Tworzymy przeciwnika wylosowanego w Adventure
        GameObject wrog = Instantiate(PlayerManager.Instance.wybranyPrzeciwnikPrefab, enemySpawnPoint.position, Quaternion.identity);
        
        // 3. Dodajemy go do listy wrogów i upewniamy się, że ma przypięty skrypt Enemy
        Enemy enemyComponent = wrog.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemies.Add(enemyComponent);
            Debug.Log($"<color=green>[BATTLE]</color> SUKCES! Wróg dodany do listy enemies. Aktualna liczba wrogów na liście: {enemies.Count}");
        }
        else
        {
            Debug.LogError($"<color=red>[BATTLE]</color> BŁĄD: Zespawnowany prefab '{wrog.name}' NIE MA przypiętego skryptu 'Enemy.cs'! Dodaj ten skrypt do prefabu potwora w Inspektorze.");
        }
    }


    /// <summary>
    /// Główna metoda przepuszczająca zaklęcie przez dekoder (Decode.cs) //[cite: 3]
    /// </summary>
    public void ExecuteSpell(SpellData spellData, Stats caster)
    {
        if (spellData == null || caster == null) 
        {
            Debug.LogWarning("Brak danych czaru lub rzucającego!"); 
            return; 
        }

        List<Stats> playerStats = new List<Stats>(); 
        foreach (var p in players)  
        {
            if (p != null && p.playerStats != null) playerStats.Add(p.playerStats); 
        }

        List<Stats> enemyStats = new List<Stats>(); 
        foreach (var e in enemies)  
        {
            if (e != null && e.stats != null) enemyStats.Add(e.stats); 
        }

        List<Stats> allies; 
        List<Stats> targetEnemies; 

        if (playerStats.Contains(caster)) 
        {
            allies = playerStats; 
            targetEnemies = enemyStats; 
        }
        else 
        {
            allies = enemyStats; 
            targetEnemies = playerStats; 
        }

        // ZMIANA: Przekazujemy całe 'spellData' do Decode!
        Decode.ResolveSpell(spellData, caster, allies, targetEnemies); 
    }
}