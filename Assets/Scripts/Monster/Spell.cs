using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI; 

public class Spell : MonoBehaviour
{
    [Header("Dane zaklęcia")]
    public SpellData data;

    [Header("Kształt na planszy (Instancja)")]
    public SpellShape shape = new SpellShape();

    [Header("Elementy UI / Tekst")]
    public TextMeshPro spellNameText; 
    public TextMeshPro spellCostText; 

    [Header("Wizualizacja Ładowania (Kolory)")]
    public SpriteRenderer cardGraphic; 
    public Color normalColor = Color.white;
    public Color disabledColor = new Color(0.4f, 0.4f, 0.4f, 1f); 

    [Header("Wizualizacja Ładowania (Pasek)")]
    public Image fillBarImage; 

    [Header("Miniatura kształtu")]
    public Transform miniShapeContainer; 
    public GameObject miniCellPrefab;    
    public float miniCellSpacing = 0.2f; 

    public string spellName => data != null ? data.spellName : "Brak danych";

    private Stats playerStats;

    private void Start()
    {
        PlayerBattle pb = GetComponentInParent<PlayerBattle>();
        if (pb != null)
        {
            playerStats = pb.playerStats;
        }
    }

    private void Update()
    {
        if (playerStats == null || data == null) return;

        float currentEnergy = playerStats.Energy;
        float requiredEnergy = data.cost;

        float fillRatio = Mathf.Clamp01(currentEnergy / requiredEnergy);

        if (fillBarImage != null)
        {
            fillBarImage.fillAmount = fillRatio;
        }

        if (cardGraphic != null)
        {
            cardGraphic.color = (currentEnergy >= requiredEnergy) ? normalColor : disabledColor;
        }
    }

    public void Initialize(SpellData newData) 
    {
        data = newData; 
        
        if (spellNameText != null) 
            spellNameText.text = data.spellName; 
            
        if (spellCostText != null) 
            spellCostText.text = data.cost.ToString();  

        if (newData.shapeTemplate != null && newData.shapeTemplate.cells != null) 
        {
            shape.cells = (bool[])newData.shapeTemplate.cells.Clone(); 
        }

        GenerateMiniShape(); 
    }

    private void GenerateMiniShape() 
    {
        if (miniShapeContainer == null || miniCellPrefab == null) return; 

        foreach (Transform child in miniShapeContainer) Destroy(child.gameObject); 

        List<Vector2Int> occupiedCells = shape.GetOccupiedCells(); 
        if (occupiedCells.Count == 0) return; 

        Vector2 center = Vector2.zero; 
        foreach (var cell in occupiedCells) 
        {
            center.x += cell.x; 
            center.y += cell.y; 
        }
        center /= occupiedCells.Count; 

        foreach (var cell in occupiedCells) 
        {
            GameObject miniCell = Instantiate(miniCellPrefab, miniShapeContainer); 
            float localX = (cell.x - center.x) * miniCellSpacing; 
            float localY = (cell.y - center.y) * miniCellSpacing; 
            miniCell.transform.localPosition = new Vector3(localX, localY, 0f); 
        }
    }

    // ==========================================
    // OBSŁUGA KURSORA I OPISU
    // ==========================================

    private void OnMouseEnter() 
    {
        if (SpellDescriptionUI.Instance != null && data != null) 
        {
            string finalDescription = data.description;

            if (string.IsNullOrWhiteSpace(finalDescription))
            {
                finalDescription = GenerujAutomatycznyOpis();
            }

            SpellDescriptionUI.Instance.ShowDescription(finalDescription); 
        }
    }

    private void OnMouseExit() 
    {
        if (SpellDescriptionUI.Instance != null) 
        {
            SpellDescriptionUI.Instance.ClearDescription(); 
        }
    }

    // ==========================================
    // GENERATOR OPISU (Zaktualizowany o statusy)
    // ==========================================

