using UnityEngine;
using System.Collections.Generic;

public class SupplyManager : MonoBehaviour
{
    public static SupplyManager Instance;

    [Header("Erzak Durumu")]
    public int currentFood = 50;   
    public int foodCost = 5;       
    public int baseFoodPerSoldier = 1; // Seviye 1 askerin yediği miktar

    public event System.Action OnFoodChanged;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnNewDay += ConsumeDailyFood;
        
        UpdateUI(); 
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnNewDay -= ConsumeDailyFood;
    }

    // --- YENİ: GİDER HESAPLAYICI (UI DA BUNU KULLANACAK) ---
    public int GetExpectedDailyFoodCost()
    {
        int totalFoodNeeded = 0;
        Gladiator[] soldiers = FindObjectsByType<Gladiator>(FindObjectsSortMode.None);

        foreach (var soldier in soldiers)
        {
            GladiatorAI ai = soldier.GetComponent<GladiatorAI>();
            // Sadece bizim askerimizse ve hayattaysa hesapla
            if (soldier.CompareTag("MySoldier") && (ai == null || !ai.isDead))
            {
                int lvl = (soldier.data != null) ? soldier.data.level : 1;
                // Formül: Askerin seviyesi kadar yemek yer (Lvl 1 = 1 erzak, Lvl 5 = 5 erzak)
                totalFoodNeeded += (baseFoodPerSoldier * lvl); 
            }
        }
        return totalFoodNeeded;
    }

    void ConsumeDailyFood()
    {
        int neededFood = GetExpectedDailyFoodCost();

        if (neededFood == 0) return; // Asker yoksa yemek gitmez

        if (currentFood >= neededFood)
        {
            currentFood -= neededFood;
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show($"Bugün {neededFood} birim erzak tüketildi.", NotificationType.Info);
        }
        else
        {
            currentFood = 0; 
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.Show("<color=red>ERZAK BİTTİ! Askerler açlıktan bitkin düştü!</color>", NotificationType.Warning);

            // Opsiyonel Ceza (İleride açarsın): CampMoraleManager.Instance.ChangeMorale(-10);
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
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show($"{amount} birim erzak alındı.", NotificationType.Info);
            UpdateUI();
        }
        else
        {
            if (NotificationManager.Instance != null) NotificationManager.Instance.Show("Yemek için yeterli akçe yok!", NotificationType.Error);
        }
    }

    public void UpdateUI() // UI'ı dışarıdan (Ordu değiştiğinde) tetiklemek için public yaptık
    {
        OnFoodChanged?.Invoke();
    }
}