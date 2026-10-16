using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class VictoryScreen : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text zdobytyExpText;

    private int calkowityExp;
    private int expNaGloweGlowny;
    private int rozdanyExpNaGlowe;
    
    private List<MonsterSaveData> zywePotwory = new List<MonsterSaveData>();
    private Coroutine animacja;
    private bool czyAnimuje = false;

    public void PokazEkranWygranej(int zdobytyExpZaWalke)
    {
        gameObject.SetActive(true);
        calkowityExp = zdobytyExpZaWalke;
        zywePotwory.Clear();
        rozdanyExpNaGlowe = 0;

        // Tekst ustawia się natychmiast na pełną pulę
        if (zdobytyExpText != null) zdobytyExpText.text = $"Zdobycz: {calkowityExp} EXP";

        foreach (var potwor in PlayerManager.Instance.activeParty)
        {
            if (potwor.health > 0) zywePotwory.Add(potwor);
        }

        expNaGloweGlowny = zywePotwory.Count > 0 ? calkowityExp / zywePotwory.Count : 0;
        
        animacja = StartCoroutine(AnimujExp());
    }

    private IEnumerator AnimujExp()
    {
        czyAnimuje = true;
        
        // --- NOWE: Opóźnienie przed startem wysysania (1 sekunda) ---
        yield return new WaitForSeconds(1f);

        float czasTrwania = 1f; // równe 1 sekunda trwania samej animacji
        float timer = 0f;

        while (timer < czasTrwania)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / czasTrwania);

            int docelowyExpTeraz = Mathf.RoundToInt(expNaGloweGlowny * progress);
            int expDoDodaniaKlatka = docelowyExpTeraz - rozdanyExpNaGlowe;

            if (expDoDodaniaKlatka > 0)
            {
                rozdanyExpNaGlowe += expDoDodaniaKlatka;
                foreach (var potwor in zywePotwory)
                {
                    DodajExpZLevelUpem(potwor, expDoDodaniaKlatka);
                }
            }

            int zostaloWPuli = calkowityExp - (rozdanyExpNaGlowe * zywePotwory.Count);
            if (zdobytyExpText != null) zdobytyExpText.text = $"Zdobycz: {zostaloWPuli} EXP";

            yield return null;
        }

        ZakonczAnimacje();
    }

    public void PominLubKontynuuj()
    {
        if (czyAnimuje)
        {
            // Przerwanie animacji (lub pauzy) i wgranie reszty z kopa
            if (animacja != null) StopCoroutine(animacja);
            
            int brakujacyExp = expNaGloweGlowny - rozdanyExpNaGlowe;
            if (brakujacyExp > 0)
            {
                foreach (var potwor in zywePotwory)
                {
                    DodajExpZLevelUpem(potwor, brakujacyExp);
                }
            }
            
            ZakonczAnimacje();
        }
        else
        {
            // Pomyślne zamknięcie ekranu
            Debug.Log("Animacja zakończona, ekran czeka na akcję (np. powrót z BattleEndManager)");
        }
    }

    private void ZakonczAnimacje()
    {
        czyAnimuje = false;
        if (zdobytyExpText != null) zdobytyExpText.text = "Zdobycz: 0 EXP";
    }

    private void DodajExpZLevelUpem(MonsterSaveData potwor, int ilosc)
    {
        potwor.currentExp += ilosc;
        
        while (potwor.currentExp >= ExperienceManager.WymaganyExp(potwor.level))
        {
            potwor.currentExp -= ExperienceManager.WymaganyExp(potwor.level);
            potwor.level++;
            
            PlayerBattle kuklaPB = ZnajdzKukleNaScenie(potwor.monsterName);
            if (kuklaPB != null)
            {
                LevelUpRewards rew = kuklaPB.GetComponent<LevelUpRewards>();
                if (rew != null) rew.PrzyznajNagrody(potwor, kuklaPB.playerStats);
                else potwor.LoadToStats(kuklaPB.playerStats);
            }
        }
    }

    private PlayerBattle ZnajdzKukleNaScenie(string nazwaDoSzukania)
    {
        string cleanName = nazwaDoSzukania.Replace("(Clone)", "").Trim();
        PlayerBattle[] wszystkieKukly = FindObjectsOfType<PlayerBattle>();
        
        foreach (var kukla in wszystkieKukly)
        {
            if (kukla.gameObject.name.Replace("(Clone)", "").Trim() == cleanName) return kukla;
        }
        return null;
    }
}