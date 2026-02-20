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
    public float moveDuration = 1.2f; // Gitme süresi (Saniye)
    
    [Header("Sallanma (Cartoon Efekt)")]
    public float wiggleSpeed = 6f; // Ne kadar hızlı sallanacak
    public float wiggleAngle = 12f; // Kaç derece sağa/sola yatacak

    [Header("Ayarlar")]
    public List<MapNode> startingNodes; 
    public MapNode currentNode; // Şu an neredeyiz?

    [Header("Çizgi Ayarları")]
    public GameObject linePrefab; 
    public Transform lineContainer; 

    // Oturum boyunca konumu aklında tutacak statik değişken
    public static string sessionLastNode = "";

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        DrawAllConnections();
        LoadPlayerPosition();
    }

    public void HideMap()
    {
        mapPanel.SetActive(false);
    }
    
    public void ShowMap()
    {
        mapPanel.SetActive(true);
    }

    void DrawAllConnections()
    {
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

    void CreateVisualLine(RectTransform start, RectTransform end)
    {
        GameObject lineObj = Instantiate(linePrefab, lineContainer);
        lineObj.transform.SetAsFirstSibling(); 

        RectTransform rect = lineObj.GetComponent<RectTransform>();
        
        Vector3 diff = end.localPosition - start.localPosition;
        Vector3 midpoint = (start.localPosition + end.localPosition) / 2;
        
        rect.localPosition = midpoint;
        rect.sizeDelta = new Vector2(diff.magnitude, 5); 
        
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        rect.rotation = Quaternion.Euler(0, 0, angle);

        Image img = lineObj.GetComponent<Image>();
        if (img != null)
        {
            img.type = Image.Type.Tiled; 
            img.pixelsPerUnitMultiplier = 2f; 
        }
    }

    void LoadPlayerPosition()
    {
        // 1. Hafızada (O anki oturumda) kayıtlı bir düğüm var mı?
        if (!string.IsNullOrEmpty(sessionLastNode))
        {
            GameObject savedNodeObj = GameObject.Find(sessionLastNode);
            
            if (savedNodeObj != null)
            {
                MapNode savedNode = savedNodeObj.GetComponent<MapNode>();
                if (savedNode != null)
                {
                    currentNode = savedNode;
                    playerIcon.anchoredPosition = savedNode.GetComponent<RectTransform>().anchoredPosition;
                    Debug.Log("Sahneler arası dönüş: " + sessionLastNode + " noktasına dönüldü.");
                    return; // İşlem bitti
                }
            }
        }

        // 2. KAYIT YOKSA (Oyuna veya Editöre ilk defa Play denildiyse):
        /*
        if (startingNodes.Count > 0 && startingNodes[0] != null)
        {
            currentNode = startingNodes[0];
            playerIcon.anchoredPosition = currentNode.GetComponent<RectTransform>().anchoredPosition;
            
            // Başlangıç noktasını da oturum hafızasına yazalım ki hata olmasın
            sessionLastNode = currentNode.gameObject.name; 
            
            Debug.Log("Yeni oyun: Başlangıç noktasına gidildi.");
        }*/
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
        StartCoroutine(MoveIconRoutine(targetNode));
    }

    bool IsMoveValid(MapNode target)
    {
        if (currentNode == null)
        {
            return startingNodes.Contains(target);
        }
        return currentNode.outgoingPaths.Contains(target);
    }

    public IEnumerator MoveIconRoutine(MapNode targetNode)
    {
        RectTransform targetRect = targetNode.GetComponent<RectTransform>();
        
        // UI elemanları olduğu için anchoredPosition kullanıyoruz
        Vector2 startPos = playerIcon.anchoredPosition;
        Vector2 targetPos = targetRect.anchoredPosition;

        // 1. ADIM: YÜZÜNÜ DÖN (FLIP)
        Vector3 currentScale = playerIcon.localScale;
        if (targetPos.x > startPos.x)
        {
            playerIcon.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
        else if (targetPos.x < startPos.x)
        {
            playerIcon.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }

        // 2. ADIM: SMOOTH HAREKET VE SALLANMA
        float t = 0;
        
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            playerIcon.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothT);

            float currentZAngle = Mathf.Sin(t * Mathf.PI * wiggleSpeed) * wiggleAngle;
            playerIcon.rotation = Quaternion.Euler(0, 0, currentZAngle);

            yield return null;
        }

        // 3. ADIM: TAMAMLAMA
        playerIcon.anchoredPosition = targetPos;
        playerIcon.rotation = Quaternion.Euler(0, 0, 0); 
        
        Debug.Log($"Yolculuk Tamamlandı! Gidilen yer: {targetNode.nodeType}");
        
        // --- SADECE OTURUMA (STATE) KAYDET ---
        sessionLastNode = targetNode.gameObject.name;
        // -------------------------------------

        // 4. ADIM: EVENT BURADA TETİKLENİYOR
        TriggerEvent(targetNode);
    }

    void TriggerEvent(MapNode node)
    {
        if (MapEventManager.Instance != null)
        {
            MapEventManager.Instance.TriggerEvent(node.nodeType);
        }
    }
}