using UnityEngine;
using System.Collections.Generic;

public class SupplyManager : MonoBehaviour
{
    public static SupplyManager Instance;

    [Header("Erzak Durumu")]
    public int currentFood = 50;   // Başlangıç erzağı
    public int foodCost = 5;       // 1 birim yemek kaç akçe?
    public int foodPerSoldier = 1; // Her asker günde kaç birim yer?

    // UI güncellemeleri için event
    public event System.Action OnFoodChanged;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Gün döngüsüne abone ol
        if (DayManager.Instance != null)
            DayManager.Instance.OnNewDay += ConsumeDailyFood;
        
        UpdateUI(); // Başlangıçta UI'ı tetikle
    }

    void OnDestroy()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.OnNewDay -= ConsumeDailyFood;
    }

    // HER GÜN ÇALIŞACAK OLAN FONKSİYON
    void ConsumeDailyFood()
    {
        // 1. Sahnedeki askerleri say (Sadece bizimkileri)
        // (İleride düşmanlar olursa tag kontrolü eklersin)
        Gladiator[] soldiers = FindObjectsOfType<Gladiator>();
        int soldierCount = soldiers.Length;

        if (soldierCount == 0) return; // Asker yoksa yemek gitmez

        int neededFood = soldierCount * foodPerSoldier;

        if (currentFood >= neededFood)
        {
            // Durum İYİ: Herkes doydu
            currentFood -= neededFood;
            Debug.Log($"Bugün {neededFood} birim erzak tüketildi.");
        }
        else
        {
            // Durum KÖTÜ: Yemek yetmedi!
            currentFood = 0; // Kalan kırıntıları da yerler
            Debug.Log("<color=red>ERZAK BİTTİ! Askerler huzursuz!</color>");

            // CEZA: Herkesin morali düşer
            foreach (var soldier in soldiers)
            {
                soldier.DecreaseMorale(10); // Her gün 10 moral kaybı
            }
        }

        UpdateUI();
    }

    // Marketten Yemek Alma Fonksiyonu
    public void BuyFood(int amount)
    {
        int totalCost = amount * foodCost;

        if (MoneyManager.Instance.gold >= totalCost)
        {
            MoneyManager.Instance.Spend(totalCost);
            currentFood += amount;
            
            Debug.Log($"{amount} birim erzak alındı.");
            UpdateUI();
        }
        else
        {
            Debug.Log("Yemek için yeterli akçe yok!");
        }
    }

    private void UpdateUI()
    {
        OnFoodChanged?.Invoke();
    }
}