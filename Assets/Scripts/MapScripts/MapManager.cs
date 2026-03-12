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
        topPanel.SetActive(true);
        isMapOpen = false;
        if (LootChest.Instance != null)
        {
            LootChest.Instance.CheckForLoot();
        }
    }
    
    public void ShowMap()
    {
        CloseAllOpenPanels();
        mapPanel.SetActive(true);
        topPanel.SetActive(false);
       isMapOpen = true;
       
       AudioManager.Instance.PlayMap();
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
    public void RetreatToPreviousNode()
    {
        if (previousNode != null)
        {
            Debug.Log($"Savaş Kaybedildi! Geri çekiliniyor: {previousNode.nodeType}");
            
            currentNode = previousNode;
            sessionLastNode = sessionPreviousNode;


            playerIcon.anchoredPosition = previousNode.GetComponent<RectTransform>().anchoredPosition + iconOffset;
        }
        else
        {
            Debug.LogWarning("Geri dönülecek bir önceki konum bulunamadı!");
        }
    }
    public void CloseAllOpenPanels()
    {
     
        if (InventoryUIManager.Instance != null) 
        {
            InventoryUIManager.Instance.CloseInventory();
        }

     
        if (GladiatorSelector.Instance != null) 
        {
            GladiatorSelector.Instance.ClearSelection();
        }

       
        if (TrainingUIManager.Instance != null)
        {
            TrainingUIManager.Instance.SetCurrentGladiator(null);
        }

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