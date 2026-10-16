using UnityEngine;

public enum TypStatusu { ZwiekszenieStat, Trucizna, Podpalenie, Oslabienie, Szczescie, Pech }
public enum TypCzasu { Sekundy, Walki, DoUleczenia }

[System.Serializable]
public class StatusData
{
    public TypStatusu typ;
    public TypCzasu typCzasu;
    public float wartoscCzasu; 
    public string rownaniePoziomu;
    public int poziom;
    public string statystyka; 
    public Sprite ikonaZrodlowa; 
}

[System.Serializable]
public class AktywnyStatus
{
    public TypStatusu typ;
    public TypCzasu typCzasu;
    public int poziom;
    public float pozostalyCzasSekundy;
    public float maxCzasSekundy; 
    public int pozostaleWalki;
    public string statystyka;
    public float tickTimer; 

    public StatusIconUI skryptUI;
    public GameObject obiektUI; 

    public AktywnyStatus(StatusData dane)
    {
        typ = dane.typ;
        typCzasu = dane.typCzasu;
        poziom = dane.poziom;
        pozostalyCzasSekundy = dane.wartoscCzasu;
        maxCzasSekundy = dane.wartoscCzasu;
        pozostaleWalki = (int)dane.wartoscCzasu;
        statystyka = dane.statystyka;
        tickTimer = 0f;
    }
}