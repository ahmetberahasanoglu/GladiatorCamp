using UnityEngine;
using System.Collections.Generic;
using System;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    public int currentDay = 1;
    public int maxDays = 100;
    [Header("Olay Sistemi")]
    public List<GameEvent> possibleEvents; 
    [Range(0, 100)] public int eventChance = 10; 

    public event Action OnNewDay; 
    
    public event Action<int> OnDayChanged;
    public event Action<GameEvent> OnEventTriggered;
    public event System.Action OnWinterArrived;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        OnDayChanged?.Invoke(currentDay);
    }


    public void NextDay(int amount)
    {
        currentDay+=amount;
        
        // Bir şeyler eklenebilir (Yemek yensin, askerler iyileşsin vs.)
        OnNewDay?.Invoke();
        OnDayChanged?.Invoke(currentDay);
        HealAllSoldiers(20 * amount);
        // NotificationManager.Instance.Show($"Gün bitti. Yeni Gün: {currentDay}", NotificationType.Success);
        CheckForRandomEvent();
        //CheckForUlufe();
      //  SaveManager.Instance.SaveGame();//Autosave ekledik
        MoneyManager.Instance.EndOfDay();   
        if (currentDay >= maxDays)
        {
            Debug.Log("KIŞ GELDİ! OYUN BİTTİ.");
            OnWinterArrived?.Invoke();
            // GameOverManager.Instance.TriggerWinterDefeat();
        }
    }
    public void HealAllSoldiers(int percentageAmount)
    {
        var soldiers = FindObjectsOfType<Gladiator>();
        
        foreach(var soldier in soldiers)
        {
            // Ne kadar iyileşecek? (Max canın %'si kadar)
            float healAmount = soldier.maxHealth * (percentageAmount / 100.0f);
            
            soldier.currentHealth += healAmount;
            
            // Max canı geçmesin
            if(soldier.currentHealth > soldier.maxHealth) 
                soldier.currentHealth = soldier.maxHealth;

            // Barı güncelle
            if(soldier.healthBar != null)
                soldier.healthBar.UpdateBar(soldier.currentHealth, soldier.maxHealth);
        }
        NotificationManager.Instance.Show($"Tüm ordu %{percentageAmount} iyileşti.", NotificationType.Success);
        //Debug.Log($"Tüm ordu %{percentageAmount} iyileşti.");
    }
    void CheckForRandomEvent()
    {
        if (possibleEvents.Count == 0) return;

        if (UnityEngine.Random.Range(0, 100) < eventChance)
        {
            GameEvent randomEvent = possibleEvents[UnityEngine.Random.Range(0, possibleEvents.Count)];
            TriggerEvent(randomEvent);
        }
    }

    void TriggerEvent(GameEvent e)
    {
        
        if (e.moneyChange != 0) MoneyManager.Instance.Add(e.moneyChange); 
        if (e.foodChange != 0) SupplyManager.Instance.BuyFood(e.foodChange); 
        /*
        if (e.moraleChange != 0)
        {
            foreach(var s in FindObjectsOfType<Gladiator>())
            {
                if(e.moraleChange < 0) s.DecreaseMorale(Mathf.Abs(e.moraleChange));
                else s.data.morale += e.moraleChange; 
            }
        } buraya direkt genel moral değişimi yapıcaz*/
        OnEventTriggered?.Invoke(e);
        Debug.Log($"OLAY VAR: {e.title}");
    }

    
    public void RefreshUI()
    {
        // Mevcut günü UI'a zorla gönder
        OnDayChanged?.Invoke(currentDay);
    }
}