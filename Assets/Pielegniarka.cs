using UnityEngine;

public class Pielegniarka : MonoBehaviour
{
    // Tę funkcję możesz podpiąć pod dowolny przycisk w menu lub dialogu z NPC!
    public void UleczWszystkich()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogWarning("Brak PlayerManagera na scenie!");
            return;
        }

        foreach (var potwor in PlayerManager.Instance.activeParty)
        {
            // Ożywiamy i leczymy każdego członka drużyny do pełna
            potwor.health = potwor.maxHealth;
        }

        Debug.Log("<color=green>Pielęgniarka: Twoja drużyna odzyskała pełnię sił!</color>");
    }
}