    private string GenerujAutomatycznyOpis()
    {
        string finalnyOpis = "";

        // 1. CZĘŚĆ OBRAŻENIOWA / LECZĄCA / WZMACNIAJĄCA
        if (data.typEfektu == TypEfektuCzaru.TylkoObrazeniaLeczenie || data.typEfektu == TypEfektuCzaru.Mieszany)
        {
            // Nowy blok: Jeśli to nie jest typowe HP/Atak, tylko po prostu manipulacja statystyką (jak Twój Regen)
            if (data.statName != "S.Health" && data.statName != "HP" && !data.uderzenie)
            {
                string akcja = data.direction == StatModifierDirection.Zwieksz ? "Zwiększa" : "Zmniejsza";
                string czytelnaNazwaStatystyki = PobierzCzytelnaNazweStatystyki(data.statName);
                
                finalnyOpis += $"{akcja} {czytelnaNazwaStatystyki} o <color=#55FF55>{data.ilosc}</color>.\n";
            }
            else // Oryginalny blok dla rzutów kostką, obrażeń i leczenia
            {
                int minDice = 0, maxDice = 0;
                bool rzutKostka = false;

                string oryginalnaIlosc = string.IsNullOrWhiteSpace(data.ilosc) ? "0" : data.ilosc;
                string[] oryginalneElementy = oryginalnaIlosc.Split('+');
                string oryginalnyDicePart = oryginalneElementy[0].Trim();

                string iloscStr = oryginalnaIlosc.ToLower().Trim();
                string[] elementyRownania = iloscStr.Split('+');
                string dicePart = elementyRownania[0].Trim();

                if (dicePart.Contains("d"))
                {
                    string[] parts = dicePart.Split('d');
                    if (parts.Length == 2)
                    {
                        int.TryParse(WyczyscZLiter(parts[0]), out int numberOfDice);
                        int.TryParse(WyczyscZLiter(parts[1]), out int diceSides);
                        minDice = numberOfDice; 
                        maxDice = numberOfDice * diceSides; 
                        rzutKostka = true;
                    }
                }
                else
                {
                    int.TryParse(WyczyscZLiter(dicePart), out int flatValue);
                    minDice = flatValue;
                    maxDice = flatValue;
                }

                int statBonusFlat = 0;
                string zawartoscNawiasu = oryginalnyDicePart.ToUpper(); 

                if (elementyRownania.Length > 1)
                {
                    string statPart = elementyRownania[1].Trim(); 
                    string oryginalnyStatPart = oryginalneElementy[1].Trim(); 

                    if (statPart.Contains("%"))
                    {
                        string[] statSplit = statPart.Split('%');
                        string statNameRaw = statSplit[0].Trim(); 
                        int.TryParse(WyczyscZLiter(statSplit[1]), out int percentMnoznik); 
                        
                        int statValue = PobierzWartoscStatystyki(statNameRaw); 
                        statBonusFlat = Mathf.FloorToInt(statValue * (percentMnoznik / 100f));

                        string[] oryginalnyStatSplit = oryginalnyStatPart.Split('%');
                        string spriteName = oryginalnyStatSplit[0].Trim(); 

                        if (spriteName.StartsWith("S.") || spriteName.StartsWith("s."))
                        {
                            spriteName = spriteName.Substring(2).ToUpper(); 
                        }
                        
                        string procentStr = oryginalnyStatSplit[1].Trim(); 
                        zawartoscNawiasu = $"{oryginalnyDicePart.ToUpper()} + <sprite name=\"{spriteName}\"> {procentStr}%";
                    }
                }

                if (playerStats == null) playerStats = GetComponentInParent<Stats>();

                int glownaStatystyka = 0;
                if (playerStats != null)
                {
                    glownaStatystyka = data.uderzenie ? playerStats.Atack : playerStats.Magic; 
                }

                float glownyMnoznik = 1f + (glownaStatystyka / 100f);
                int minTotal = Mathf.FloorToInt((minDice + statBonusFlat) * glownyMnoznik);
                int maxTotal = Mathf.FloorToInt((maxDice + statBonusFlat) * glownyMnoznik);

                string elementEnumStr = data.element.ToString().ToUpper();
                string actionWord = data.uderzenie ? "Zadaje" : "Przywraca";
                string typeWord = data.uderzenie ? PobierzNazweObrazen(elementEnumStr) : "punktów życia";
                string hexKolor = data.uderzenie ? PobierzKodKoloru(elementEnumStr) : "#55FF55"; 
                
                string koloroweLiczby = rzutKostka ? $"<color={hexKolor}>{minTotal}-{maxTotal}</color>" : $"<color={hexKolor}>{minTotal}</color>";

                finalnyOpis += $"{actionWord} {koloroweLiczby} ({zawartoscNawiasu}) {typeWord}.\n";
            }
        }

        // 2. CZĘŚĆ STATUSOWA
        if (data.typEfektu == TypEfektuCzaru.TylkoStatus || data.typEfektu == TypEfektuCzaru.Mieszany)
        {
            if (data.statusyNaCel.Count > 0)
            {
                finalnyOpis += "<color=yellow>Na cel:</color> ";
                foreach (var s in data.statusyNaCel)
                {
                    string wartoscPoz = string.IsNullOrEmpty(s.rownaniePoziomu) ? s.poziom.ToString() : s.rownaniePoziomu;
                    finalnyOpis += $"{s.typ} (Poz. {wartoscPoz}) na {s.wartoscCzasu} {s.typCzasu}. ";
                }
                finalnyOpis += "\n";
            }

            if (data.statusyNaSiebie.Count > 0)
            {
                finalnyOpis += "<color=cyan>Na rzucającego:</color> ";
                foreach (var s in data.statusyNaSiebie)
                {
                    string wartoscPoz = string.IsNullOrEmpty(s.rownaniePoziomu) ? s.poziom.ToString() : s.rownaniePoziomu;
                    finalnyOpis += $"{s.typ} (Poz. {wartoscPoz}) na {s.wartoscCzasu} {s.typCzasu}. ";
                }
            }
        }
        
        // NOWE: Informacja o jednorazowości
        if (data.jednorazowy)
        {
            finalnyOpis += "\n<color=orange>Zaklęcie jednorazowe.</color>";
        }

        return finalnyOpis.TrimEnd(); 
    }

