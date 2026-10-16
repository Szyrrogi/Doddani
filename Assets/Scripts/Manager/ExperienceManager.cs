using UnityEngine;

public static class ExperienceManager
{
    public static int WymaganyExp(int obecnyPoziom)
    {
        switch (obecnyPoziom)
        {
            case 1: return 30;
            case 2: return 55;
            case 3: return 115;
            case 4: return 170;
            case 5: return 255;
            case 6: return 380;
            case 7: return 570;
            case 8: return 855;
            case 9: return 1280;
            case 10: return 1920;
            
            case 11: return 2880;
            case 12: return 4320;
            case 13: return 6480;
            case 14: return 9720;
            case 15: return 14580;
            case 16: return 21870;
            case 17: return 32800;
            case 18: return 49200;
            case 19: return 73800;
            case 20: return 110700;
            
            case 21: return 166000;
            case 22: return 249000;
            case 23: return 373500;
            case 24: return 560000;
            case 25: return 840000;
            case 26: return 1260000;
            case 27: return 1890000;
            case 28: return 2835000;
            case 29: return 4250000;
            case 30: return 6375000;
            
            case 31: return 9560000;
            case 32: return 14340000;
            case 33: return 21500000;
            case 34: return 32250000;
            case 35: return 48375000;
            case 36: return 72500000;
            case 37: return 108750000;
            case 38: return 163000000;
            case 39: return 244500000;
            case 40: return 366750000;

            default:
                // Zabezpieczenie: jeśli potwór jakimś cudem osiągnie wyższy poziom niż 40,
                // na każdy kolejny będzie potrzebował astronomicznej wartości z 40 poziomu.
                if (obecnyPoziom > 40) return 366750000;
                
                // Awaryjne zero dla błędnych poziomów (np. 0 lub ujemnych)
                return 50; 
        }
    }
}