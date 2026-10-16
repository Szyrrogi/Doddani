using UnityEngine;

public class BoardActivator : MonoBehaviour
{
    public void ActivateAllShapes()
    {
        // Znajduje wszystkie postawione czary na planszy
        PlacedSpell[] placedSpells = FindObjectsOfType<PlacedSpell>();

        Debug.Log($"--- AKTYWACJA CAŁEJ PLANSZY ({placedSpells.Length} kształtów) ---");

        foreach (PlacedSpell placed in placedSpells)
        {
            placed.ActivateSpell();
        }
    }
}