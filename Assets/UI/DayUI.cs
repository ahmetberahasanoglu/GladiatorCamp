using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DayUI : MonoBehaviour
{
    [Header("Gün Sayacı")]
    public TextMeshProUGUI dayText; // "Gün: 45"

    [Header("Olay Penceresi (Popup)")]
    public GameObject eventPanel;
    public TextMeshProUGUI eventTitle;
    public TextMeshProUGUI eventDesc;
    public TextMeshProUGUI eventEffect;
    public Button closeButton;

    void Start()
    {
        eventPanel.SetActive(false); // Başta kapalı
        closeButton.onClick.AddListener(ClosePanel); 
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayChanged += UpdateDayText;
            DayManager.Instance.OnEventTriggered += ShowEventPopup;
        }
        UpdateDayText(DayManager.Instance.currentDay);
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayChanged -= UpdateDayText;
            DayManager.Instance.OnEventTriggered -= ShowEventPopup;
        }
    }



void UpdateDayText(int day)
{
     dayText.text = $"Gün: {day}";
    
    //int startYear = 1420;
   // int currentYear = startYear + (day / 365);
    //int dayOfYear = day % 365;
   // dayText.text = $"Sene: {currentYear} | Gün: {dayOfYear}";
}

    void ShowEventPopup(GameEvent gameEvent)
    {
        eventPanel.SetActive(true);
        eventTitle.text = gameEvent.title;
        eventDesc.text = gameEvent.description;
        string effectStr = "";
        if (gameEvent.moneyChange != 0) effectStr += $"{gameEvent.moneyChange} Akçe\n";
        if (gameEvent.foodChange != 0) effectStr += $"{gameEvent.foodChange} Erzak\n";
        if (gameEvent.moraleChange != 0) effectStr += $"{gameEvent.moraleChange} Moral\n";
        
        eventEffect.text = effectStr;
        
        // Time.timeScale = 0;
    }

    void ClosePanel()
    {
        eventPanel.SetActive(false);
        // Time.timeScale = 1; // Oyunu devam ettir
    }
}