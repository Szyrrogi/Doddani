using System.Collections.Generic;
using UnityEngine;

// Wymaga: Collider2D na tym samym obiekcie co Spell (np. BoxCollider2D),
// żeby OnMouseDown/OnMouseDrag/OnMouseUp w ogóle się wywoływały.
[RequireComponent(typeof(Spell))]
public class SpellShapePreview : MonoBehaviour
{
    [Header("Referencja do planszy")]
    public SpellBoard board; // jeśli puste, użyty zostanie SpellBoard.Instance

    [Header("Ustawienia podglądu")]
    public GameObject cellPreviewPrefab;   // prosty kwadrat ze SpriteRenderer (przypisz w Inspectorze)
    public float cellWorldSizeOffBoard = 1f; // rozmiar komórki, gdy podgląd nie leży jeszcze na planszy
    public string sortingLayerName = "Default";
    public int sortingOrder = 10;

    [Header("Kolory")]
    public Color validColor = new Color(0.2f, 1f, 0.2f, 0.6f);   // zielony
    public Color invalidColor = new Color(1f, 0.2f, 0.2f, 0.6f); // czerwony
    public Color offBoardColor = new Color(1f, 1f, 1f, 0.5f);    // biały/neutralny, gdy poza planszą

    private Spell spell;
    private readonly List<GameObject> activeCells = new List<GameObject>();
    private readonly List<SpriteRenderer> activeRenderers = new List<SpriteRenderer>();

    private List<Vector2Int> localShapeCells; // współrzędne kształtu 0..N (z SpellShape)
    private Vector2Int shapeMin;              // lewy-dolny róg bounding-boxu kształtu

    private List<Vector2Int> lastTargetCells = new List<Vector2Int>();
    private bool lastPlacementValid = false;
    private bool isOverBoard = false;
    private bool isDragging = false;
    
    // Zmienna przechowująca początkową pozycję czaru przed przeciągnięciem
    private Vector3 initialLocalPosition;
    private PlayerBattle owner;

    void Awake()
    {
        spell = GetComponent<Spell>();
        owner = GetComponentInParent<PlayerBattle>();
    }

    SpellBoard ActiveBoard => board != null ? board : SpellBoard.Instance;

    void OnMouseDown()
    {
        isDragging = true;
        
        // Zapisujemy pozycję czaru (względem rodzica), żeby nowy czar zrespił się w tym samym miejscu
        initialLocalPosition = transform.localPosition; 
        
        ShowPreview();
    }

    void OnMouseDrag()
    {
        if (isDragging)
            UpdatePreview();
    }

    void OnMouseUp()
    {
        isDragging = false;
        ConfirmOrCancelPlacement();
    }

