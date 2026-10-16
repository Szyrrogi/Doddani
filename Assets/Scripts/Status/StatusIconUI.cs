using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusIconUI : MonoBehaviour
{
    public Image iconImage;
    public Image cooldownRing; // Obrazek ustawiony na Radial 360
    public TMP_Text levelText;

    public void Aktualizuj(int poziom, float obecnyCzas, float maxCzas)
    {
        if (levelText != null) levelText.text = poziom.ToString();
        
        // Zmniejszanie kółka z czasem (wartość od 0 do 1)
        if (cooldownRing != null && maxCzas > 0)
        {
            cooldownRing.fillAmount = obecnyCzas / maxCzas;
        }
    }
}