    // Nowa funkcja pomocnicza zamieniająca systemowe nazwy statystyk na czytelny polski tekst
    private string PobierzCzytelnaNazweStatystyki(string surowaNazwa)
    {
        string u = surowaNazwa.ToUpper();
        if (u.Contains("ATACK")) return "Siłę Ataku";
        if (u.Contains("ARMOR")) return "Pancerz";
        if (u.Contains("LIFESTEAL")) return "Kradzież Życia";
        if (u.Contains("REGEN")) return "Regenerację";
        if (u.Contains("DOGE") || u.Contains("DODGE")) return "Unik";
        if (u.Contains("MAGIC")) return "Magię";
        if (u.Contains("FIRE")) return "Obrażenia Ognia";
        if (u.Contains("WATER")) return "Obrażenia Wody";
        if (u.Contains("EARTH")) return "Obrażenia Ziemi";
        if (u.Contains("AIR")) return "Obrażenia Powietrza";
        
        // Jeśli nie rozpozna, zwróci np. "Regen" z usuniętym "S."
        return surowaNazwa.Replace("S.", "");
    }

    private string WyczyscZLiter(string wejscie)
    {
        string wynik = "";
        foreach (char c in wejscie) if (char.IsDigit(c)) wynik += c;
        return wynik;
    }

    private int PobierzWartoscStatystyki(string nazwaStatystyki)
    {
        if (playerStats == null) playerStats = GetComponentInParent<Stats>();
        if (playerStats == null) return 0; 

        string upper = nazwaStatystyki.ToUpper();
        
        if (upper.Contains("ATACK") || upper.Contains("ATAK")) return playerStats.Atack;
        if (upper.Contains("MAXHEALTH")) return playerStats.MaxHealth;
        if (upper.Contains("HEALTH") || upper.Contains("HP")) return (int)playerStats.Health;
        if (upper.Contains("ARMOR")) return playerStats.Armor;
        if (upper.Contains("LIFESTEAL")) return playerStats.LifeSteal;
        if (upper.Contains("REGEN")) return playerStats.Regen;
        if (upper.Contains("DOGE")) return playerStats.Doge;
        if (upper.Contains("FIRE")) return playerStats.Fire;
        if (upper.Contains("WATER")) return playerStats.Water;
        if (upper.Contains("EARTH")) return playerStats.Earth;
        if (upper.Contains("AIR")) return playerStats.Air;
        if (upper.Contains("MAGIC")) return playerStats.Magic;
        
        return 0;
    }

    private string PobierzNazweObrazen(string elementName)
    {
        switch (elementName)
        {
            case "FIRE": case "OGIEN": case "OGIEŃ": return "obrażeń ognia";
            case "WATER": case "WODA": return "obrażeń wody";
            case "EARTH": case "ZIEMIA": return "obrażeń ziemi";
            case "AIR": case "POWIETRZE": return "obrażeń powietrza";
            case "MAGIC": case "MAGIA": return "obrażeń magicznych";
            case "DARK": case "MROK": return "obrażeń mroku";
            case "ZWYKLY": case "ZWYKŁY": case "NORMAL": case "PHYSICAL": return "obrażeń fizycznych";
            default: return "obrażeń"; 
        }
    }