    // Zabezpieczenie, gdyby przycisk puszczono poza colliderem obiektu
    void Update()
    {
        if (isDragging)
        {
            // Obsługa rotacji scrollem
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                spell.shape.Rotate90();
                ShowPreview(); // Przebuduj podgląd po obrocie
            }

            // Zabezpieczenie na puszczenie przycisku myszy
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                ConfirmOrCancelPlacement();
            }
        }
    }

    // ZMIANA: Zmienna pomocnicza do trzymania "środkowego" klocka, za który trzyma myszka
    private Vector2Int anchorOffset; 

    void ShowPreview()
    {
        ClearVisuals();

        List<Vector2Int> occupied = spell.shape.GetOccupiedCells();
        if (occupied.Count == 0) return;

        localShapeCells = occupied;
        
        // ZMIANA: Szukamy klocka leżącego najbliżej środka całego kształtu
        Vector2 exactCenter = GetShapeCenter(occupied);
        Vector2Int closestCell = occupied[0];
        float minDistance = float.MaxValue;

        foreach (var cell in occupied)
        {
            float dist = Vector2.Distance(exactCenter, new Vector2(cell.x, cell.y));
            if (dist < minDistance)
            {
                minDistance = dist;
                closestCell = cell;
            }
        }
        
        // Zapisujemy offset względem tego środkowego klocka, a nie lewego rogu!
        anchorOffset = closestCell;

        foreach (var cell in occupied)
        {
            GameObject go = Instantiate(cellPreviewPrefab);
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = sortingOrder;
            }
            activeCells.Add(go);
            activeRenderers.Add(sr);
        }

        UpdatePreview();
    }

    void UpdatePreview()
    {
        if (localShapeCells == null || activeCells.Count == 0) return;

        SpellBoard b = ActiveBoard;
        Vector3 mouseWorld = GetMouseWorldPosition();

        if (b == null)
        {
            isOverBoard = false;
            PositionCellsFreeform(mouseWorld);
            SetAllColors(offBoardColor);
            return;
        }

        // ZMIANA: Myszka teraz celuje prosto w znaleziony w ShowPreview "środkowy klocek"
        Vector2Int centerBoardCell = b.WorldToCell(mouseWorld);

        lastTargetCells = new List<Vector2Int>(localShapeCells.Count);
        foreach (var cell in localShapeCells)
        {
            // Odejmujemy pozycję tego wybranego klocka, żeby reszta ułożyła się ładnie dookoła niego
            Vector2Int offset = cell - anchorOffset;
            lastTargetCells.Add(centerBoardCell + offset);
        }

        bool fullyInsideBounds = true;
        foreach (var c in lastTargetCells)
        {
            if (!b.IsInsideBoard(c)) { fullyInsideBounds = false; break; }
        }

        isOverBoard = fullyInsideBounds;

        if (isOverBoard)
        {
            lastPlacementValid = b.CanPlaceShape(lastTargetCells);
            PositionCellsSnapped(b);
            SetAllColors(lastPlacementValid ? validColor : invalidColor);
        }
        else
        {
            lastPlacementValid = false;
            PositionCellsFreeform(mouseWorld);
            SetAllColors(offBoardColor);
        }
    }

    void ConfirmOrCancelPlacement()
    {
        if (isOverBoard && lastPlacementValid && ActiveBoard != null)
        {
            int spellCost = spell.data != null ? spell.data.cost : 0;

            // --- POBIERANIE ENERGII ZE STATYSTYK GRACZA ---
            if (owner != null && owner.playerStats != null)
            {
                if (!owner.playerStats.TrySpendEnergy(spellCost))
                {
                    Debug.Log($"<color=red>Brak energii!</color> Wymagane: {spellCost}, Obecnie masz: {(int)owner.playerStats.Energy}");
                    ClearVisuals(); // Anulujemy położenie
                    return;
                }
            }
            else
            {
                Debug.LogWarning("Brak obiektu PlayerBattle lub nie przypisano PlayerStats w PlayerBattle!");
            }

            // Mamy energię i miejsce na planszy - kładziemy!
            ActiveBoard.OccupyCells(lastTargetCells);
            FinalizePlacedVisual();
        }
        else
        {
            ClearVisuals();
        }
    }

    void FinalizePlacedVisual()
    {
        // Oznaczamy pola jako zajęte
        ActiveBoard.OccupyCells(lastTargetCells);

        // Tworzymy kontener na postawiony na planszy kształt
        GameObject placedShapeObj = new GameObject($"PlacedShape_{spell.spellName}");
        placedShapeObj.transform.SetParent(ActiveBoard.transform);

        // Przypisujemy komponent z informacjami o zajętych komórkach i danych czaru
        // Przypisujemy komponent z informacjami o zajętych komórkach i statystykach rzucającego (owner.playerStats)
        PlacedSpell placedSpell = placedShapeObj.AddComponent<PlacedSpell>();
        // ZMIANA TUTAJ: Dodajemy owner.playerStats jako trzeci argument
        placedSpell.Initialize(spell.data, new List<Vector2Int>(lastTargetCells), owner.playerStats);
        // Przepinamy wygenerowane kwadraciki graficzne do nowego obiektu
        foreach (var go in activeCells)
        {
            if (go != null)
            {
                go.transform.SetParent(placedShapeObj.transform);
                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 1f; // pełna widoczność
                    sr.color = c;
                }
            }
        }

        activeCells.Clear();
        activeRenderers.Clear();
        localShapeCells = null;

        // ==========================================
        // Logika po udanym rzuceniu czaru
        // ==========================================

        // Zapisujemy pozycję startową do zmiennej tymczasowej, bo za chwilę usuniemy ten obiekt
        // ==========================================
        // Logika po udanym rzuceniu czaru
        // ==========================================

        Vector3 spawnPos = initialLocalPosition; //[cite: 13]

        // 1. Sprawdzamy, czy czar jest jednorazowy. Jeśli tak, usuwamy go z dostępnej puli!
        if (owner != null && spell.data != null && spell.data.jednorazowy)
        {
            owner.availableSpells.Remove(spell.data);
            Debug.Log($"<color=orange>[ZAKLĘCIA]</color> Czar {spell.data.spellName} spłonął (jednorazowy). Usunięto z talii!");
        }

        // 2. Zniszcz oryginalny obiekt (ten przeciągany)[cite: 13]
        Destroy(gameObject); //[cite: 13]

        // 3. Wylosuj i stwórz nowy czar[cite: 13]
        if (owner != null) //[cite: 13]
        {
            owner.SpawnSingleRandomSpell(spawnPos); //[cite: 13]
        }
        else
        {
            Debug.LogWarning("Brak owner na scenie!"); //[cite: 13]
        }
    }
    
    void ClearVisuals()
    {
        foreach (var go in activeCells)
        {
            if (go != null) Destroy(go);
        }
        activeCells.Clear();
        activeRenderers.Clear();
        localShapeCells = null;
    }

    void PositionCellsSnapped(SpellBoard b)
    {
        for (int i = 0; i < activeCells.Count; i++)
        {
            activeCells[i].transform.position = b.CellToWorldCenter(lastTargetCells[i]);
            activeCells[i].transform.localScale = Vector3.one * b.cellSize;
        }
    }

    void PositionCellsFreeform(Vector3 centerWorld)
    {
        for (int i = 0; i < activeCells.Count; i++)
        {
            Vector2Int cell = localShapeCells[i];
            
            // ZMIANA: Zamiast GetShapeCenter, używamy fizycznego klocka (anchorOffset), za który złapaliśmy
            Vector3 offset = new Vector3(
                (cell.x - anchorOffset.x) * cellWorldSizeOffBoard,
                (cell.y - anchorOffset.y) * cellWorldSizeOffBoard,
                0f
            );
            
            activeCells[i].transform.position = centerWorld + offset;
            activeCells[i].transform.localScale = Vector3.one * cellWorldSizeOffBoard;
        }
    }

    void SetAllColors(Color color)
    {
        foreach (var sr in activeRenderers)
        {
            if (sr != null) sr.color = color;
        }
    }

    Vector2Int GetMin(List<Vector2Int> cells)
    {
        int minX = int.MaxValue, minY = int.MaxValue;
        foreach (var c in cells)
        {
            if (c.x < minX) minX = c.x;
            if (c.y < minY) minY = c.y;
        }
        return new Vector2Int(minX, minY);
    }

    Vector2 GetShapeCenter(List<Vector2Int> cells)
    {
        float sumX = 0f, sumY = 0f;
        foreach (var c in cells)
        {
            sumX += c.x;
            sumY += c.y;
        }
        return new Vector2(sumX / cells.Count, sumY / cells.Count);
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(mouseScreen);
    }
}