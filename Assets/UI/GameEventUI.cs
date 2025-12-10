using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameEventUI : MonoBehaviour
{
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
        
        // Sadece "Olay Tetiklendi" eventini dinliyoruz
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnEventTriggered += ShowEventPopup;
        }
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnEventTriggered -= ShowEventPopup;
        }
    }

    void ShowEventPopup(GameEvent gameEvent)
    {
        eventPanel.SetActive(true);
        eventTitle.text = gameEvent.title;
        eventDesc.text = gameEvent.description;
        
        // Etkileri yazdır
        string effectStr = "";
        if (gameEvent.moneyChange != 0) effectStr += $"{Colorize(gameEvent.moneyChange)} Akçe\n";
        if (gameEvent.foodChange != 0) effectStr += $"{Colorize(gameEvent.foodChange)} Erzak\n";
        if (gameEvent.moraleChange != 0) effectStr += $"{Colorize(gameEvent.moraleChange)} Moral\n";
        
        eventEffect.text = effectStr;
        
        // Olay çıkınca oyunu durdurmak iyi bir fikirdir
        // Time.timeScale = 0; 
    }

    void ClosePanel()
    {
        eventPanel.SetActive(false);
        // Time.timeScale = 1; 
    }

    string Colorize(int val)
    {
        if (val > 0) return $"<color=green>+{val}</color>";
        if (val < 0) return $"<color=red>{val}</color>";
        return val.ToString();
    }
}