using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public TMP_Text textMesh;
    public float moveSpeed = 1f;
    public float fadeDuration = 1f;

    private float timer = 0f;
    private Color startColor;

    public void Initialize(string text, Color color, bool isCrit)
    {
        if (textMesh == null) textMesh = GetComponent<TMP_Text>();

        textMesh.text = text;
        textMesh.color = color;

        if (isCrit)
        {
            transform.localScale *= 1.4f; // Powiększenie dla krytyka
        }

        startColor = color;
    }

    void Update()
    {
        // Unoszenie w górę
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Zanikanie
        timer += Time.deltaTime;
        if (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}