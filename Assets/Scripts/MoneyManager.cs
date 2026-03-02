using UnityEngine;
using System;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    public int gold = 200;

    public int trainingCost = 30;
    //public int dailyIncome = -20;//YEMEK PARASI

    public event Action<int> OnGoldChanged;

    void Awake()
    {
        Instance = this;

       // gold = PlayerPrefs.GetInt("PlayerGold", 200);
    }

    void Start()
    {
        // UI'ın açılır açılmaz güncellenmesi için
        RefreshUI();
    }   
    /*
    private void SaveGold()
    {
        PlayerPrefs.SetInt("PlayerGold", gold);
        PlayerPrefs.Save();
    }*/
    public bool Spend(int amount)
    {
        if (gold < amount) {
            AudioManager.Instance.PlayError();
            return false;
        }
       // SaveGold();
        gold -= amount;
        AudioManager.Instance.PlayGold();
        OnGoldChanged?.Invoke(gold);
        return true;
    }

    public void Add(int amount)
    {
        gold += amount;
     //   SaveGold();
        OnGoldChanged?.Invoke(gold);
    }

    public int GetExpectedDailyWageCost()
    {
        int totalWage = 0;
        Gladiator[] soldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);
        
        foreach (var soldier in soldiers)
        {
            GladiatorAI ai = soldier.GetComponent<GladiatorAI>();
            if (soldier.CompareTag("MySoldier") && (ai == null || !ai.isDead))
            {
                int lvl = (soldier.data != null) ? soldier.data.level : 1;
                totalWage += (lvl * 5); // Her seviye için 5 Akçe maaş (Örn: Lvl 3 = 15 Akçe)
            }
        }
        return totalWage;
    }

    // --- DEĞİŞEN KISIM BURASI ---
   // Artık içine kaç gün geçtiğini alıyor (Varsayılan olarak 1)
    public void EndOfDay(int daysPassed = 1)
    {
        // 1. Ödememiz gereken toplam maaşı hesapla ve GEÇEN GÜN İLE ÇARP
        int dailyWage = GetExpectedDailyWageCost();
        int totalWageToPay = dailyWage * daysPassed;

        if (totalWageToPay == 0) return; // Asker yoksa dert de yok

        // 2. KONTROL: KASADA YETERLİ PARA VAR MI?
        if (gold >= totalWageToPay)
        {
            // --- DURUM İYİ: MAAŞLAR ÖDENDİ ---
            gold -= totalWageToPay;
            
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.Show($"Gün sonu: Askerlere {daysPassed} günlük toplam {totalWageToPay} Akçe ulufe ödendi.", NotificationType.Info);
            }
        }
        else
        {
            // --- DURUM KÖTÜ: PARA YETMEDİ ---
            gold = 0; 
            
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.Show($"<color=red>KASADA PARA YOK!</color> {daysPassed} günlük ulufe ödenemedi, ordu isyankar!", NotificationType.Warning);
            }

            // Asıl ceza: Moralleri ciddi şekilde düşür!
            if (CampMoraleManager.Instance != null)
            {
                CampMoraleManager.Instance.ChangeMorale(-15); 
            }
        }

        OnGoldChanged?.Invoke(gold);
    }
    public void RefreshUI()
    {
        OnGoldChanged?.Invoke(gold);
    }
}
