using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapNode : MonoBehaviour
{
    [Header("Veri")]
    public NodeType nodeType;
    public int layerIndex; // Kaçıncı menzilde? (0, 1, 2...)
    public List<MapNode> accessibleNodes = new List<MapNode>(); // Buradan nerelere gidilebilir?

    [Header("Görsel")]
    public Button nodeButton;
    public Image iconImage;
    
    // Çizgileri çizmek için referans (Sonra ekleyeceğiz)
    
    public void Setup(NodeType type, int layer)
    {
        nodeType = type;
        layerIndex = layer;
        
        // Tipine göre ikon değiştir (İleride Sprite listesinden çekeriz)
        // iconImage.sprite = MapManager.Instance.GetIcon(type);
    }
    void Start()
{

    if (nodeButton != null)
    {
        nodeButton.onClick.AddListener(OnNodeClicked);
    }
}
    // Tıklanınca çalışacak
    public void OnNodeClicked()
    {
        MapManager.Instance.SelectNode(this);
    }
}