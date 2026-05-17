// v2 — ApplyVisuals ile label + sprite güncelleme
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Haritadaki node'ları StartPoint'ten BFS derinliğine göre tier'lara ayırır
/// ve her seferde rastgele bir event atar.
/// 
/// Tier Sistemi:
///   Tier 1 (derinlik 1-4)  : Güvenli başlangıç bölgesi
///   Tier 2 (derinlik 5-9)  : Orta risk, kervan çıkabilir
///   Tier 3 (derinlik 10+)  : Tehlikeli, yüksek ödüllü
///   Boss   (en derin node) : Her zaman Boss kalır, değişmez
/// </summary>
public class MapRandomizer : MonoBehaviour
{
    public static MapRandomizer Instance;

    // ── Tier Havuzları ──────────────────────────────────────────────────────
    [Header("Tier 1 — Güvenli Bölge (Derinlik 1-4)")]
    public List<NodeType> tier1Pool = new List<NodeType>
    {
        NodeType.Village,
        NodeType.RestArea,
        NodeType.NasibEncounter,
        NodeType.DervishEncounter,
        NodeType.Tuccar,
        NodeType.Dice,
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
        NodeType.Kacak,
    };

    [Header("Tier 3 — Tehlikeli Bölge (Derinlik 10+)")]
    public List<NodeType> tier3Pool = new List<NodeType>
    {
        NodeType.KuleSavas,
        NodeType.Treasure,
        NodeType.Kalkan,
        NodeType.Vahsi,
        NodeType.Archery,
        NodeType.KervanEncounter,  // Çantası dolu oyuncu için çok değerli
        NodeType.Zindan,
    };

    // Sabit derinlik sınırları (Inspector'dan ayarlanabilir)
    [Header("Derinlik Sınırları")]
    [Tooltip("Bu derinliğe kadar Tier 1 havuzundan seçilir")]
    public int tier1MaxDepth = 4;
    [Tooltip("Bu derinliğe kadar Tier 2 havuzundan seçilir")]
    public int tier2MaxDepth = 9;
    // tier2MaxDepth üzeri otomatik Tier 3 olur

    // ────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Haritayı baştan sona BFS ile dolaşır, her node'a derinliğine
    /// uygun rastgele bir event atar. Boss ve StartPoint değişmez.
    /// </summary>
    public void RandomizeMap()
    {
        // 1. StartPoint'i bul
        MapNode startNode = FindStartNode();
        if (startNode == null)
        {
            Debug.LogError("[MapRandomizer] StartPoint bulunamadı! " +
                           "Sahnede NodeType.StartPoint türünde bir node olduğundan emin ol.");
            return;
        }

        // 2. BFS ile her node'un derinliğini hesapla
        Dictionary<MapNode, int> depthMap = BuildDepthMap(startNode);

        if (depthMap.Count == 0)
        {
            Debug.LogError("[MapRandomizer] BFS sonuç döndürmedi. " +
                           "outgoingPaths bağlantılarını kontrol et.");
            return;
        }

        int maxDepth = depthMap.Values.Max();
        Debug.Log($"[MapRandomizer] Toplam {depthMap.Count} node bulundu. " +
                  $"Maks derinlik: {maxDepth}");

        // 3. BFS'e giremeyen (hiçbir node'a bağlı olmayan) orphan node'ları da topla
        MapNode[] allNodes = FindObjectsByType<MapNode>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var n in allNodes)
        {
            if (!depthMap.ContainsKey(n))
            {
                // Bağlantısız node: Tier 1'den rastgele ver, derinlik 1 say
                depthMap[n] = 1;
                Debug.LogWarning($"[MapRandomizer] '{n.gameObject.name}' " +
                                 "hiçbir node'a bağlı değil (orphan). Tier 1 atandı.");
            }
        }

