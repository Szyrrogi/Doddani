using System.Collections.Generic;
using UnityEngine;

// Plansza X na Y, na którą można wkładać kształty zaklęć.
// Umieść ten skrypt na pustym GameObjekcie w miejscu, gdzie ma zaczynać się plansza
// (originWorldPosition można zostawić puste - wtedy użyta zostanie pozycja tego obiektu).
public class SpellBoard : MonoBehaviour
{
    public static SpellBoard Instance { get; private set; }

    [Header("Wymiary planszy")]
    public int width = 8;
    public int height = 8;
    public float cellSize = 1f;

    [Header("Wizualne tło siatki (generowane tak jak kształty)")]
    public GameObject gridCellPrefab;      // prosty kwadrat ze SpriteRenderer (przypisz w Inspectorze)
    public bool generateGridBackground = true;
    public string gridSortingLayerName = "Default";
    public int gridSortingOrder = -10;     // niski order, żeby leżał POD kwadracikami kształtów
    public Color gridColorA = new Color(1f, 1f, 1f, 0.12f); // kolor "parzystych" pól (szachownica)
    public Color gridColorB = new Color(1f, 1f, 1f, 0.06f); // kolor "nieparzystych" pól
    public float gridCellSpacing = 0.02f;  // mały odstęp między kwadracikami, żeby widać było linie siatki

    private Vector3 originWorldPosition;
    private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
    private readonly List<GameObject> gridVisualCells = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        originWorldPosition = transform.position;
    }

    void Start()
    {
        if (generateGridBackground && gridCellPrefab != null)
        {
            GenerateGridBackground();
        }
    }

    // Generuje siatkę tła jako osobne obiekty (dokładnie tak jak podgląd kształtów) -
    // dzięki temu jest widoczna w Game view i w buildzie, w przeciwieństwie do Gizmos.
    public void GenerateGridBackground()
    {
        ClearGridBackground();

        float visualSize = cellSize - gridCellSpacing;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject go = Instantiate(gridCellPrefab, transform);
                go.name = $"GridCell_{x}_{y}";
                go.transform.position = CellToWorldCenter(new Vector2Int(x, y));
                go.transform.localScale = Vector3.one * visualSize;

                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingLayerName = gridSortingLayerName;
                    sr.sortingOrder = gridSortingOrder;
                    // Szachownica dla lepszej czytelności siatki - opcjonalnie
                    sr.color = ((x + y) % 2 == 0) ? gridColorA : gridColorB;
                }

                gridVisualCells.Add(go);
            }
        }
    }

    public void ClearGridBackground()
    {
        foreach (var go in gridVisualCells)
        {
            if (go != null) Destroy(go);
        }
        gridVisualCells.Clear();
    }

    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        Vector3 local = worldPos - originWorldPosition;
        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 CellToWorldCenter(Vector2Int cell)
    {
        return originWorldPosition + new Vector3(
            cell.x * cellSize + cellSize * 0.5f,
            cell.y * cellSize + cellSize * 0.5f,
            0f
        );
    }

    public bool IsInsideBoard(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    public bool IsCellFree(Vector2Int cell)
    {
        return !occupiedCells.Contains(cell);
    }

    // Sprawdza czy CAŁY kształt (lista komórek) mieści się na planszy i nic z nim nie koliduje
    public bool CanPlaceShape(List<Vector2Int> cells)
    {
        foreach (var c in cells)
        {
            if (!IsInsideBoard(c)) return false;
            if (!IsCellFree(c)) return false;
        }
        return true;
    }

    public void OccupyCells(List<Vector2Int> cells)
    {
        foreach (var c in cells)
            occupiedCells.Add(c);
    }

    public void FreeCells(List<Vector2Int> cells)
    {
        foreach (var c in cells)
            occupiedCells.Remove(c);
    }

    // Rysuje siatkę planszy w edytorze, żeby łatwo ją ustawić
    void OnDrawGizmos()
    {
        Vector3 origin = Application.isPlaying ? originWorldPosition : transform.position;
        Gizmos.color = Color.gray;

        for (int x = 0; x <= width; x++)
        {
            Vector3 from = origin + new Vector3(x * cellSize, 0, 0);
            Vector3 to = origin + new Vector3(x * cellSize, height * cellSize, 0);
            Gizmos.DrawLine(from, to);
        }
        for (int y = 0; y <= height; y++)
        {
            Vector3 from = origin + new Vector3(0, y * cellSize, 0);
            Vector3 to = origin + new Vector3(width * cellSize, y * cellSize, 0);
            Gizmos.DrawLine(from, to);
        }
    }
}