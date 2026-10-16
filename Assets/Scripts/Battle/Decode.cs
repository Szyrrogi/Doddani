using System.Collections.Generic;
using UnityEngine;

public static class Decode
{
    /// <summary>
    /// Główna metoda dekodująca i rzucająca zaklęcie.
    /// </summary>
    public static GameObject floatingTextPrefab; 

    public static void ShowFloatingText(Transform targetTransform, string text, Color color, bool isCrit)
    {
        // Jeśli referencja jest pusta, próbujemy ją automatycznie załadować z folderu Resources
        if (floatingTextPrefab == null)
        {
            floatingTextPrefab = Resources.Load<GameObject>("PopUpTekst");
            if (floatingTextPrefab == null)
            {
                Debug.LogWarning("Nie znaleziono prefabu 'PopUpText' w folderze Resources!");
                return;
            }
        }

        Vector3 spawnPos = targetTransform.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.5f, 1f), -0.5f);
        GameObject textObj = Object.Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
        
        FloatingText ft = textObj.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.Initialize(text, color, isCrit);
        }
    }

    public static void ResolveSpell(SpellData data, Stats caster, List<Stats> allies, List<Stats> enemies)
    {
        List<Stats> targets = GetTargets(data.target.ToString(), caster, allies, enemies);
        if (targets.Count == 0) return;

        // ==========================================
        // 1. CZĘŚĆ OBRAŻENIOWA / LECZĄCA
        // ==========================================
        if (data.typEfektu == TypEfektuCzaru.TylkoObrazeniaLeczenie || data.typEfektu == TypEfektuCzaru.Mieszany)
        {
            bool isCrit = false;
            if (data.uderzenie)
            {
                float critRoll = Random.Range(1f, 101f);
                if (critRoll < GetStatValue(caster, "CRITE"))
                {
                    isCrit = true;
                    Debug.Log("<b><color=red>TRAFIENIE KRYTYCZNE!</color></b>");
                }
            }

            float baseAmount = EvaluateFormula(data.ilosc, caster, isCrit);

            foreach (Stats target in targets)
            {
                int finalAmountToApply = 0;

                if (data.uderzenie)
                {
                    float dodgeRoll = Random.Range(1f, 101f);
                    if (dodgeRoll < GetStatValue(target, "DOGE"))
                    {
                        Debug.Log($"{target.gameObject.name} <color=cyan>UNIKNĄŁ</color> ataku!");
                        ShowFloatingText(target.transform, "DOGE", Color.blue, false);
                        continue; 
                    }

                    float attackModifier = (100f + GetStatValue(caster, "ATACK")) / 100f;
                    float modifiedAmount = baseAmount * attackModifier;

                    float armor = GetStatValue(target, "ARMOR");
                    float finalDamageFloat = modifiedAmount * Mathf.Pow(0.99f, armor);
                    
                    finalAmountToApply = Mathf.RoundToInt(finalDamageFloat);

                    float lifeSteal = GetStatValue(caster, "LIFESTEAL");
                    if (lifeSteal > 0)
                    {
                        int healAmount = Mathf.RoundToInt(finalAmountToApply * (lifeSteal / 100f));
                        if (healAmount > 0)
                        {
                            ModifyStat(caster, "HP", healAmount); 
                            Debug.Log($"{caster.gameObject.name} kradnie {healAmount} życia!");
                        }
                    }
                }
                else
                {
                    finalAmountToApply = Mathf.RoundToInt(baseAmount);
                }

                if (data.direction == StatModifierDirection.Zmniejsz)
                    finalAmountToApply = -Mathf.Abs(finalAmountToApply); 
                else if (data.direction == StatModifierDirection.Zwieksz)
                    finalAmountToApply = Mathf.Abs(finalAmountToApply); 

                ModifyStat(target, data.statName, finalAmountToApply);
                Debug.Log($"Zmodyfikowano {data.statName} u {target.gameObject.name} o {finalAmountToApply}.");

                string textPrefix = finalAmountToApply >= 0 ? "+" : "";
                Color textColor = isCrit ? Color.red : Color.white;
                ShowFloatingText(target.transform, $"{textPrefix}{finalAmountToApply}", textColor, isCrit);
            }
        }

        // ==========================================
        // 2. NAKŁADANIE STATUSÓW
        // ==========================================
        if (data.typEfektu == TypEfektuCzaru.TylkoStatus || data.typEfektu == TypEfektuCzaru.Mieszany)
        {
            // Statusy na Cel (Wroga/Sojusznika)
            if (data.statusyNaCel != null && data.statusyNaCel.Count > 0)
            {
                foreach (Stats target in targets)
                {
                    StatusController targetStatus = target.GetComponent<StatusController>();
                    if (targetStatus != null)
                    {
                        foreach (var wzor in data.statusyNaCel)
                        {
                            // Używamy EvaluateFormula, żeby system sam przeliczył np. "3D4" lub "S.ATACK"!
                            int wyliczonyPoz = string.IsNullOrEmpty(wzor.rownaniePoziomu) ? wzor.poziom : Mathf.RoundToInt(EvaluateFormula(wzor.rownaniePoziomu, caster, false));
                            
                            StatusData ns = new StatusData { typ = wzor.typ, typCzasu = wzor.typCzasu, wartoscCzasu = wzor.wartoscCzasu, poziom = wyliczonyPoz, statystyka = wzor.statystyka, ikonaZrodlowa = wzor.ikonaZrodlowa };
                            targetStatus.NalozStatus(ns);
                        }
                    }
                }
            }

            // Statusy na Siebie (Rzucającego)
            if (data.statusyNaSiebie != null && data.statusyNaSiebie.Count > 0)
            {
                StatusController myStatus = caster.GetComponent<StatusController>();
                if (myStatus != null)
                {
                    foreach (var wzor in data.statusyNaSiebie)
                    {
                        int wyliczonyPoz = string.IsNullOrEmpty(wzor.rownaniePoziomu) ? wzor.poziom : Mathf.RoundToInt(EvaluateFormula(wzor.rownaniePoziomu, caster, false));
                        
                        StatusData ns = new StatusData { typ = wzor.typ, typCzasu = wzor.typCzasu, wartoscCzasu = wzor.wartoscCzasu, poziom = wyliczonyPoz, statystyka = wzor.statystyka, ikonaZrodlowa = wzor.ikonaZrodlowa };
                        myStatus.NalozStatus(ns);
                    }
                }
            }
        }
    }

    private static float EvaluateFormula(string formula, Stats caster, bool isCrit)
    {
        float total = 0f;
        
        // Czyszczenie spacji i zamiana minusów na plus z ujemną wartością
        formula = formula.Replace(" ", "").Replace("-", "+-");
        string[] parts = formula.Split(new[] { '+' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string part in parts)
        {
            float multiplier = 1f;
            string expr = part;
            
            if (expr.StartsWith("-"))
            {
                multiplier = -1f;
                expr = expr.Substring(1);
            }

            float partValue = 0f;

            // 1. NAJPIERW SPRAWDZAMY STATYSTYKĘ (żeby uniknąć "D" z nazwy np. DOGE)
            if (expr.StartsWith("S.")) 
            {
                string statExpr = expr.Substring(2);
                string[] statParts = statExpr.Split('%');
                string sName = statParts[0];         
                
                float percent = 100f; 
                if (statParts.Length > 1 && float.TryParse(statParts[1], out float parsedPercent))
                {
                    percent = parsedPercent;
                }

                float sValue = GetStatValue(caster, sName);
                partValue = sValue * (percent / 100f);
                Debug.Log($"[FORMULA] Statystyka {sName} (Wartość: {sValue}, Procent: {percent}%) dała wynik: {partValue}");
            }
            // 2. DOPIERO POTEM SPRAWDZAMY RZUTY KOŚCIĄ
            else if (expr.Contains("D")) 
            {
                string[] diceParts = expr.Split('D');
                int rolls = 1; 
                int sides = 6; 

                if (!string.IsNullOrEmpty(diceParts[0]) && int.TryParse(diceParts[0], out int parsedRolls))
                    rolls = parsedRolls;

                if (diceParts.Length > 1 && !string.IsNullOrEmpty(diceParts[1]) && int.TryParse(diceParts[1], out int parsedSides))
                    sides = parsedSides;

                if (isCrit) rolls *= 2; 

                int diceSum = 0;
                for (int i = 0; i < rolls; i++)
                {
                    diceSum += Random.Range(1, sides + 1);
                }
                partValue = diceSum;
                Debug.Log($"[FORMULA] Rzut {expr} dał wynik: {partValue}");
            }
            // 3. ZWYKŁA LICZBA
            else 
            {
                if (float.TryParse(expr, out float val))
                {
                    partValue = val;
                }
            }

            total += partValue * multiplier;
        }

        Debug.Log($"[FORMULA] Suma końcowa wzoru: {total}");

        return total;
    }
    private static List<Stats> GetTargets(string targetString, Stats caster, List<Stats> allies, List<Stats> enemies)
    {
        List<Stats> targets = new List<Stats>();
        targetString = targetString.ToLower();

        if (targetString == "ty") // Pojedynczy cel (losowy wrog)
        {
            if (enemies.Count > 0)
            {
                int r = Random.Range(0, enemies.Count);
                targets.Add(enemies[r]);
            }
        }
        else if (targetString == "wy") // Wszyscy wrogowie grupowo
        {
            targets.AddRange(enemies);
        }
        else if (targetString == "random")
        {
            if (enemies.Count > 0)
            {
                int r = Random.Range(0, enemies.Count);
                targets.Add(enemies[r]);
            }
        }
        else if (targetString == "my")
        {
            targets.AddRange(allies);
        }
        else if (targetString == "ja")
        {
            targets.Add(caster);
        }

        return targets;
    }

    private static float GetStatValue(Stats stats, string statName)
    {
        switch (statName.ToUpper())
        {
            case "HP": 
            case "HEALTH": return stats.Health;
            case "MAXHP": 
            case "MAXHEALTH": return stats.MaxHealth;
            case "ENERGY": return stats.Energy;
            case "ENERGYMAX": return stats.EnergyMax;
            case "ATACK": return stats.Atack;
            case "CRITE": return stats.Crite;
            case "LIFESTEAL": return stats.LifeSteal;
            case "ARMOR": return stats.Armor;
            case "DOGE": return stats.Doge;
            case "REGEN": return stats.Regen;
            case "FIRE": return stats.Fire;
            case "WATER": return stats.Water;
            case "EARTH": return stats.Earth;
            case "AIR": return stats.Air;
            case "MAGIC": return stats.Magic;
            default:
                Debug.LogWarning($"Nie znaleziono statystyki (Get): {statName}");
                return 0f;
        }
    }

    private static void ModifyStat(Stats stats, string statName, int amount)
    {
        switch (statName.ToUpper())
        {
            case "HP": 
            case "HEALTH":
                stats.Health += amount;
                if (stats.Health > stats.MaxHealth) stats.Health = stats.MaxHealth;
                break;
            case "MAXHP": 
            case "MAXHEALTH": stats.MaxHealth += amount; break;
            case "ENERGY":
                stats.Energy += amount;
                if (stats.Energy > stats.EnergyMax) stats.Energy = stats.EnergyMax;
                break;
            case "ENERGYMAX": stats.EnergyMax += amount; break;
            case "ATACK": stats.Atack += amount; break;
            case "CRITE": stats.Crite += amount; break;
            case "LIFESTEAL": stats.LifeSteal += amount; break;
            case "ARMOR": stats.Armor += amount; break;
            case "DOGE": stats.Doge += amount; break;
            case "REGEN": stats.Regen += amount; break;
            case "FIRE": stats.Fire += amount; break;
            case "WATER": stats.Water += amount; break;
            case "EARTH": stats.Earth += amount; break;
            case "AIR": stats.Air += amount; break;
            case "MAGIC": stats.Magic += amount; break;
            default:
                Debug.LogWarning($"Nie można zmodyfikować statystyki (Set): {statName}");
                break;
        }
    }


}