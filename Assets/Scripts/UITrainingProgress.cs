using UnityEngine;
using UnityEngine.UI;

public class UITrainingProgress : MonoBehaviour
{
    public static UITrainingProgress Instance;

    public Image fill;

    void Awake()
    {
        Instance = this;
        Hide();
    }

    public void StartProgress(int totalDays)
    {
        gameObject.SetActive(true);
        fill.fillAmount = 1f;
    }

    public void UpdateProgress(int remaining)
    {
        // ör: 3 günden 2 güne düştü → %66 göster
        float percent = remaining / 3f; // istersen totalDays'i parametre yapabilirsin
        fill.fillAmount = percent;
    }


    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
