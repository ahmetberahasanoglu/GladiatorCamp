using UnityEngine;
using TMPro;
using UnityEngine.UI; // YENİ: UI işlemleri için

public class CampMoraleManager : MonoBehaviour
{
    public static CampMoraleManager Instance;

    [Header("UI Ayarları")]
    public TextMeshProUGUI moraleText; 
    public Image moraleFillBar; // YENİ: Moral barının imajı

    [Header("Durum")]
    [Range(0, 100)]
    public int currentMorale = 80; 

    private bool isInitialized = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
        isInitialized = true;
    }

    public void ChangeMorale(int amount)
    {
        if (amount == 0) return;
        currentMorale += amount;
        currentMorale = Mathf.Clamp(currentMorale, 0, 100);
        
        UpdateUI();

        if (isInitialized && TopInfoBarUI.Instance != null && moraleText != null)
        {
            TopInfoBarUI.Instance.FlashUI(moraleText);
        }

        if (NotificationManager.Instance != null)
        {
            if (amount > 0)
                NotificationManager.Instance.Show($"Kışla Morali Yükseldi! (+{amount})", NotificationType.Success);
            else if (amount < 0)
                NotificationManager.Instance.Show($"Kışla Morali Düştü! ({amount})", NotificationType.Error);
        }
    }

    void UpdateUI()
    {
        if (moraleText != null)
        {
            moraleText.text = $"{currentMorale}";
            if (currentMorale <= 40) moraleText.color = Color.red;    
            else moraleText.color = Color.black; 
        }

        // YENİ: Barı Güncelle
        if (moraleFillBar != null)
        {
            // Moral max 100 olduğu için direkt 100'e bölüyoruz
            moraleFillBar.fillAmount = Mathf.Clamp01((float)currentMorale / 100f);
        }
    }
}