using UnityEngine;
using UnityEngine.UI;

public class CombatButtonCooldown : MonoBehaviour
{
    [Header("UI Przycisku")]
    [Tooltip("Jeśli zostawisz puste, skrypt sam znajdzie Button na tym obiekcie")]
    public Button activationButton; 
    [Tooltip("Obrazek typu Filled pokazujący ładowanie")]
    public Image buttonFill; 
    
    [Header("Ustawienia")]
    public float cooldownTime = 3f;

    private float currentTimer = 0f;
    private bool isReady = false;

    void Start()
    {
        if (activationButton == null) 
            activationButton = GetComponent<Button>();

        // Na start walki przycisk jest nieaktywny i zaczyna się ładować
        ResetCooldown();
    }

    void Update()
    {
        if (!isReady)
        {
            currentTimer += Time.deltaTime;
            
            // Wypełnianie paska (od 0.0 do 1.0)
            if (buttonFill != null)
            {
                buttonFill.fillAmount = currentTimer / cooldownTime;
            }

            // Kiedy czas upłynie, odblokowujemy przycisk
            if (currentTimer >= cooldownTime)
            {
                isReady = true;
                if (activationButton != null) activationButton.interactable = true;
            }
        }
    }

    // Tę funkcję podepniemy, żeby przycisk "resetował" sam siebie po kliknięciu
    public void ResetCooldown()
    {
        isReady = false;
        currentTimer = 0f;
        
        if (activationButton != null) activationButton.interactable = false;
        if (buttonFill != null) buttonFill.fillAmount = 0f;
    }
}