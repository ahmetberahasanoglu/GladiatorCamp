using UnityEngine;
using TMPro;

public class CampMoraleManager : MonoBehaviour
{
    public static CampMoraleManager Instance;

    [Header("UI Ayarları")]
    public TextMeshProUGUI moraleText; // Üst bardaki "Moral: %100" yazısı

    [Header("Durum")]
    [Range(0, 100)]
    public int currentMorale = 80; // Oyuna %80 moralle başlasınlar

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

    // Morali artırmak veya azaltmak için her yerden çağrılabilir
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
            
            // Moralin durumuna göre modeli değiştirebilirm simdi değil
           
        }
    }
}