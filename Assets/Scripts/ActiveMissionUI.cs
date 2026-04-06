using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActiveMissionUI : MonoBehaviour
{
    public TextMeshProUGUI missionNameText;
    public TextMeshProUGUI timeText; 
    public Image progressBar;

    public void Setup(string name, int totalDays)
    {
        missionNameText.text = name;
        UpdateVisuals(totalDays, totalDays); 
    }

    public void UpdateVisuals(int remaining, int total)
    {
      
        if (remaining <= 0)
        {
            timeText.text = "Dönüyorlar...";
            progressBar.fillAmount = 1f;
        }
        else
        {
            timeText.text = remaining + " Gün Kaldı";
            
          
            float progress = (float)(total - remaining) / total;
            progressBar.fillAmount = progress;
        }
    }
}