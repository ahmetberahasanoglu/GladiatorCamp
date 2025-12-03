using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionResultUI : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public TextMeshProUGUI titleText;       // "ZAFER!" veya "BOZGUN!"
    public TextMeshProUGUI missionNameText; // "Viyana Seferi"
    public TextMeshProUGUI descriptionText; // "Askerlerin ganimetle döndü..."
    public TextMeshProUGUI rewardText;      // "500 Akçe"
    
    public Image headerPanel;               // Renk değiştirmek için (Yeşil/Kırmızı)
    public Button closeButton;

    // Bu fonksiyonu MissionManager çağıracak
    public void Setup(bool isVictory, MissionData mission)
    {
        missionNameText.text = mission.missionName;

        if (isVictory)
        {
            // ZAFER DURUMU
            titleText.text = "ŞANLI ZAFER!";
            descriptionText.text = "Ordun muzaffer oldu, düşman darmadağın edildi.";
            rewardText.text = $"+{mission.goldReward} Akçe";
            
            // Paneli Yeşil yap
            headerPanel.color = new Color(0.2f, 0.6f, 0.2f); // Koyu Yeşil
        }
        else
        {
            // YENİLGİ DURUMU
            titleText.text = "ACI BOZGUN...";
            descriptionText.text = "Askerlerin geri çekilmek zorunda kaldı.";
            rewardText.text = "0 Akçe";
            
            // Paneli Kırmızı yap
            headerPanel.color = new Color(0.6f, 0.2f, 0.2f); // Koyu Kırmızı
        }

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(ClosePanel);
    }

    void ClosePanel()
    {
        // Pencereyi yok et
        Destroy(gameObject);
    }
}