using UnityEngine;
using System.Collections.Generic;

public class SupplyManager : MonoBehaviour
{
    public static SupplyManager Instance;

    [Header("Erzak Durumu")]
    public int currentFood = 50;   
    public int foodCost = 5;       
    public int baseFoodPerSoldier = 1; 
[Header("Kapasite")]
    public int maxFood = 100;
    public event System.Action OnFoodChanged;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Event aboneliklerini sildik! Artık DayManager bizi doğrudan çağıracak.
        UpdateUI(); 
    }

 public int GetExpectedDailyFoodCost()
    {
        int totalFoodNeeded = 0;
        Gladiator[] soldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);

        foreach (var soldier in soldiers)
        {
            GladiatorAI ai = soldier.GetComponent<GladiatorAI>();
            if (soldier.CompareTag("MySoldier") && (ai == null || !ai.isDead))
            {
                int lvl = (soldier.data != null) ? soldier.data.level : 1;
                
                int dailyFood = baseFoodPerSoldier * lvl; 
                
         
                if (soldier.data != null && soldier.data.trait == SoldierTrait.Obur)
                {
                    dailyFood *= 2; 
                }
                
                totalFoodNeeded += dailyFood; 
            }
        }
        return totalFoodNeeded;
    }


    public void ConsumeDailyFood(int daysPassed)
    {
        int dailyNeededFood = GetExpectedDailyFoodCost();
        int totalNeededFood = dailyNeededFood * daysPassed;

        if (totalNeededFood == 0) return; 

        if (currentFood >= totalNeededFood)
        {
            currentFood -= totalNeededFood;
            AudioManager.Instance.PlayEat();
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show($"{daysPassed} günde toplam {totalNeededFood} birim erzak tüketildi.", NotificationType.Info);
        }
        else
        {
            currentFood = 0; 
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show($"<color=red>ERZAK BİTTİ!</color> {daysPassed} günlük erzak yetmedi, askerler açlıktan bitkin düştü!", NotificationType.Warning);

            if (CampMoraleManager.Instance != null) CampMoraleManager.Instance.ChangeMorale(-10);
        }

        UpdateUI();
    }

    public void BuyFood(int amount)
    {
        int totalCost = amount * foodCost;
        if (MoneyManager.Instance.gold >= totalCost)
        {
            MoneyManager.Instance.Spend(totalCost);
            currentFood += amount;
            if (currentFood > maxFood) currentFood = maxFood;
            //Currentfood-amount birim erzak alındı deniycem
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show($"{amount} birim erzak alındı.", NotificationType.Info);
            UpdateUI();
        }
        else
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Yemek için yeterli akçe yok!", NotificationType.Error);
        }
    }
    // --- DIŞARIDAN ERZAK EKLEME (Yağma, Sandık, Ödül vb.) ---
  public void AddFood(int amount)
    {
        if (amount <= 0) return;
        currentFood += amount;
        
        // YENİ: Kapasiteyi aşmasını engelle
        if (currentFood > maxFood) currentFood = maxFood; 

        UpdateUI();
    }


    public bool SpendFood(int amount)
    {
        if (amount <= 0) return true;

        if (currentFood >= amount)
        {
            currentFood -= amount;
            UpdateUI();
            return true;
        }
        else
        {
            // Erzak yetersiz! (Uyarı mesajını dışarıdaki event koduna da bırakabilirsin)
            return false;
        }
    }

    public void UpdateUI() 
    {
        OnFoodChanged?.Invoke();
    }
}