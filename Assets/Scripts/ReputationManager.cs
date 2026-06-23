using UnityEngine;
using System;

public class ReputationManager : MonoBehaviour
{
    public static ReputationManager Instance;

    [Header("İtibar Ayarları")]
    private int currentReputation = 80;
    public int maxReputation = 100;
    
    
    public event Action<int> OnReputationChanged;
    public event Action OnGameOver; 

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
      //  currentReputation = PlayerPrefs.GetInt("PlayerReputation", 100);
    }

    void Start()
    {
        OnReputationChanged?.Invoke(currentReputation);
    }

    public void ChangeReputation(int amount)
    {
        currentReputation += amount;
        currentReputation = Mathf.Clamp(currentReputation, 0, maxReputation);

      //  PlayerPrefs.SetInt("PlayerReputation", currentReputation);
      //  PlayerPrefs.Save();
        
        OnReputationChanged?.Invoke(currentReputation);

        if (currentReputation <= 0)
        {
            NotificationManager.Instance.Show("İtibarın sıfırlandı isyan çıkabilir!", NotificationType.Warning);
            OnGameOver?.Invoke();
            // Burada oyun bitirme panelini açacağız
        }
    }
// ... Mevcut kodların ...

    // UI Butonundan çağıracağımız fonksiyon
    public void DonateToState(int goldCost, int reputationGain)
    {
        // Paramız yetiyor mu?
        if (MoneyManager.Instance.gold >= goldCost)
        {
            // İtibar zaten 100 ise boşuna para harcatma
            if (currentReputation >= maxReputation)
            {
                NotificationManager.Instance.Show("Padişah senden zaten çok memnun! (İtibar dolu)", NotificationType.Info);
                return;
            }

            // Parayı harca
            MoneyManager.Instance.Spend(goldCost);

            // İtibarı artır
            ChangeReputation(reputationGain);
 NotificationManager.Instance.Show($"{goldCost} Akçe bağışlandı, {reputationGain} itibar kazanıldı.", NotificationType.Success);
        }
        else
        {
             NotificationManager.Instance.Show("Bağış yapacak paran yok!", NotificationType.Error);
        }
    }
    public int GetReputation()
    {
        return currentReputation;
    }
}