using UnityEngine;
using UnityEngine.EventSystems;

public class MapNavigator : MonoBehaviour, IDragHandler, IScrollHandler
{
    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private RectTransform parentRect;

    [Header("Sürükleme Ayarları")]
    public bool canDrag = true;

    [Header("Yakınlaştırma (Zoom) Ayarları")]
    public float zoomSpeed = 0.1f;
    public float minZoom = 1.0f;   // DİKKAT: Artık en az 1x (tam ekran) olabilir
    public float maxZoom = 2.5f;   // Ne kadar yakınlaşılabilir?

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        
        // Haritanın içinde bulunduğu ana ekranın boyutlarını almak için
        if (transform.parent != null)
        {
            parentRect = transform.parent.GetComponent<RectTransform>();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag || parentCanvas == null) return;

        // Fare hareketini ekle
        rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;

        // Haritanın ekranın dışına (boşluğa) çıkmasını engelle
        ClampPosition();
    }

    public void OnScroll(PointerEventData eventData)
    {
        float scroll = eventData.scrollDelta.y;

        if (scroll != 0)
        {
            Vector3 currentScale = rectTransform.localScale;
            float newScaleVal = currentScale.x + (scroll * zoomSpeed);

            newScaleVal = Mathf.Clamp(newScaleVal, minZoom, maxZoom);
            rectTransform.localScale = new Vector3(newScaleVal, newScaleVal, 1f);

            // Zoom yaparken de harita yanlışlıkla kenardan boşluk bırakmasın diye sınırla
            ClampPosition();
        }
    }

    // Haritanın kenarlarının ekranın içine girmesini engelleyen matematik
    private void ClampPosition()
    {
        if (parentRect == null) return;

        // Haritanın mevcut zoom ile ulaştığı gerçek genişlik ve yükseklik
        float currentWidth = rectTransform.rect.width * rectTransform.localScale.x;
        float currentHeight = rectTransform.rect.height * rectTransform.localScale.y;

        // Ekranın (veya kapsayıcının) genişliği ve yüksekliği
        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        // Gidebileceği maksimum X ve Y sınırlarını hesapla
        float maxX = Mathf.Max(0, (currentWidth - parentWidth) / 2f);
        float maxY = Mathf.Max(0, (currentHeight - parentHeight) / 2f);

        // Pozisyonu bu sınırlar içinde tut (Boşluk görmeyi engeller)
        Vector2 clampedPosition = rectTransform.anchoredPosition;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -maxX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -maxY, maxY);

        rectTransform.anchoredPosition = clampedPosition;
    }
}