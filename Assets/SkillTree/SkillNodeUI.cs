using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillNodeUI : MonoBehaviour
{
    [Header("Konfiguracja")]
    public string nodeID; 
    public string displayName;
    public int cost;
    public string statToBoost; 
    public int boostAmount;
    
    [Tooltip("Ile razy można kupić tę umiejętność? (Domyślnie 1)")]
    public int maxLevels = 1;

    public bool isStartingNode = false;

    public List<SkillNodeUI> unlocksNodes = new List<SkillNodeUI>();

    [Header("Referencje UI")]
    public Image iconImage;
    public Image bgImage; 
    public Image progressRing; // NOWE: Pierścień postępu wokół ikony
    public TMP_Text nameText;
    public TMP_Text costText;
    public Button button;

    [Header("Kolory Stanów")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color availableColor = Color.white;
    public Color unlockedColor = Color.green; 

    private SkillTreeManager manager;

    void Start()
    {
        if (nameText != null) nameText.text = displayName;
        if (costText != null) costText.text = cost.ToString() + " SP";
    }

    public void Setup(SkillTreeManager mgr, int state, int currentLevel)
    {
        manager = mgr;

        if (bgImage == null || button == null)
        {
            Debug.LogError($"<color=red>[SKILL TREE]</color> W węźle '{gameObject.name}' brakuje podpiętych elementów UI!");
            return; 
        }

        // Aktualizacja okrągłego paska postępu
        if (progressRing != null)
        {
            progressRing.fillAmount = (float)currentLevel / maxLevels;
        }

        if (state == 0) // Zablokowane
        {
            bgImage.color = lockedColor;
            button.interactable = false;
            if (costText != null) costText.color = Color.red;
        }
        else if (state == 1) // Dostępne (można kupować)
        {
            bgImage.color = availableColor;
            button.interactable = true;
            
            // Jeśli już trochę kupiono, pokaż na zielono obecny koszt, by zachęcić gracza
            if (costText != null) 
            {
                costText.color = currentLevel > 0 ? Color.green : Color.yellow;
                costText.text = cost.ToString() + " SP";
            }
        }
        else if (state == 2) // Odblokowane na MAX
        {
            bgImage.color = unlockedColor;
            button.interactable = false;
            if (costText != null) 
            {
                costText.color = Color.green;
                costText.text = "MAX";
            }
        }
    }

    public void OnNodeClicked()
    {
        if (manager != null) manager.SprobujKupicUmiejetnosc(this);
    }
}