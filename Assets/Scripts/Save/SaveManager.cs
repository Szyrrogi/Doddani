using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public int aktualnySlot = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // --- INTELIGENTNE USTALANIE ŚCIEŻKI ---
    private string PobierzSciezkeZapisu(int numerSlotu)
    {
        string folder;
        
#if UNITY_EDITOR
        // W edytorze zapisujemy prosto do Assets/Saves (super do testów)
        folder = Path.Combine(Application.dataPath, "Saves");
#else
        // W gotowej grze (plik .exe/.apk) używamy ukrytego folderu gracza
        folder = Application.persistentDataPath;
#endif

        // Upewniamy się, że folder istnieje, zanim spróbujemy tam zapisać plik
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return Path.Combine(folder, $"SaveSlot_{numerSlotu}.json");
    }

    public void ZapiszGre()
    {
        if (PlayerManager.Instance == null) return;

        GameSaveData dane = new GameSaveData
        {
            adventureWejscia = PlayerManager.Instance.adventureWejscia,
            maxWejscia = PlayerManager.Instance.maxWejscia,
            playerMonsters = PlayerManager.Instance.playerMonsters
        };

        // Zamiast zapisywać potwory, zapisujemy ich numery identyfikacyjne
        foreach (var m in PlayerManager.Instance.activeParty)
        {
            int index = PlayerManager.Instance.playerMonsters.IndexOf(m);
            if (index != -1) dane.activePartyIndices.Add(index);
        }

        foreach (var item in PlayerManager.Instance.playerItems)
        {
            if (item != null) dane.playerItemNames.Add(item.itemName);
        }

        string json = JsonUtility.ToJson(dane, true);
        string sciezka = PobierzSciezkeZapisu(aktualnySlot);
        System.IO.File.WriteAllText(sciezka, json);

        Debug.Log($"<color=green>[ZAPIS]</color> Zapisano plik: {sciezka}");
        
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

// --- DODAJ TE DWIE ZMIENNE NA GÓRZE SKRYPTU (pod 'aktualnySlot') ---
    public GameSaveData wczytaneDaneZPamięci;
    public bool czyWymuszonoNowaGre = false;

    // --- PODMIEŃ TE DWIE FUNKCJE ---
    public void WczytajGre(int numerSlotu)
    {
        aktualnySlot = numerSlotu;
        string sciezka = PobierzSciezkeZapisu(numerSlotu);

        if (!File.Exists(sciezka))
        {
            NowaGra();
            return;
        }

        // Zamiast pakować to od razu do Gracza, ładujemy "do plecaka" SaveManagera
        string json = File.ReadAllText(sciezka);
        wczytaneDaneZPamięci = JsonUtility.FromJson<GameSaveData>(json);
        czyWymuszonoNowaGre = false;
        
        Debug.Log($"<color=cyan>[WCZYTYWANIE]</color> Plik ze slotu {numerSlotu} wczytany do pamięci podręcznej. Czekam na pojawienie się Gracza na scenie...");
    }

    private void NowaGra()
    {
        Debug.Log($"<color=yellow>[NOWA GRA]</color> Przygotowanie do stworzenia czystego zapisu na slocie {aktualnySlot}.");
        czyWymuszonoNowaGre = true;
        wczytaneDaneZPamięci = null;
    }

    public bool CzyZapisIstnieje(int numerSlotu)
    {
        return File.Exists(PobierzSciezkeZapisu(numerSlotu));
    }
}