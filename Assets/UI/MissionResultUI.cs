using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionResultUI : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    public TextMeshProUGUI titleText;      
    public TextMeshProUGUI missionNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI rewardText;     
    
    public Image headerPanel;              
    public Button closeButton;

    public void Setup(bool isVictory, MissionData mission)
    {
        missionNameText.text = mission.missionName;

        if (isVictory)
        {
     
            titleText.text = "GÖREV BAŞARILI!";
            descriptionText.text = "Ordun muzaffer oldu, düşman darmadağın edildi.";
            rewardText.text = $"+{mission.goldReward} Akçe\n+{mission.woodReward} Odun";
            
           
           // headerPanel.color = new Color(0.2f, 0.6f, 0.2f); // Koyu Yeşil
        }
        else
        {
            // YENİLGİ DURUMU
            titleText.text = "GÖREV BAŞARISIZ!";
            descriptionText.text = "Askerler eli boş döndü.";
            rewardText.text = "0 Akçe";
            
            
           // headerPanel.color = new Color(0.6f, 0.2f, 0.2f); // Koyu Kırmızı
        }

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(ClosePanel);
    }

    void ClosePanel()
    {
        Destroy(gameObject);
    }
}