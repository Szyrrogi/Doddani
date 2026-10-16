using UnityEngine;
using TMPro;

public class SpellDescriptionUI : MonoBehaviour
{
    // Singleton, żeby łatwo było się do niego odwołać z dowolnego czaru
    public static SpellDescriptionUI Instance { get; private set; }

    public TMP_Text descriptionText; 
    public string defaultText = ""; // Tekst wyświetlany, gdy na nic nie najeżdżamy
    public GameObject descriptionPanel; // Panel, który można włączać/wyłączać, jeśli chcesz

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        ClearDescription();
    }

    public void ShowDescription(string opis)
    {
        if (descriptionText != null)
        {
            descriptionText.text = opis;
            descriptionPanel.SetActive(true); // Włącz panel, jeśli chcesz go pokazać
        }
    }

    public void ClearDescription()
    {
        if (descriptionText != null)
        {
            // Zamiast wyłączać obiekt (SetActive), po prostu czyścimy tekst
            descriptionText.text = defaultText; 
            descriptionPanel.SetActive(false);
        }
    }
}