using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public int adventureWejscia;
    public int maxWejscia;

    public List<MonsterSaveData> playerMonsters = new List<MonsterSaveData>();
    
    // ZAPISUJEMY TYLKO POZYCJĘ POTWORA, żeby zapobiec klonowaniu przez JSON!
    public List<int> activePartyIndices = new List<int>(); 

    public List<string> playerItemNames = new List<string>();
}