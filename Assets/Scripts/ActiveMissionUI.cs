using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActiveMissionUI : MonoBehaviour
{
    public TextMeshProUGUI missionNameText;
    public TextMeshProUGUI timerText;
    public Image progressBar;

    private float totalDuration;
    private float timeElapsed;
    
    // Bu scripti MissionManager yönetecek, o yüzden Update kullanmıyoruz.
    // Manager her frame'de buraya "Güncelle" emri verecek.
    
    public void Setup(string name, float duration)
    {
        missionNameText.text = name;
        totalDuration = duration;
        timeElapsed = 0;
        UpdateVisuals(0);
    }

    public void UpdateProgress(float deltaTime)
    {
        timeElapsed += deltaTime;
        UpdateVisuals(timeElapsed);
    }

    void UpdateVisuals(float current)
    {
        float ratio = current / totalDuration;
        progressBar.fillAmount = ratio;
        
        float remaining = totalDuration - current;
        timerText.text = remaining.ToString("F1") + " sn"; 
        // İstersen burayı "Gün" olarak da gösterebilirsin.
    }
}