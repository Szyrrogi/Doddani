using System.Collections.Generic;
using UnityEngine;
using TMPro; // Niezbędne do obsługi TextMeshPro[cite: 8]

public class Enemy : MonoBehaviour
{
    [Header("Statystyki")]
    public Stats stats; 

    [Header("UI Statystyk (Tekst)")]
    [Tooltip("Przeciągnij tutaj obiekt tekstowy dla Życia przeciwnika")]
    public TMP_Text healthText; 
    
    [Header("UI Paski (Skrypty)")]
    public HealthBar healthBarScript; // <--- NOWE: Odniesienie do skryptu paska zdrowia
    
    // Opcjonalnie, jeśli potwór ma też używać i wyświetlać energię, odkomentuj poniższe:[cite: 8]
    // public TMP_Text energyText;[cite: 8]
    // public HealthBar energyBarScript;

    [Header("Zaklęcia i atakowanie")]
    public List<SpellData> availableSpells; 
    [Tooltip("Co ile sekund przeciwnik rzuca zaklęcie?")]
    public float castCooldown = 4f; 

    private float timer = 0f; 

    public int expReward = 50; 

    void Update()
    {
        // 1. Odświeżamy interfejs w każdej klatce[cite: 8]
        UpdateUI(); 

        // 2. Logika rzucania czarów[cite: 8]
        if (availableSpells == null || availableSpells.Count == 0) return; 

        timer += Time.deltaTime; 
        
        if (timer >= castCooldown) 
        {
            timer = 0f; 
            CastRandomSpell(); 
        }
    }

    private void UpdateUI()
    {
        if (stats == null) return; 

        // Wyświetlanie zdrowia w formacie: Życie / MaxŻycie (zaokrąglone do 1 miejsca po przecinku)[cite: 8]
        if (healthText != null) 
        {
            healthText.text = $"{stats.Health:F0} / {stats.MaxHealth:F0}";  
        }

        // --- AKTUALIZACJA PASKA ZDROWIA WROGA ---
        if (healthBarScript != null)
        {
            healthBarScript.UpdateBar(stats.Health, stats.MaxHealth);
        }

        // Jeśli używasz też energii dla potwora, odkomentuj:[cite: 8]
        // if (energyText != null)[cite: 8]
        // {[cite: 8]
        //     energyText.text = $"{stats.Energy:F1} / {stats.MaxEnergy:F1}";[cite: 8]
        // }[cite: 8]
        // if (energyBarScript != null)
        // {
        //     energyBarScript.UpdateBar(stats.Energy, stats.EnergyMax);
        // }
    }

    private void CastRandomSpell()
    {
        int randomIndex = Random.Range(0, availableSpells.Count); 
        SpellData spellToCast = availableSpells[randomIndex]; 

        Debug.Log($"<color=orange>[ENEMY]</color> {gameObject.name} rzuca zaklęcie: <b>{spellToCast.spellName}</b>!"); 

        // Przekazujemy wylosowane dane i SWOJE statystyki do menedżera walki[cite: 8]
        if (BattleManager.Instance != null) 
        {
            BattleManager.Instance.ExecuteSpell(spellToCast, this.stats); 
        }
    }
}