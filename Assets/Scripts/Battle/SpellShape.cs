using System;
using System.Collections.Generic;
using UnityEngine;

// Kształt zaklęcia zapisany jako siatka 5x5 (jak klocek w Tetrisie)
[Serializable]
public class SpellShape
{
    public const int GridSize = 5;

    // Płaska tablica bool: index = y * GridSize + x
    public bool[] cells = new bool[GridSize * GridSize];

    public bool GetCell(int x, int y)
    {
        return cells[y * GridSize + x];
    }

    public void SetCell(int x, int y, bool value)
    {
        cells[y * GridSize + x] = value;
    }

    // Zwraca listę zajętych komórek (przydatne np. do generowania ataku)
    public List<Vector2Int> GetOccupiedCells()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (GetCell(x, y))
                    result.Add(new Vector2Int(x, y));
            }
        }
        return result;
    }

    public void Clear()
    {
        for (int i = 0; i < cells.Length; i++)
            cells[i] = false;
    }

    public void Rotate90()
    {
        bool[] rotated = new bool[GridSize * GridSize];
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                // Obrót o 90 stopni w prawo
                int newX = GridSize - 1 - y;
                int newY = x;
                rotated[newY * GridSize + newX] = GetCell(x, y);
            }
        }
        cells = rotated;
    }
}
