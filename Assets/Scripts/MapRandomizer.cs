using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Haritayı BFS ile tarar ve "Nefes Ritmi" (Pacing) + "Tekrar Yok" (No Repeat)
/// algoritmalarını kullanarak rastgele etkinlikler atar.
/// </summary>
public class MapRandomizer : MonoBehaviour
{
    public static MapRandomizer Instance;

    // ── Tier Havuzları ──────────────────────────────────────────────────────
    [Header("Tier 1 — Güvenli Bölge (Derinlik 1-4)")]
    public List<NodeType> tier1Pool = new List<NodeType>
    {
        NodeType.Village,
        NodeType.NasibEncounter,
        NodeType.DervishEncounter,
        NodeType.Tuccar,
        NodeType.Dice,
        NodeType.FirstBattle,
        NodeType.Vahsi
        // RestArea tamamen kaldırıldı. Köy aynı işlevi görüyor.
    };

   [Header("Tier 2 — Orta Risk (Derinlik 5-9)")]
    public List<NodeType> tier2Pool = new List<NodeType>
    {
        NodeType.Battle,
        NodeType.Atyarisi,
        NodeType.YagliGures,
        NodeType.CaravanEncounter,
        NodeType.KervanEncounter,
        NodeType.Zindan,
        NodeType.CenkOyunu // Kacak Çıkarıldı
    };

    [Header("Tier 3 — Tehlikeli Bölge (Derinlik 10+)")]
    public List<NodeType> tier3Pool = new List<NodeType>
    {
        NodeType.KuleSavas,
        NodeType.Treasure,
        NodeType.Kalkan,
        NodeType.Vahsi,
        NodeType.Archery,
        NodeType.KervanEncounter,  
        NodeType.Zindan,
     NodeType.NightRaid,
    };
    [Header("Map Config Referansı")]
    public MapConfig mapConfig;

    [Header("Derinlik Sınırları")]
    public int tier1MaxDepth = 4;
    public int tier2MaxDepth = 9;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RandomizeMap()
    {
        MapNode startNode = FindStartNode();
        if (startNode == null)
        {
            Debug.LogError("[MapRandomizer] StartPoint bulunamadı!");
            return;
        }

        // 1. BFS ile derinlikleri ve ebeveyn (önceki) node'ları bul
        Dictionary<MapNode, int> depthMap = BuildDepthMap(startNode, out Dictionary<MapNode, List<MapNode>> incomingPaths);
        
        if (depthMap.Count == 0) return;

        int maxDepth = depthMap.Values.Max();

        // 2. Orphan (bağlantısız) node'ları topla
        MapNode[] allNodes = FindObjectsByType<MapNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var n in allNodes)
        {
            if (!depthMap.ContainsKey(n))
            {
                depthMap[n] = 1;
                incomingPaths[n] = new List<MapNode>();
            }
        }

        // 3. Atamaları Derinliğe (Tier) Göre Sırayla Yap
        var sortedNodes = depthMap.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
        
        // Savaş Pacing'ini takip etmek için (Üst üste kaç savaş oldu?)
        Dictionary<MapNode, int> consecutiveBattles = new Dictionary<MapNode, int>();
        consecutiveBattles[startNode] = 0;

