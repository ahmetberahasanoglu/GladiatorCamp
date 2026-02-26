using UnityEngine;
using UnityEngine.EventSystems;

public class MapNavigator : MonoBehaviour, IDragHandler, IScrollHandler
{
    private RectTransform rectTransform;
    private Canvas parentCanvas;

    [Header("Sürükleme Ayarları")]
    public bool canDrag = true;

    [Header("Yakınlaştırma (Zoom) Ayarları")]
    public float zoomSpeed = 0.1f;
    public float minZoom = 0.5f;   // Ne kadar uzaklaşılabilir?
    public float maxZoom = 2.0f;   // Ne kadar yakınlaşılabilir?

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    // Fareyle haritaya tıklayıp sürüklendiğinde çalışır
    public void OnDrag(PointerEventData eventData)
    {
        if (!canDrag || parentCanvas == null) return;

        // Farenin hareketini (delta) Canvas'ın ölçeğine bölerek pozisyona ekliyoruz.
        // Bu sayede fareyi ne kadar çekersek, harita da birebir o kadar kayar.
        rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
    }

    // Fare tekerleği (Scroll) çevrildiğinde çalışır
    public void OnScroll(PointerEventData eventData)
    {
        float scroll = eventData.scrollDelta.y;

        if (scroll != 0)
        {
            Vector3 currentScale = rectTransform.localScale;
            float newScaleVal = currentScale.x + (scroll * zoomSpeed);

            // Zoom değerini minimum ve maksimum sınırların içinde tut
            newScaleVal = Mathf.Clamp(newScaleVal, minZoom, maxZoom);

            // Haritayı yeni değerle büyüt/küçült
            rectTransform.localScale = new Vector3(newScaleVal, newScaleVal, 1f);
        }
    }
}