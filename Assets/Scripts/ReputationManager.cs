using UnityEngine;
using System;

public class ReputationManager : MonoBehaviour
{
    public static ReputationManager Instance;

    [Header("İtibar Ayarları")]
    public int currentReputation = 100;
    public int maxReputation = 100;
    
    
    public event Action<int> OnReputationChanged;
    public event Action OnGameOver; 

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        OnReputationChanged?.Invoke(currentReputation);
    }

    public void ChangeReputation(int amount)
    {
        currentReputation += amount;
        currentReputation = Mathf.Clamp(currentReputation, 0, maxReputation);
        OnReputationChanged?.Invoke(currentReputation);

        if (currentReputation <= 0)
        {
            Debug.Log("İtibar bitti! Ocak kapatılıyor...");
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
                Debug.Log("Padişah senden zaten çok memnun! (İtibar dolu)");
                return;
            }

            // Parayı harca
            MoneyManager.Instance.Spend(goldCost);

            // İtibarı artır
            ChangeReputation(reputationGain);

            Debug.Log($"{goldCost} Akçe bağışlandı, {reputationGain} itibar kazanıldı.");
        }
        else
        {
            Debug.Log("Bağış yapacak paran yok!");
        }
    }
    public int GetReputation()
    {
        return currentReputation;
    }
}