using UnityEngine;
using System.Collections.Generic;

public enum TypPrzedmiotu { Ekwipunek, Mikstura, Material, Klucz }

[CreateAssetMenu(fileName = "Nowy Przedmiot", menuName = "RPG/Przedmiot")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    
    [Header("Kategoria")]
    public TypPrzedmiotu typ = TypPrzedmiotu.Ekwipunek;
    
    [TextArea(2, 3)]
    public string opis = "";
    
    [Header("Dla Ekwipunku / Permamentne Staty dla Mikstur")]
    public List<StatReward> itemStats = new List<StatReward>(); 

    [Header("Tylko dla Mikstur (Jednorazowe)")]
    public int przywrocenieHP;
    
    [Tooltip("Statusy, z którymi potwór ROZPOCZNIE najbliższą walkę (np. Buff)")]
    public List<StatusData> statusyPrzedWalka = new List<StatusData>();
}