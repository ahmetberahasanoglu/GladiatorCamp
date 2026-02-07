using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("UI")]
    public GameObject mapPanel;
    public RectTransform playerIcon;

    [Header("Ayarlar")]
    // Oyunun en başında gidilebilecek ilk noktalar (Sen elle seçeceksin)
    public List<MapNode> startingNodes; 

    public MapNode currentNode; // Şu an neredeyiz?

    // MapManager.cs içine ekle:
[Header("Çizgi Ayarları")]
public GameObject linePrefab; // İnce uzun bir Image (UI Line)
public Transform lineContainer; // Çizgilerin toplanacağı yer (MapContainer ile aynı olsun)

void Start()
{
    DrawAllConnections();
}

void DrawAllConnections()
{
    // Tüm MapNode'ları bul
    var allNodes = FindObjectsOfType<MapNode>();

    foreach (var node in allNodes)
    {
        foreach (var target in node.outgoingPaths)
        {
            if (target != null)
            {
                CreateVisualLine(node.GetComponent<RectTransform>(), target.GetComponent<RectTransform>());
            }
        }
    }
}

// MapManager.cs içindeki CreateVisualLine fonksiyonunu güncelle:

void CreateVisualLine(RectTransform start, RectTransform end)
{
    GameObject lineObj = Instantiate(linePrefab, lineContainer);
    lineObj.transform.SetAsFirstSibling(); 

    RectTransform rect = lineObj.GetComponent<RectTransform>();
    
    // İki nokta arası mesafe ve orta nokta
    Vector3 diff = end.localPosition - start.localPosition;
    Vector3 midpoint = (start.localPosition + end.localPosition) / 2;
    
    rect.localPosition = midpoint;
    
    // Çizgi uzunluğunu ayarla
    // Yükseklik (5) çizginin kalınlığıdır. DashLine görselin çok büyükse bunu artır/azalt.
    rect.sizeDelta = new Vector2(diff.magnitude, 5); 
    
    // Dönme açısını hesapla
    float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
    rect.rotation = Quaternion.Euler(0, 0, angle);

    // --- YENİ EKLENEN KISIM: Image Tiled Ayarı ---
    Image img = lineObj.GetComponent<Image>();
    if (img != null)
    {
        img.type = Image.Type.Tiled; // Kodla da garantiye alalım
        
        // Bu değerle oynayarak kesiklerin sıklığını değiştirebilirsin
        // 1 = Normal, 2 = Daha sık çizgiler, 0.5 = Daha seyrek
        img.pixelsPerUnitMultiplier = 2f; 
    }
}
    public void HideMap()
    {
        mapPanel.SetActive(false);
    }
    public void ShowMap()
    {
        mapPanel.SetActive(true);
    }
    void Awake()
    {
        Instance = this;
    }

    public void SelectNode(MapNode targetNode)
    {
        // KONTROL: Oraya gitmeye iznimiz var mı?
        if (!IsMoveValid(targetNode))
        {
            Debug.Log("Oraya gidemezsin! Bağlantı yok.");
            return;
        }

        // HAREKET ONAYLANDI
        currentNode = targetNode;
        StartCoroutine(MovePlayerIconRoutine(targetNode.GetComponent<RectTransform>().anchoredPosition));

        // OLAYI BAŞLAT (Burası aynı)
        Debug.Log($"Gidilen yer: {targetNode.nodeType}");
         TriggerEvent(targetNode); 
    }

void TriggerEvent(MapNode node)
{
    // ... Gün ilerletme kodları ...

    // Olay Panelini Aç
    MapEventManager.Instance.TriggerEvent(node.nodeType);
}
    bool IsMoveValid(MapNode target)
    {
        // 1. Hiçbir yerde değilsek, sadece başlangıç noktalarına gidebiliriz
        if (currentNode == null)
        {
            return startingNodes.Contains(target);
        }

        // 2. Bir yerdeysen, sadece oradan çıkan yollara (outgoing) gidebilirsin
        return currentNode.outgoingPaths.Contains(target);
    }

    IEnumerator MovePlayerIconRoutine(Vector2 targetPos)
    {
        float duration = 0.5f;
        float timer = 0;
        Vector2 startPos = playerIcon.anchoredPosition;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            playerIcon.anchoredPosition = Vector2.Lerp(startPos, targetPos, timer / duration);
            yield return null;
        }
        playerIcon.anchoredPosition = targetPos;
    }
}