        foreach (var node in sortedNodes)
        {
            int maxParentBattles = 0;
            List<NodeType> parentTypes = new List<NodeType>();

            // Bu node'a gelen önceki node'ların (ebeveynlerin) tiplerini ve savaş sayılarını al
            if (incomingPaths.ContainsKey(node))
            {
                foreach (var parent in incomingPaths[node])
                {
                    if (consecutiveBattles.ContainsKey(parent))
                    {
                        maxParentBattles = Mathf.Max(maxParentBattles, consecutiveBattles[parent]);
                    }
                    parentTypes.Add(parent.nodeType);
                }
            }

            // Sabit Node'lar (Başlangıç ve Boss değişmez)
           if (node.nodeType == NodeType.StartPoint || node.nodeType == NodeType.Boss)
            {
                consecutiveBattles[node] = IsCombatEvent(node.nodeType) ? maxParentBattles + 1 : 0;
                // YENİ: GetSpriteForType yerine mapConfig.GetIcon kullanıyoruz
                node.ApplyVisuals(mapConfig != null ? mapConfig.GetIcon(node.nodeType) : null, GetLabel(node.nodeType));
                continue;
            }

            int depth = depthMap[node];
            
            // Son derinlikteki manuel Boss yoksa Boss ata
            if (depth == maxDepth && !HasManualBossNode(depthMap))
            {
                node.nodeType = NodeType.Boss;
                consecutiveBattles[node] = maxParentBattles + 1;
                node.ApplyVisuals(GetSpriteForType(NodeType.Boss), GetLabel(NodeType.Boss));
                continue;
            }

            // --- AKILLI SEÇİM ALGORİTMASI ---
            bool forcePassive = maxParentBattles >= 2; // Arka arkaya 2 savaş olduysa mola ver!
            NodeType chosen = PickSmartFromTier(depth, parentTypes, forcePassive);
            
            node.nodeType = chosen;
            consecutiveBattles[node] = IsCombatEvent(chosen) ? maxParentBattles + 1 : 0;
            // YENİ: GetSpriteForType yerine mapConfig.GetIcon kullanıyoruz
            node.ApplyVisuals(mapConfig != null ? mapConfig.GetIcon(chosen) : null, GetLabel(chosen));
        }

