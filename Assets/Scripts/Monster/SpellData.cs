using UnityEngine;
using System.Collections.Generic;

// Cel działania zaklęcia
public enum SpellTarget { Ty, Wy, Ja, My, Random }
public enum SpellElement { Zwykly, Woda, Ogien, Ziemia, Powietrze, Swiatlo, Cien }
public enum StatModifierDirection { Zwieksz, Zmniejsz }

// NOWE: Wybór, jak ma zachować się czar
public enum TypEfektuCzaru { TylkoObrazeniaLeczenie, TylkoStatus, Mieszany }

[CreateAssetMenu(fileName = "NowySpell", menuName = "Spelle/Spell Data")]
public class SpellData : ScriptableObject
{
    [Header("Podstawowe informacje")]
    public string spellName = "Nowy Spell";

    [TextArea(3, 6)]
    public string description = "";

    public int cost = 1;

    [Header("Cel i typ")]
    public SpellTarget target = SpellTarget.Ja;
    public SpellElement element = SpellElement.Zwykly;

    [Header("Ustawienia rzucania")]
    public bool jednorazowy = false;

    [Header("RODZAJ CZARU")]
    public TypEfektuCzaru typEfektu = TypEfektuCzaru.TylkoObrazeniaLeczenie;

    [Header("Opcja 1: Obrażenia i Leczenie")]
    public StatModifierDirection direction = StatModifierDirection.Zwieksz;
    public string statName;
    public bool uderzenie;   
    public string ilosc = "0"; 

    [Header("Opcja 2: Nakładane Statusy")]
    public List<StatusData> statusyNaCel = new List<StatusData>();
    public List<StatusData> statusyNaSiebie = new List<StatusData>();

    [Header("Kształt Zaklęcia")]
    public SpellShape shapeTemplate = new SpellShape(); 
}