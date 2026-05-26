using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Paralı asker satın alma paneli. (Sadeleştirilmiş Tekli Sistem)
/// </summary>
public class MercenaryCampUI : MonoBehaviour
{
    public static MercenaryCampUI Instance;

    [Header("Panel")]
    public GameObject panel;

    [Header("Başlık ve Kapasite")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI capacityText;

    [Header("Kart Alanı")]
    public Transform      cardContainer;
    public GameObject     mercenaryCardPrefab;  

    [Header("Kapat Butonu")]
    public Button closeButton;

    private MercenaryCamp _camp;

    void Awake()
    {
        Instance = this;
        if (panel != null) panel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    public void Open(MercenaryCamp camp)
    {
        _camp = camp;
        panel.SetActive(true);
        Refresh();
    }

    public void Refresh()
    {
        if (_camp == null) return;

        // Kapasite bilgisi
        int capacity     = CampManager.Instance != null ? CampManager.Instance.GetMaxSoldierCapacity() : 3;
        int soldierCount = CampManager.Instance != null ? CampManager.Instance.GetLivingSoldiers().Count : 0;
        bool isFull      = soldierCount >= capacity;

        if (capacityText != null)
        {
            capacityText.text = $"Koğuş: <color={(isFull ? "red" : "black")}>{soldierCount}/{capacity}</color>";
        }

        if (titleText != null)
            titleText.text = isFull
                ? "<color=red>Koğuş Dolu — Önce Koğuşu Yükselt</color>"
                : "Paralı Asker Çadırı";

        // Kartları temizle
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        // Sabit tek bir standart paralı asker kartı oluştur
        MercenaryData basicSoldier = _camp.GetBasicMercenary();

        GameObject cardObj = Instantiate(mercenaryCardPrefab, cardContainer);
        var card = cardObj.GetComponent<MercenaryCardUI>();
        card?.Setup(basicSoldier, _camp, isFull);
    }

    public void Close()
    {
        panel.SetActive(false);
        _camp = null;
    }
}