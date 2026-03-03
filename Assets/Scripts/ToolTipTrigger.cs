using UnityEngine;
using UnityEngine.EventSystems; // Fare etkileşimleri için şart

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(3, 5)] // Unity editöründe geniş bir yazı kutusu sağlar
    public string infoText;

    // Fare ikonun/objenin GİRDİĞİNDE çalışır
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ShowTooltip(infoText);
        }
    }

    // Fare ikonun/objenin ÇIKTIĞINDA çalışır
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }
}