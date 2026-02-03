using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject nodePrefab;  // Haritadaki yuvarlak buton prefabı
    public Transform mapContainer; // UI'da nerenin içine dizilecek?
    public int totalLayers = 10;   // Kaç durak var?
    
    // Basitlik için katmanların Y koordinatları ve X aralıkları
    public float layerDistanceX = 200f; // Yatay mesafe
    public float nodeDistanceY = 150f;  // Dikey mesafe
void Start()
    {
        // Oyun başladığında haritayı üret!
        GenerateMap(); 
    }
    public void GenerateMap()
    {
        // Önce temizle
        foreach (Transform child in mapContainer) Destroy(child.gameObject);

        List<MapNode> previousLayerNodes = new List<MapNode>();

        // --- KATMANLARI OLUŞTUR ---
        for (int i = 0; i < totalLayers; i++)
        {
            List<MapNode> currentLayerNodes = new List<MapNode>();
            
            // Bu katmanda kaç yol ayrımı olsun? (Rastgele 2 veya 3, ilk ve son katman hariç)
            int nodeCount = (i == 0 || i == totalLayers - 1) ? 1 : Random.Range(2, 4);

            for (int j = 0; j < nodeCount; j++)
            {
                // 1. Düğümü Yarat
                GameObject newNodeObj = Instantiate(nodePrefab, mapContainer);
                
                // 2. Konumunu Ayarla (Basit matematik)
                // X: İlerlemeyi temsil eder, Y: Alternatif yolları
                float xPos = i * layerDistanceX; 
                // Ortalamak için matematik:
                float yPos = (j - (nodeCount - 1) / 2f) * nodeDistanceY; 
                
                newNodeObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);

                // 3. Verisini Ayarla
                MapNode nodeScript = newNodeObj.GetComponent<MapNode>();
                NodeType type = DetermineNodeType(i); // Rastgele tip seç
                nodeScript.Setup(type, i);
                
                currentLayerNodes.Add(nodeScript);

                // 4. Bağlantıları Kur (Bir önceki katmandakilerle)
                if (i > 0)
                {
                    ConnectNodes(previousLayerNodes, nodeScript);
                }
            }

            previousLayerNodes = currentLayerNodes;
        }
    }

    // Katmana göre rastgele tip belirle
    NodeType DetermineNodeType(int layer)
    {
        if (layer == 0) return NodeType.StartPoint;
        if (layer == totalLayers - 1) return NodeType.Boss;
        
        // %40 Savaş, %30 Köy, %20 Olay, %10 Elit
        float chance = Random.value;
        if (chance < 0.4f) return NodeType.Battle;
        else if (chance < 0.7f) return NodeType.Village;
        else return NodeType.RestArea;
    }

    // Basit bağlantı mantığı: Önceki katmandaki her düğüm, 
    // şimdiki katmandaki en yakın düğüme bağlansın.
    void ConnectNodes(List<MapNode> prevLayer, MapNode currentNode)
    {
        // Burada biraz karmaşık bir algoritma gerekir ama şimdilik
        // Basitçe: Önceki katmandaki rastgele birine bağla
        var parent = prevLayer[Random.Range(0, prevLayer.Count)];
        parent.accessibleNodes.Add(currentNode);
        
        // ÇİZGİ ÇİZME İŞLEMİ (Line Renderer veya UI Line ile yapılacak)
        // DrawLine(parent, currentNode);
    }
}