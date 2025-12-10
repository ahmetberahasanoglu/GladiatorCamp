using UnityEngine;
using System.Collections.Generic;
using System;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    public int currentDay = 1;

    [Header("Olay Sistemi")]
    public List<GameEvent> possibleEvents; 
    [Range(0, 100)] public int eventChance = 10; 

    public event Action OnNewDay; 
    
    public event Action<int> OnDayChanged;
    public event Action<GameEvent> OnEventTriggered;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        OnDayChanged?.Invoke(currentDay);
    }


    public void NextDay()
    {
        currentDay++;
        
        // Bir şeyler eklenebilir (Yemek yensin, askerler iyileşsin vs.)
        OnNewDay?.Invoke();
        OnDayChanged?.Invoke(currentDay);
        Debug.Log($"Gün bitti. Yeni Gün: {currentDay}");
        CheckForRandomEvent();
        CheckForUlufe();
      //  SaveManager.Instance.SaveGame();//Autosave ekledik
        MoneyManager.Instance.EndOfDay();
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
        
        if (e.moraleChange != 0)
        {
            foreach(var s in FindObjectsOfType<Gladiator>())
            {
                if(e.moraleChange < 0) s.DecreaseMorale(Mathf.Abs(e.moraleChange));
                else s.data.morale += e.moraleChange; 
            }
        }
        OnEventTriggered?.Invoke(e);
        Debug.Log($"OLAY VAR: {e.title}");
    }

    void CheckForUlufe()
    {
        // Osmanlı'da Ulufe 3 ayda bir dağıtılırdı
        if (currentDay % 90 == 0)
        {
            Debug.Log("ULUFE GÜNÜ GELDİ!");
            // Buraya özel bir Ulufe Eventi tetiklyicez
        }
    }
    public void RefreshUI()
    {
        // Mevcut günü UI'a zorla gönder
        OnDayChanged?.Invoke(currentDay);
    }
}