    private string PobierzKodKoloru(string elementName)
    {
        switch (elementName)
        {
            case "FIRE": return "#FF5555";
            case "WATER": return "#5555FF";
            case "EARTH": return "#55AA55";
            case "AIR": return "#AAAAFF";
            case "MAGIC": return "#AA55AA";
            case "DARK": return "#8822AA";
            case "ZWYKLY": return "#DDDDDD";
            default: return "#FFFFFF"; 
        }
    }

    // ==========================================
    // FIZYCZNE RZUCANIE CZARU W WALCE
    // ==========================================
    
    // Zadzwoń do tej funkcji z Menedżera Walki, kiedy gracz puści czar na konkretne pole/cel!
    public void RzucCzar(GameObject cel)
    {
        Debug.Log($"<color=yellow>[SPELL DEBUG]</color> Rozpoczęto RzucCzar: {data?.spellName}. Cel: {(cel != null ? cel.name : "NULL")}");

        if (data == null) 
        {
            Debug.LogError("<color=red>[SPELL DEBUG]</color> Przerwano! 'data' jest puste (nie przypisano czaru).");
            return;
        }

        if (data.typEfektu == TypEfektuCzaru.TylkoObrazeniaLeczenie || data.typEfektu == TypEfektuCzaru.Mieszany)
        {
            Debug.Log($"<color=yellow>[SPELL DEBUG]</color> Obliczam obrażenia/leczenie...");
        }

        // Nakładanie Statusów
        if (data.typEfektu == TypEfektuCzaru.TylkoStatus || data.typEfektu == TypEfektuCzaru.Mieszany)
        {
            Debug.Log($"<color=yellow>[SPELL DEBUG]</color> Typ efektu pozwala na statusy. Sprawdzam cele...");

            StatusController targetStatus = cel != null ? cel.GetComponent<StatusController>() : null;
            StatusController myStatus = GetComponentInParent<StatusController>();

            if (targetStatus == null) Debug.LogWarning("<color=orange>[SPELL DEBUG]</color> Cel nie ma podpiętego skryptu StatusController!");
            else Debug.Log($"<color=yellow>[SPELL DEBUG]</color> Znaleziono StatusController na celu: {cel.name}. Statusów do nałożenia: {data.statusyNaCel.Count}");

            if (myStatus == null) Debug.LogWarning("<color=orange>[SPELL DEBUG]</color> Rzucający (Ja) nie ma podpiętego skryptu StatusController!");
            else Debug.Log($"<color=yellow>[SPELL DEBUG]</color> Znaleziono StatusController na rzucającym. Statusów do nałożenia: {data.statusyNaSiebie.Count}");

            // Nakładanie na wroga
            if (targetStatus != null && data.statusyNaCel.Count > 0)
            {
                foreach (var wzor in data.statusyNaCel)
                {
                    int wyliczonyPoz = string.IsNullOrEmpty(wzor.rownaniePoziomu) ? wzor.poziom : PobierzWartoscStatystyki(wzor.rownaniePoziomu);
                    Debug.Log($"<color=yellow>[SPELL DEBUG]</color> Nakładam na cel: {wzor.typ} ostateczny poziom: {wyliczonyPoz}");
                    
                    StatusData ns = new StatusData { typ = wzor.typ, typCzasu = wzor.typCzasu, wartoscCzasu = wzor.wartoscCzasu, poziom = wyliczonyPoz, statystyka = wzor.statystyka, ikonaZrodlowa = wzor.ikonaZrodlowa };
                    targetStatus.NalozStatus(ns);
                }
            }

            // Nakładanie na siebie
            if (myStatus != null && data.statusyNaSiebie.Count > 0)
            {
                foreach (var wzor in data.statusyNaSiebie)
                {
                    int wyliczonyPoz = string.IsNullOrEmpty(wzor.rownaniePoziomu) ? wzor.poziom : PobierzWartoscStatystyki(wzor.rownaniePoziomu);
                    Debug.Log($"<color=yellow>[SPELL DEBUG]</color> Nakładam na siebie: {wzor.typ} ostateczny poziom: {wyliczonyPoz}");

                    StatusData ns = new StatusData { typ = wzor.typ, typCzasu = wzor.typCzasu, wartoscCzasu = wzor.wartoscCzasu, poziom = wyliczonyPoz, statystyka = wzor.statystyka, ikonaZrodlowa = wzor.ikonaZrodlowa };
                    myStatus.NalozStatus(ns);
                }
            }
        }
        else
        {
            Debug.LogWarning($"<color=orange>[SPELL DEBUG]</color> Czar '{data.spellName}' ma ustawiony typ '{data.typEfektu}', więc ignoruje listę statusów!");
        }
    }
}