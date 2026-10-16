using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO; // Wymagane do czytania plików!

public class SaveSlotsUI : MonoBehaviour
{
    [Header("Przyciski Slotów")]
    public Button slot1Btn;
    public TMP_Text slot1Text;
    
    public Button slot2Btn;
    public TMP_Text slot2Text;
    
    public Button slot3Btn;
    public TMP_Text slot3Text;

    void Start()
    {
        OdswiezWygladSlotu(1, slot1Text);
        OdswiezWygladSlotu(2, slot2Text);
        OdswiezWygladSlotu(3, slot3Text);

        if (slot1Btn != null) slot1Btn.onClick.AddListener(() => WybierzSlot(1));
        if (slot2Btn != null) slot2Btn.onClick.AddListener(() => WybierzSlot(2));
        if (slot3Btn != null) slot3Btn.onClick.AddListener(() => WybierzSlot(3));
    }

    private void OdswiezWygladSlotu(int numer, TMP_Text tekst)
    {
        if (tekst == null) return;

        if (SaveManager.Instance != null && SaveManager.Instance.CzyZapisIstnieje(numer))
        {
            // Odtwarzamy ścieżkę do pliku zapisu z SaveManagera
            string folder;
#if UNITY_EDITOR
            folder = Path.Combine(Application.dataPath, "Saves");
#else
            folder = Application.persistentDataPath;
#endif
            string sciezka = Path.Combine(folder, $"SaveSlot_{numer}.json");

            if (File.Exists(sciezka))
            {
                // Odczyt z fizycznego pliku na dysku
                string json = File.ReadAllText(sciezka);
                GameSaveData szybkieDane = JsonUtility.FromJson<GameSaveData>(json);
                
                if (szybkieDane != null)
                {
                    tekst.text = $"Zapis {numer}\nWejścia: {szybkieDane.adventureWejscia}";
                }
                else
                {
                    tekst.text = $"Slot {numer}\n<color=red>BŁĄD ZAPISU</color>";
                }
            }
        }
        else
        {
            tekst.text = $"Slot {numer}\n<color=gray>PUSTY</color>";
        }
    }

    private void WybierzSlot(int numer)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.WczytajGre(numer);
            
            // Zmień "1" na odpowiedni indeks Twojej sceny z lasem w Build Settings (jeśli jest inny)
            SceneManager.LoadScene(0); 
        }
    }
}