        Debug.Log("[MapRandomizer] Harita başarıyla akıllı ritimle rastgeleleştirildi!");
    }

    // ── AKILLI SEÇİCİ ──────────────────────────────────────────────────────
    private NodeType PickSmartFromTier(int depth, List<NodeType> forbiddenTypes, bool forcePassive)
    {
        List<NodeType> pool;

        if (depth <= tier1MaxDepth) pool = new List<NodeType>(tier1Pool);
        else if (depth <= tier2MaxDepth) pool = new List<NodeType>(tier2Pool);
        else pool = new List<NodeType>(tier3Pool);

        // 1. KURAL: NEFES RİTMİ (Eğer zorunlu mola lazımsa savaş eventlerini havuzdan çıkar)
        if (forcePassive)
        {
            var passivePool = pool.Where(t => !IsCombatEvent(t)).ToList();
            if (passivePool.Count > 0) pool = passivePool;
        }

        // 2. KURAL: TEKRAR YOK (Önceki node ile aynı event çıkmasın)
        var noRepeatPool = pool.Where(t => !forbiddenTypes.Contains(t)).ToList();
        if (noRepeatPool.Count > 0)
        {
            pool = noRepeatPool;
        }

        // Eğer havuz bir şekilde boşalırsa (çok düşük ihtimal), her zaman güvenli liman Köy ver.
        if (pool.Count == 0) return NodeType.Village;

        return pool[Random.Range(0, pool.Count)];
    }

    // ── KATEGORİ BELİRLEYİCİLER ────────────────────────────────────────────
    private bool IsCombatEvent(NodeType type)
    {
        return type == NodeType.Battle || 
               type == NodeType.FirstBattle || 
               type == NodeType.KuleSavas || 
               type == NodeType.Vahsi || 
               type == NodeType.Zindan || 
               type == NodeType.Boss;
    }

    // ── BFS Derinlik Haritası ve Geliş Yönleri ──────────────────────────────
    private Dictionary<MapNode, int> BuildDepthMap(MapNode root, out Dictionary<MapNode, List<MapNode>> incomingPaths)
    {
        var depthMap = new Dictionary<MapNode, int>();
        incomingPaths = new Dictionary<MapNode, List<MapNode>>();
        var queue = new Queue<MapNode>();

        depthMap[root] = 0;
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            MapNode current = queue.Dequeue();
            int currentDepth = depthMap[current];

            foreach (MapNode neighbor in current.outgoingPaths)
            {
                if (neighbor == null) continue;

                // Bu neighbor'a nereden gelindiğini kaydet (Ebeveyn kontrolü için)
                if (!incomingPaths.ContainsKey(neighbor))
                    incomingPaths[neighbor] = new List<MapNode>();
                
                if (!incomingPaths[neighbor].Contains(current))
                    incomingPaths[neighbor].Add(current);

                if (depthMap.ContainsKey(neighbor)) continue;

                depthMap[neighbor] = currentDepth + 1;
                queue.Enqueue(neighbor);
            }
        }
        return depthMap;
    }

    // ── YARDIMCI METOTLAR ───────────────────────────────────────────────────
    private MapNode FindStartNode()
    {
        if (MapManager.Instance != null && MapManager.Instance.startingNodes != null && MapManager.Instance.startingNodes.Count > 0)
        {
            foreach (var n in MapManager.Instance.startingNodes)
                if (n != null && n.nodeType == NodeType.StartPoint) return n;
            if (MapManager.Instance.startingNodes[0] != null) return MapManager.Instance.startingNodes[0];
        }

        MapNode[] allNodes = FindObjectsByType<MapNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var node in allNodes)
            if (node.nodeType == NodeType.StartPoint) return node;

        return null;
    }

    private bool HasManualBossNode(Dictionary<MapNode, int> depthMap)
    {
        foreach (var node in depthMap.Keys)
            if (node.nodeType == NodeType.Boss) return true;
        return false;
    }

    private string GetLabel(NodeType type) => type switch
    {
        NodeType.Battle            => "Savaş",
        NodeType.FirstBattle       => "Savaş",
        NodeType.Village           => "Köy",
        NodeType.KervanEncounter   => "Kervan",
        NodeType.CaravanEncounter  => "Kervan",
        NodeType.Zindan            => "Zindan",
        NodeType.Boss              => "Kızıl Kale",
        NodeType.Tuccar            => "Tüccar",
        NodeType.Treasure          => "Hazine",
        NodeType.Vahsi             => "Vahşi Hayvan",
        NodeType.KuleSavas         => "Kule Savaşı",
        NodeType.Archery           => "Ok Yarışması",
        NodeType.Atyarisi          => "At Yarışı",
        NodeType.YagliGures        => "Güreş",
        NodeType.Dice              => "Zar",
        NodeType.NasibEncounter    => "Nasip",
        NodeType.DervishEncounter  => "Derviş",
        NodeType.Kalkan            => "Savunma",
        NodeType.StartPoint        => "Kamp",
        NodeType.CenkOyunu         => "Cenk",
         NodeType.NightRaid        => "Gece Baskını",
        _                          => "?"
    };

    private Sprite GetSpriteForType(NodeType type)
    {
        MapEventManager em = MapEventManager.Instance;
        if (em == null) return null;

        return type switch
        {
            NodeType.Village           => em.villageSprite,
            NodeType.Battle            => em.battleSprite,
            NodeType.FirstBattle       => em.battleSprite,
            NodeType.Treasure          => em.treasureSprite,
            NodeType.Boss              => em.bossSprite,
            NodeType.Archery           => em.archerySprite,
            NodeType.Atyarisi          => em.horseSprite,
            NodeType.KuleSavas         => em.towerSprite,
            NodeType.Tuccar            => em.tuccarSprite,
            NodeType.Kalkan            => em.shieldSprite,
            NodeType.Zindan            => em.dungeonSprite,
            NodeType.Vahsi             => em.wildSprite,
            NodeType.Dice              => em.diceSprite,
            NodeType.YagliGures        => em.wrestlingSprite,
            NodeType.NasibEncounter    => em.nasibSprite,
            NodeType.DervishEncounter  => em.dervishSprite,
            NodeType.CaravanEncounter  => em.caravanSprite,
            NodeType.KervanEncounter   => em.kervanSprite,
            NodeType.CenkOyunu         => em.cenkSprite, 
            NodeType.AsikAtismasi         => em.asikSprite, 
            NodeType.NightRaid        => em.battleSprite,
            _                          => em.villageSprite,
        };
    }
}