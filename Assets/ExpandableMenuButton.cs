using UnityEngine;
using System.Collections.Generic;

public class ExpandableMenuButton : MonoBehaviour
{
    [Header("Elementy do rozwinięcia")]
    [Tooltip("Przeciągnij tutaj obiekty, które mają się pokazać po kliknięciu")]
    public List<GameObject> subItems;

    // Statyczna zmienna - wspólna dla wszystkich przycisków na scenie.
    // Pamięta, który przycisk jest w danej chwili "szefem" (rozwiniętym).
    private static ExpandableMenuButton currentExpandedButton;

    private Vector3 originalScale;
    private bool isExpanded = false;

    void Start()
    {
        // Zapamiętujemy oryginalny rozmiar przycisku (100%)
        originalScale = transform.localScale;
        
        // Na start upewniamy się, że wszystkie podrzędne elementy są ukryte
        SetSubItemsActive(false);
    }

    // Tę funkcję podepniemy pod zdarzenie OnClick() przycisku w Unity
    public void ToggleMenu()
    {
        // Jeśli kliknęliśmy w przycisk, który jest JUŻ rozwinięty -> po prostu go zwijamy
        if (isExpanded)
        {
            Collapse();
            currentExpandedButton = null;
        }
        else
        {
            // Jeśli jakiś INNY przycisk jest obecnie rozwinięty -> wymuszamy jego zwinięcie
            if (currentExpandedButton != null && currentExpandedButton != this)
            {
                currentExpandedButton.Collapse();
            }

            // Rozwijamy NASZ przycisk
            Expand();
        }
    }

    private void Expand()
    {
        isExpanded = true;
        currentExpandedButton = this;

        // Zmniejszamy przycisk o 40% (czyli ustawiamy jego skalę na 60% oryginału)
        transform.localScale = originalScale * 0.6f;

        // Włączamy widoczność ukrytych przycisków
        SetSubItemsActive(true);
    }

    public void Collapse()
    {
        isExpanded = false;

        // Przywracamy przycisk do normalnych rozmiarów
        transform.localScale = originalScale;

        // Ukrywamy podrzędne przyciski
        SetSubItemsActive(false);
    }

    private void SetSubItemsActive(bool state)
    {
        foreach (GameObject item in subItems)
        {
            if (item != null)
            {
                item.SetActive(state);
            }
        }
    }
}