        // 4. Her node'a tier'ına uygun event + görsel ata
        foreach (var kvp in depthMap)
        {
            MapNode node = kvp.Key;
            int depth    = kvp.Value;

            // Sabit node'lara sprite/label güncelle ama type'a dokunma
            if (node.nodeType == NodeType.StartPoint ||
                node.nodeType == NodeType.Boss)
            {
                node.ApplyVisuals(GetSpriteForType(node.nodeType), GetLabel(node.nodeType));
                continue;
            }

            // Manuel Boss yoksa en derin node'ları Boss yap
            if (depth == maxDepth && !HasManualBossNode(depthMap))
            {
                node.nodeType = NodeType.Boss;
                node.ApplyVisuals(GetSpriteForType(NodeType.Boss), GetLabel(NodeType.Boss));
                continue;
            }

            // Tier seç → görsel uygula
            NodeType chosen = PickFromTier(depth);
            node.nodeType   = chosen;
            node.ApplyVisuals(GetSpriteForType(chosen), GetLabel(chosen));
        }

        Debug.Log("[MapRandomizer] Harita başarıyla rastgeleleştirildi!");
    }

    // ── Yardımcı: BFS Derinlik Haritası ────────────────────────────────────
    private Dictionary<MapNode, int> BuildDepthMap(MapNode root)
    {
        var depthMap = new Dictionary<MapNode, int>();
        var queue    = new Queue<MapNode>();

        depthMap[root] = 0;
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            MapNode current = queue.Dequeue();
            int currentDepth = depthMap[current];

            foreach (MapNode neighbor in current.outgoingPaths)
            {
                if (neighbor == null) continue;
                if (depthMap.ContainsKey(neighbor)) continue; // Zaten ziyaret edildi

                depthMap[neighbor] = currentDepth + 1;
                queue.Enqueue(neighbor);
            }
        }

        return depthMap;
    }

    // ── Yardımcı: Derinliğe göre havuzdan seç ──────────────────────────────
    private NodeType PickFromTier(int depth)
    {
        List<NodeType> pool;

        if (depth <= tier1MaxDepth)
            pool = tier1Pool;
        else if (depth <= tier2MaxDepth)
            pool = tier2Pool;
        else
            pool = tier3Pool;

        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning($"[MapRandomizer] Derinlik {depth} için havuz boş! Village atandı.");
            return NodeType.Village;
        }

        return pool[Random.Range(0, pool.Count)];
    }

    // ── Yardımcı: StartPoint bul ────────────────────────────────────────────
    private MapNode FindStartNode()
    {
        // Önce Inspector'daki MapManager.startingNodes'a bak (en güvenilir)
        if (MapManager.Instance != null &&
            MapManager.Instance.startingNodes != null &&
            MapManager.Instance.startingNodes.Count > 0)
        {
            // startingNodes listesinde StartPoint türünde olan varsa onu al
            foreach (var n in MapManager.Instance.startingNodes)
            {
                if (n != null && n.nodeType == NodeType.StartPoint)
                    return n;
            }
            // Yoksa listenin ilk elemanını kullan
            if (MapManager.Instance.startingNodes[0] != null)
                return MapManager.Instance.startingNodes[0];
        }

        // Fallback: sahnede ara
        MapNode[] allNodes = FindObjectsByType<MapNode>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var node in allNodes)
            if (node.nodeType == NodeType.StartPoint)
                return node;

        return null;
    }

    // ── Yardımcı: Sahneye elle konulmuş Boss var mı? ───────────────────────
    private bool HasManualBossNode(Dictionary<MapNode, int> depthMap)
    {
        foreach (var node in depthMap.Keys)
            if (node.nodeType == NodeType.Boss) return true;
        return false;
    }

    // ── Label metinleri ─────────────────────────────────────────────────────
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
        NodeType.RestArea          => "Dinlenme",
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
        NodeType.Kacak             => "Kaçak",
        _                          => "?"
    };

    // ── Sprite seçici ───────────────────────────────────────────────────────
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
            NodeType.RestArea          => em.villageSprite,
            NodeType.NasibEncounter    => em.villageSprite,
            NodeType.DervishEncounter  => em.villageSprite,
            NodeType.CaravanEncounter  => em.merchant,
            NodeType.KervanEncounter   => em.merchant,
            NodeType.Kacak             => em.villageSprite,
            _                          => em.villageSprite,
        };
    }
}