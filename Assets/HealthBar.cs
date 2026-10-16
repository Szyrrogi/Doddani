using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Slidery")]
    public Slider mainSlider;
    public Slider easeSlider;

    [Header("Ustawienia Animacji")]
    public float lerpSpeed = 5f;

    [Header("Ustawienia Wyglądu")]
    [Tooltip("Zaznacz, jeśli ten konkretny pasek ma pokazywać ułamki (np. 15.5)")]
    public bool pokazMiejscaPoPrzecinku = false;

    // Tę funkcję będzie wywoływał PlayerBattle, przekazując ile mamy życia/energii i ile wynosi max
    public void UpdateBar(float currentValue, float maxValue)
    {
        // 1. Aktualizacja maksymalnej wartości (gdyby np. potwór awansował)
        if (mainSlider.maxValue != maxValue)
        {
            mainSlider.maxValue = maxValue;
            easeSlider.maxValue = maxValue;
        }

        // 2. Główny pasek skacze natychmiast
        if (mainSlider.value != currentValue)
        {
            mainSlider.value = currentValue;
        }

        // 3. Pasek pod spodem płynnie opada
        if (mainSlider.value != easeSlider.value)
        {
            easeSlider.value = Mathf.Lerp(easeSlider.value, currentValue, lerpSpeed * Time.deltaTime);
        }
    }
}