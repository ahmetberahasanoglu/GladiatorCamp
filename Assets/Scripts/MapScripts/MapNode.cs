using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapNode : MonoBehaviour
{
    [Header("Ayarlar")]
    public NodeType nodeType;
    
    // NERELERE GİDİLEBİLİR? (Bunu elle dolduracaksın)
    public List<MapNode> outgoingPaths = new List<MapNode>(); 

    [Header("Görsel")]
    public Button nodeButton;
    public Image iconImage;
public Image contentImage; // Butonun içindeki İKON resmi (Çerçeve değil)

    public void Setup(NodeType type, int layer, MapConfig config) // Config parametresi ekledik
    {
        nodeType = type;
        
        // Config dosyasından doğru resmi çek
        if (config != null && contentImage != null)
        {
            contentImage.sprite = config.GetIcon(type);
            contentImage.preserveAspect = true; // Resim sünmesin diye
        }
    }
    void Start()
    {
        // Buton tıklamasını dinle
        if (nodeButton != null)
        {
            // Önce temizle sonra ekle (Hata önlemek için)
            nodeButton.onClick.RemoveAllListeners();
            nodeButton.onClick.AddListener(OnNodeClicked);
        }
    }

    public void OnNodeClicked()
    {
        // Tıklanınca Manager'a haber ver
        if (MapManager.Instance != null)
        {
            MapManager.Instance.SelectNode(this);
        }
    }

    // --- EDİTÖRDE ÇİZGİLERİ GÖRMEK İÇİN SİHİRLİ KOD ---
    // Bu kod oyun çalışırken değil, sen editörde tasarım yaparken çalışır.
    void OnDrawGizmos()
    {
        if (outgoingPaths == null || outgoingPaths.Count == 0) return;

        Gizmos.color = Color.yellow; // Çizgi rengi

        foreach (var node in outgoingPaths)
        {
            if (node != null)
            {
                // Mevcut noktadan, hedef noktaya sarı bir çizgi çek
                Gizmos.DrawLine(transform.position, node.transform.position);
                
                // Ok ucu gibi bir küre koy (Yönü belli olsun)
                Vector3 direction = (node.transform.position - transform.position).normalized;
                Gizmos.DrawSphere(Vector3.Lerp(transform.position, node.transform.position, 0.2f), 10f);
            }
        }
    }
}