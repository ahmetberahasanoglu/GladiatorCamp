using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("UI")]
    public GameObject mapPanel;
    public GameObject topPanel;
    public RectTransform playerIcon;
    public float moveDuration = 1.2f; 

    [Header("Hiza Ayarı")]
    public Vector2 iconOffset = new Vector2(-40f, 0f); 

    [Header("Sallanma (Cartoon Efekt)")]
    public float wiggleSpeed = 6f; 
    public float wiggleAngle = 12f; 

    [Header("Ayarlar")]
    public List<MapNode> startingNodes; 
    public MapNode currentNode;
    [Header("Geri Çekilme (Retreat) Hafızası")]
    public MapNode previousNode;
    public static string sessionPreviousNode = ""; 

    [Header("Çizgi Ayarları")]
    public GameObject linePrefab; 
    public Transform lineContainer; 
    public static string sessionLastNode = "";
    public bool isMapOpen = false;

    [Header("Sis ve Çizgi Ayarları")]
    public List<MapNode> visitedNodes = new List<MapNode>();
    private List<MapLine> allLines = new List<MapLine>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        DrawAllConnections();
        LoadPlayerPosition();
        UpdateMapVisibility(); 
    }

    public void HideMap()
    {
        mapPanel.SetActive(false);
        //topPanel.SetActive(true);
        isMapOpen = false;
        PauseEnvironmentSystems(false); // Harita kapanınca sistemler devam eder
        if (LootChest.Instance != null)
        {
            LootChest.Instance.CheckForLoot();
        }
    }
    
    // YENİ: Kampa Dönüş Metodu (Butondan çağrılacak)
    public void ReturnToCamp()
    {
        if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.isExpeditionActive)
        {
            ExpeditionManager.Instance.ReturnToCampSafely();
        }

        // Haritayı sıfırla (Böylece oyuncu haritayı bir daha açtığında baştan başlar)
        ResetMapProgress();
        HideMap();
    }
    
    public void ResetMapProgress()
    {
        currentNode = null;
        previousNode = null;
        sessionLastNode = "";
        sessionPreviousNode = "";
        visitedNodes.Clear();


        SnapToStartPoint();

        UpdateMapVisibility();
        Debug.Log("Harita sıfırlandı. Yeni sefer için hazır.");
    }

    public void ShowMap()
    {
        CloseAllOpenPanels();

        // YENİ: Eğer halihazırda bir seferde değilsek (yani kamptan yeni çıkıyorsak)
        if (ExpeditionManager.Instance != null && !ExpeditionManager.Instance.isExpeditionActive)
        {
            // Eğer yeni çıkıyorsa harita progress'ini de sıfırlayalım ki eski sisli yerler olmasın
            ResetMapProgress(); 
            ExpeditionManager.Instance.StartExpedition();
        }

        mapPanel.SetActive(true);
        //topPanel.SetActive(false);
        isMapOpen = true;
       
        AudioManager.Instance.PlayMap();
        UpdateMapVisibility();
    }
