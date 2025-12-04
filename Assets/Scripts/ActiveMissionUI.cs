using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActiveMissionUI : MonoBehaviour
{
    public TextMeshProUGUI missionNameText;
    public TextMeshProUGUI timeText; // "3 Gün Kaldı"
    public Image progressBar;

    public void Setup(string name, int totalDays)
    {
        missionNameText.text = name;
        UpdateVisuals(totalDays, totalDays); // İlk başta full veya boş başlar
    }

    public void UpdateVisuals(int remaining, int total)
    {
        // Kalan gün 0 ise (bugün dönüyorlar demektir), "Son Gün" yazabiliriz
        if (remaining <= 0)
        {
            timeText.text = "Dönüyorlar...";
            progressBar.fillAmount = 1f;
        }
        else
        {
            timeText.text = remaining + " Gün Kaldı";
            
            // Geçen süreyi hesapla: (Toplam - Kalan) / Toplam
            float progress = (float)(total - remaining) / total;
            progressBar.fillAmount = progress;
        }
    }
}