using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillTreeManager : MonoBehaviour
{
    [Header("UI i Prefaby")]
    public TMP_Text skillPointsText;
    public GameObject linePrefab;
    public RectTransform linesContainer; 
    public RectTransform treeContainer; 

    [Header("Kolory Linii")]
    public Color lockedLineColor = new Color(0.3f, 0.3f, 0.3f, 1f); 
    public Color availableLineColor = Color.white;                   
    public Color unlockedLineColor = Color.green;                    

    private MonsterSaveData currentMonster;
    private List<SkillNodeUI> allNodes = new List<SkillNodeUI>();
    private GameObject currentTreeInstance;
    
    private class NodeConnection
    {
        public SkillNodeUI fromNode;
        public SkillNodeUI toNode;
        public Image lineImage;
    }
    private List<NodeConnection> connections = new List<NodeConnection>();

    public void OtworzDrzewko(MonsterSaveData monster, GameObject treePrefab)
    {
        currentMonster = monster;
        gameObject.SetActive(true);

        if (currentTreeInstance != null)
        {
            Destroy(currentTreeInstance);
            foreach (Transform child in linesContainer) Destroy(child.gameObject);
            allNodes.Clear();
            connections.Clear();
        }

        if (treePrefab != null && treeContainer != null)
        {
            currentTreeInstance = Instantiate(treePrefab, treeContainer);
            allNodes = new List<SkillNodeUI>(currentTreeInstance.GetComponentsInChildren<SkillNodeUI>(true));
        }

        RysujWszystkieLinie();
        OdswiezInterfejs();
    }

    private void RysujWszystkieLinie()
    {
        connections.Clear();
        foreach (var node in allNodes)
        {
            foreach (var childNode in node.unlocksNodes)
            {
                if (childNode != null)
                {
                    Image lineImg = RysujLinie(node.GetComponent<RectTransform>(), childNode.GetComponent<RectTransform>());
                    
                    connections.Add(new NodeConnection { 
                        fromNode = node, 
                        toNode = childNode, 
                        lineImage = lineImg 
                    });
                }
            }
        }
    }

    private Image RysujLinie(RectTransform fromNode, RectTransform toNode)
    {
        GameObject lineObj = Instantiate(linePrefab, linesContainer);
        RectTransform rect = lineObj.GetComponent<RectTransform>();
        Image img = lineObj.GetComponent<Image>(); 
        
        Vector2 posA = linesContainer.InverseTransformPoint(fromNode.position);
        Vector2 posB = linesContainer.InverseTransformPoint(toNode.position);

        Vector2 dir = posB - posA;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float distance = dir.magnitude;

        rect.anchoredPosition = posA;
        rect.sizeDelta = new Vector2(distance, 8f); 
        rect.localRotation = Quaternion.Euler(0, 0, angle);

        return img;
    }

    public void OdswiezInterfejs()
    {
        if (currentMonster == null) return;
        if (skillPointsText != null) skillPointsText.text = $"SP: {currentMonster.skillPoints}";

        Dictionary<SkillNodeUI, int> nodeStates = new Dictionary<SkillNodeUI, int>();

        // 1. Aktualizacja Kulek
        foreach (var node in allNodes)
        {
            int currentLevel = currentMonster.unlockedSkills.FindAll(id => id == node.nodeID).Count;
            int state = 0; 

            if (currentLevel >= node.maxLevels)
            {
                state = 2; // Wymaksowane
            }
            else
            {
                if (node.isStartingNode) 
                {
                    state = 1; // Dostępne
                }
                else
                {
                    // Szukamy, czy jakikolwiek rodzic jest w pełni WYMAKSOWANY
                    foreach (var parentNode in allNodes)
                    {
                        if (parentNode.unlocksNodes.Contains(node))
                        {
                            int parentLevel = currentMonster.unlockedSkills.FindAll(id => id == parentNode.nodeID).Count;
                            
                            // NOWE ZASADY: Odblokuj jeśli rodzic ma maxLevels
                            if (parentLevel >= parentNode.maxLevels)
                            {
                                state = 1;
                                break;
                            }
                        }
                    }
                }
            }

            if (state == 1 && currentMonster.skillPoints < node.cost)
            {
                node.button.interactable = false;
                if (node.costText != null) node.costText.color = Color.red;
            }

            nodeStates[node] = state;
            node.Setup(this, state, currentLevel);
        }

        // 2. Aktualizacja Linii
        foreach (var conn in connections)
        {
            int parentLevel = currentMonster.unlockedSkills.FindAll(id => id == conn.fromNode.nodeID).Count;
            int targetState = nodeStates.ContainsKey(conn.toNode) ? nodeStates[conn.toNode] : 0;

            // Linia jest szara, jeśli cel zablokowany (bo rodzic nie jest na maksa)
            if (parentLevel < conn.fromNode.maxLevels)
            {
                conn.lineImage.color = lockedLineColor; 
            }
            else
            {
                if (targetState == 2) conn.lineImage.color = unlockedLineColor; // Zielona
                else conn.lineImage.color = availableLineColor; // Biała
            }
        }
    }

    public void SprobujKupicUmiejetnosc(SkillNodeUI node)
    {
        if (currentMonster == null) return;

        int currentLevel = currentMonster.unlockedSkills.FindAll(id => id == node.nodeID).Count;

        if (currentLevel >= node.maxLevels)
        {
            Debug.Log($"<color=orange>[SKILL TREE]</color> Maksymalny poziom umiejętności osiągnięty!");
            return;
        }

        if (currentMonster.skillPoints >= node.cost)
        {
            currentMonster.skillPoints -= node.cost;
            currentMonster.unlockedSkills.Add(node.nodeID); 

            DodajStatystykeDoZapisu(node.statToBoost, node.boostAmount);

            Debug.Log($"<color=cyan>[SKILL TREE]</color> Ulepszono: {node.displayName} na poziom {currentLevel + 1}/{node.maxLevels}");
            OdswiezInterfejs();
        }
    }

    public void ZamknijDrzewko()
    {
        gameObject.SetActive(false);
    }

    private void DodajStatystykeDoZapisu(string stat, int amount)
    {
        if (string.IsNullOrEmpty(stat)) return;
        string upper = stat.ToUpper();

        if (upper.Contains("ATACK") || upper.Contains("ATAK")) currentMonster.atack += amount;
        else if (upper.Contains("MAXHEALTH") || upper.Contains("HEALTH")) 
        {
            currentMonster.maxHealth += amount;
            currentMonster.health += amount; 
        }
        else if (upper.Contains("ARMOR")) currentMonster.armor += amount;
        else if (upper.Contains("LIFESTEAL")) currentMonster.lifeSteal += amount;
        else if (upper.Contains("REGEN")) currentMonster.regen += amount;
        else if (upper.Contains("DOGE")) currentMonster.doge += amount;
        else if (upper.Contains("FIRE")) currentMonster.fire += amount;
        else if (upper.Contains("WATER")) currentMonster.water += amount;
        else if (upper.Contains("EARTH")) currentMonster.earth += amount;
        else if (upper.Contains("AIR")) currentMonster.air += amount;
        else if (upper.Contains("MAGIC")) currentMonster.magic += amount;
        else if (upper.Contains("CRITE")) currentMonster.crite += amount;
        else if (upper.Contains("ENERGYMAX")) currentMonster.energyMax += amount;
        else if (upper.Contains("ENERGYSTART")) currentMonster.energyStart += amount;
        else if (upper.Contains("ENERGYPERSECOND")) currentMonster.energyPerSecond += amount;
        else
        {
            Debug.LogWarning($"<color=orange>[SKILL TREE]</color> Nie rozpoznano statystyki: {stat}");
        }
    }
}