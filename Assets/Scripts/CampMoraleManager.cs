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

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    // Morali artırmak veya azaltmak için her yerden çağrılabilir
    public void ChangeMorale(int amount)
    {
        currentMorale += amount;
        
        // Moral 0 ile 100 arasında kalmalı
        currentMorale = Mathf.Clamp(currentMorale, 0, 100);
        
        UpdateUI();

        // Görsel Geri Bildirim
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
            moraleText.text = $"Moral: %{currentMorale}";
            
            // Moralin durumuna göre modeli değiştirebilirm simdi değil
           
        }
    }
}