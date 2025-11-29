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

    public void StartProgress(float duration)
    {
        fill.fillAmount = 0;
        gameObject.SetActive(true);
    }

    public void UpdateProgress(float percent)
    {
        fill.fillAmount = percent;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
