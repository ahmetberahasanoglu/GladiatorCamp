using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapRandomizer : MonoBehaviour
{
    public static MapRandomizer Instance;

    [Header("Tier Havuzları (Event Dağılımı)")]
    public List<NodeType> tier1Pool = new List<NodeType> { NodeType.Village, NodeType.RestArea, NodeType.NasibEncounter, NodeType.DervishEncounter, NodeType.FirstBattle };
    public List<NodeType> tier2Pool = new List<NodeType> { NodeType.Battle, NodeType.Tuccar, NodeType.Atyarisi, NodeType.YagliGures, NodeType.Dice, NodeType.CaravanEncounter };
    public List<NodeType> tier3Pool = new List<NodeType> { NodeType.Zindan, NodeType.KuleSavas, NodeType.Treasure, NodeType.Kalkan, NodeType.Vahsi, NodeType.Archery };

    void Awake()
    {
        Instance = this;
    }

    public void RandomizeMap()
    {
        // 1. Tüm Node'ları bul
        var allNodes = FindObjectsByType<MapNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (allNodes.Length == 0) return;

        // 2. Haritanın genişliğini hesapla (X koordinatına göre)
        float minX = allNodes.Min(n => n.GetComponent<RectTransform>().anchoredPosition.x);
        float maxX = allNodes.Max(n => n.GetComponent<RectTransform>().anchoredPosition.x);
        float tierWidth = (maxX - minX) / 3f; // Haritayı 3 dikey dilime bölüyoruz

        // 3. Node'ları Tier'lara göre rastgeleleştir
        foreach (var node in allNodes)
        {
            // Başlangıç ve Boss noktalarının yeri ve türü sabittir, onlara dokunma!
            if (node.nodeType == NodeType.StartPoint || node.nodeType == NodeType.Boss) continue;

            float xPos = node.GetComponent<RectTransform>().anchoredPosition.x;
            NodeType newType;

            // X pozisyonuna göre hangi Tier'da olduğunu bul ve o havuzdan rastgele bir event seç
            if (xPos < minX + tierWidth) 
                newType = tier1Pool[Random.Range(0, tier1Pool.Count)];
            else if (xPos < minX + (tierWidth * 2)) 
                newType = tier2Pool[Random.Range(0, tier2Pool.Count)];
            else 
                newType = tier3Pool[Random.Range(0, tier3Pool.Count)];

            // Node'u güncelle
            node.nodeType = newType;
            node.iconImage.sprite = GetSpriteForEvent(newType);
        }

        Debug.Log("Harita Rastgeleleştirildi! Yeni bir sefer (Run) başlıyor.");
    }

    // MapEventManager'daki görselleri otomatik çeker
    private Sprite GetSpriteForEvent(NodeType type)
    {
        MapEventManager em = MapEventManager.Instance;
        if (em == null) return null;

        switch (type)
        {
            case NodeType.Village: return em.villageSprite;
            case NodeType.Battle: return em.battleSprite;
            case NodeType.FirstBattle: return em.battleSprite;
            case NodeType.Treasure: return em.treasureSprite;
            case NodeType.Boss: return em.bossSprite;
            case NodeType.Archery: return em.archerySprite;
            case NodeType.Atyarisi: return em.horseSprite;
            case NodeType.KuleSavas: return em.towerSprite;
            case NodeType.Tuccar: return em.tuccarSprite;
            case NodeType.Kalkan: return em.shieldSprite;
            case NodeType.Zindan: return em.dungeonSprite;
            case NodeType.Vahsi: return em.wildSprite;
            case NodeType.Dice: return em.diceSprite;
            case NodeType.YagliGures: return em.wrestlingSprite;
            case NodeType.RestArea: return em.villageSprite; // Dinlenme için özel sprite ekleyebilirsin
            case NodeType.NasibEncounter: return em.villageSprite; // Derviş için özel sprite ekleyebilirsin
            case NodeType.DervishEncounter: return em.villageSprite; 
            case NodeType.CaravanEncounter: return em.merchant;
            default: return em.villageSprite; 
        }
    }
}