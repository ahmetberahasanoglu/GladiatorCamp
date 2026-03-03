using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("UI Referansları")]
    public GameObject tooltipPanel;       // Arka planı olan panel
    public TextMeshProUGUI tooltipText;   // İçindeki yazı
    
    [Header("Ayarlar")]
    public Vector2 offset = new Vector2(15f, -15f); // Farenin tam üstünü kapatmasın diye sağ alta kaydırıyoruz

    void Awake()
    {
        Instance = this;
        HideTooltip(); // Oyun başlarken gizli olsun
    }

    void Update()
    {
        // Eğer panel açıksa, farenin pozisyonunu takip et
        if (tooltipPanel.activeSelf)
        {
            // Farenin ekrandaki koordinatlarına offset (kayma) değerini ekleyip paneli oraya taşıyoruz
            transform.position = Input.mousePosition + (Vector3)offset;
        }
    }

    public void ShowTooltip(string text)
    {
        tooltipText.text = text;
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}