using System.Collections.Generic;
using UnityEngine;

public class PlacedSpell : MonoBehaviour
{
    public SpellData spellData; 
    public List<Vector2Int> occupiedCells;
    public Stats casterStats; 

    public void Initialize(SpellData data, List<Vector2Int> cells, Stats caster)
    {
        spellData = data;
        occupiedCells = cells;
        casterStats = caster;
    }

    public void ActivateSpell()
    {
        if (spellData == null)
        {
            Debug.LogWarning("[AKTYWACJA] Aktywowano czar bez pełnych danych!");
            return;
        }

        Debug.Log($"<color=green>[BOARD]</color> Aktywacja czaru z planszy: <b>{spellData.spellName}</b>");

        if (BattleManager.Instance != null && casterStats != null)
        {
            BattleManager.Instance.ExecuteSpell(spellData, casterStats);
        }

        // ZWALNIANIE PLANSZY
        if (SpellBoard.Instance != null)
        {
            SpellBoard.Instance.FreeCells(occupiedCells); 
        }

        Destroy(gameObject);
    }
}