// MapManager.cs içine bu yeni metodu ekle:
    void SnapToStartPoint()
    {
        // Haritadaki StartPoint node'unu bul
        MapNode[] allNodes = FindObjectsByType<MapNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        MapNode startNode = null;

        foreach(var node in allNodes)
        {
            if(node.nodeType == NodeType.StartPoint)
            {
                startNode = node;
                break;
            }
        }

        // Eğer StartPoint türünde bir node yoksa, startingNodes listesinin ilk elemanını zorla al
        if (startNode == null && startingNodes != null && startingNodes.Count > 0)
        {
            startNode = startingNodes[0];
        }

        // İkonu bulduğumuz başlangıç noktasına tam olarak oturt
        if (startNode != null)
        {
            currentNode = startNode;
            playerIcon.anchoredPosition = startNode.GetComponent<RectTransform>().anchoredPosition + iconOffset;
            sessionLastNode = startNode.gameObject.name;
            
            if (!visitedNodes.Contains(startNode)) visitedNodes.Add(startNode);
        }
    }
   

   
    public void UpdateMapVisibility()
    {
        // 1. ÇİZGİLERİ GÜNCELLE
        foreach (var line in allLines)
        {
            if ((line.startNode == currentNode && IsMoveValid(line.endNode)) || 
                (line.endNode == currentNode && IsMoveValid(line.startNode)))
            {
                line.lineImage.color = new Color(1f, 0f, 0f, 1f); // Gidilebilir (Parlak Kırmızı)
            }
            else if (visitedNodes.Contains(line.startNode) && visitedNodes.Contains(line.endNode))
            {
                line.lineImage.color = new Color(0.6f, 0f, 0f, 0.4f); // Geçilmiş (Soluk)
            }
            else
            {
                line.lineImage.color = new Color(0.1f, 0.1f, 0.1f, 0.3f); // Karanlık
            }
        }

        // 2. NODE'LARI GÜNCELLE
        var allNodes = FindObjectsByType<MapNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (var node in allNodes)
        {
            if (visitedNodes.Contains(node))
            {
                node.SetState(NodeState.Cleared);
            }
            else if (IsMoveValid(node))
            {
                node.SetState(NodeState.Accessible);
            }
            else if (node == currentNode)
            {
                node.SetState(NodeState.Cleared);
            }
            else
            {
                node.SetState(NodeState.Locked);
            }

            if (node.nodeButton != null)
            {
                node.nodeButton.interactable = IsMoveValid(node);
            }
        }
    }

    void DrawAllConnections()
    {
        var allNodes = FindObjectsByType<MapNode>(FindObjectsSortMode.None);
        foreach (var node in allNodes)
        {
            foreach (var target in node.outgoingPaths)
            {
                if (target != null)
                {
                    CreateVisualLine(node, target); 
                }
            }
        }
    }

    void CreateVisualLine(MapNode start, MapNode end)
    {
        GameObject lineObj = Instantiate(linePrefab, lineContainer);
        lineObj.transform.SetAsFirstSibling(); 

        RectTransform rect = lineObj.GetComponent<RectTransform>();
        RectTransform startRect = start.GetComponent<RectTransform>();
        RectTransform endRect = end.GetComponent<RectTransform>();
        
        Vector3 diff = endRect.localPosition - startRect.localPosition;
        Vector3 midpoint = (startRect.localPosition + endRect.localPosition) / 2;
        
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

        MapLine ml = lineObj.AddComponent<MapLine>();
        ml.startNode = start;
        ml.endNode = end;
        ml.lineImage = img;
        allLines.Add(ml);
    }
    
    void LoadPlayerPosition()
    {
        if (!string.IsNullOrEmpty(sessionLastNode))
        {
            GameObject savedNodeObj = GameObject.Find(sessionLastNode);
            
            if (savedNodeObj != null)
            {
                MapNode savedNode = savedNodeObj.GetComponent<MapNode>();
                if (savedNode != null)
                {
                    currentNode = savedNode;
                    playerIcon.anchoredPosition = savedNode.GetComponent<RectTransform>().anchoredPosition + iconOffset;
                    Debug.Log("Sahneler arası dönüş: " + sessionLastNode + " noktasına dönüldü.");
                    return;
                }
            }
        }
        
        SnapToStartPoint();
    }

    public void SelectNode(MapNode targetNode)
    {
        if (!IsMoveValid(targetNode))
        {
            Debug.Log("Oraya gidemezsin! Bağlantı yok.");
            return;
        }

        if (currentNode != null)
        {
            previousNode = currentNode;
            sessionPreviousNode = previousNode.gameObject.name;
            
            if (!visitedNodes.Contains(currentNode))
            {
                visitedNodes.Add(currentNode);
            }
        }
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

    // YENİ: Sürgün Yediğimizde / Askerler Öldüğünde Tetiklenir
    public void ExpeditionFailedFromMap()
    {
        if (ExpeditionManager.Instance != null)
        {
            ExpeditionManager.Instance.FailExpedition();
        }
        ResetMapProgress();
        HideMap();
    }

    public void RetreatToPreviousNode()
    {
        if (previousNode != null)
        {
            Debug.Log($"Savaş Kaybedildi! Geri çekiliniyor: {previousNode.nodeType}");
            
            currentNode = previousNode;
            sessionLastNode = sessionPreviousNode;
            playerIcon.anchoredPosition = previousNode.GetComponent<RectTransform>().anchoredPosition + iconOffset;

            if (visitedNodes.Contains(previousNode)) visitedNodes.Remove(previousNode);
        }
        else
        {
            Debug.Log("İlk savaş kaybedildi, sefer başarısız.");
            ExpeditionFailedFromMap();
            return;
        }

        UpdateMapVisibility(); 
    }

    public void CloseAllOpenPanels()
    {
        // PanelManager üzerinden açık olan her şeyi kapat
        if (PanelManager.Instance != null) PanelManager.Instance.CloseAll();

        // Belirli panel yöneticilerini kapat
        if (InventoryUIManager.Instance  != null) InventoryUIManager.Instance.CloseInventory();
        if (GladiatorSelector.Instance   != null) GladiatorSelector.Instance.ClearSelection();
        if (TrainingUIManager.Instance   != null) TrainingUIManager.Instance.SetCurrentGladiator(null);
        if (RepairPanelManager.Instance  != null) RepairPanelManager.Instance.ClosePanel();

        // Wanderer ve çevre sistemlerini durdur
        PauseEnvironmentSystems(true);
    }

    /// <summary>
    /// Harita açıkken wanderer, kar, kuş gibi sistemler durur.
    /// Harita kapanınca tekrar başlar.
    /// </summary>
    public void PauseEnvironmentSystems(bool pause)
    {
        // Kar/yağmur
        if (SeasonManager.Instance != null)
            SeasonManager.Instance.SetWeatherPaused(pause);

        // Kuzgun spawner
        var ravenSpawner = FindFirstObjectByType<RavenSpawner>();
        if (ravenSpawner != null) ravenSpawner.enabled = !pause;

        // Wandering strangers — sahnedekileri durdur
        var strangers = FindObjectsByType<WanderingStranger>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var s in strangers)
            s.SetPaused(pause);
    }

    public IEnumerator MoveIconRoutine(MapNode targetNode)
    {
        RectTransform targetRect = targetNode.GetComponent<RectTransform>();
        AudioManager.Instance.PlayMapMove();
        Vector2 startPos = playerIcon.anchoredPosition;
        Vector2 targetPos = targetRect.anchoredPosition + iconOffset;

        Vector3 currentScale = playerIcon.localScale;
        if (targetRect.anchoredPosition.x > startPos.x)
        {
            playerIcon.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
        else if (targetRect.anchoredPosition.x < startPos.x)
        {   
            playerIcon.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }

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

        playerIcon.anchoredPosition = targetPos;
        playerIcon.rotation = Quaternion.Euler(0, 0, 0); 
        
        Debug.Log($"Yolculuk Tamamlandı! Gidilen yer: {targetNode.nodeType}");
        
        sessionLastNode = targetNode.gameObject.name;
        UpdateMapVisibility();
        if (ExpeditionManager.Instance != null && ExpeditionManager.Instance.isExpeditionActive)
        {
            ExpeditionManager.Instance.AdvanceEncounter();
        }
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