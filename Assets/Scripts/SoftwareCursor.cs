using UnityEngine;
using UnityEngine.UI; // YENİ: UI kullandığımız için eklendi

public class SoftwareCursor : MonoBehaviour
{
    [Header("Tıklama Merkezi (Hotspot)")]
    // Eğer imlecin sivri ucu görselin sol üstündeyse 0,0 kalsın. 
    // Eğer ortasındaysa bu değerlerle oynayarak tıklama hissini merkeze alabilirsin.
    public Vector2 hotspotOffset; 

    private RectTransform _rectTransform;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        
        // İşletim sisteminin o küçük ve sıkıcı imlecini tamamen gizle!
        Cursor.visible = false;
    }

    void Update()
    {
        // UI imlecini farenin anlık ekran pozisyonuna taşı
        Vector2 mousePos = Input.mousePosition;
        _rectTransform.position = mousePos + hotspotOffset;
    }

    // Oyun duraklatıldığında veya alt-tab yapıldığında işletim sistemi imlecini geri getirmek isteyebilirsin
    void OnApplicationFocus(bool hasFocus)
    {
        Cursor.visible = !hasFocus;
    }
}